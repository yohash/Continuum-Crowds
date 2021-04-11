using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Continuum Crowds dynamic global fields.
/// </summary>
[System.Serializable]
public class CCDynamicGlobalFields
{
  public int tileSize;

  // the meat of the CC Dynamic Global Fields computer
  private Dictionary<Location, CC_Tile> _tiles;
  private List<CC_Unit> _units;

  //// map dimensions
  //private int _mapX, _mapY;

  // cached 2x2 float[] for linear1stOrderSplat (GC redux)
  private float[,] mat = new float[2, 2];

  // ****************************************************************
  // 				PUBLIC ACCESSORS and CONSTRUCTORS
  // ****************************************************************
  public CCDynamicGlobalFields(List<MapTile> tiles)
  {
    _tiles = new Dictionary<Location, CC_Tile>();
    _units = new List<CC_Unit>();

    initiateTiles(tiles);
  }

  private bool initiateTiles(List<MapTile> tiles)
  {
    if (tiles.Count == 0) { throw new NavSystemException("CCDynamicGlobalFields: No Tiles"); }

    tileSize = tiles[0].TileSize;
    // instantiate all our tiles
    for (int i = 0; i < tiles.Count; i++) {
      // create a new tile based on this location
      var cct = new CC_Tile(tiles[i]);
      // save the tile
      _tiles.Add(cct.Corner, cct);
    }


    // initialize some tile values
    foreach (CC_Tile cct in _tiles.Values) {
      // initialize the cost and speed field
      computeSpeedField(cct);
      computeCostField(cct);
      // store the current data in memory
      cct.StoreCurrentSpeedAndCostFields();
    }

    return true;
  }

  public List<CC_Tile> GetActiveTiles()
  {
    var activeTiles = new List<CC_Tile>();
    foreach (CC_Tile cct in _tiles.Values) {
      if (cct.UPDATE_TILE) {
        activeTiles.Add(cct);
      }
    }
    return activeTiles;
  }

  public void UpdateTiles()
  {
    // first, clear the tiles
    foreach (CC_Tile cct in _tiles.Values) {
      if (cct.UPDATE_TILE) {
        cct.ResetTile();
      }
    }

    // update the unit specific elements (rho, vAve))
    for (int i = 0; i < _units.Count; i++) {
      // predictive velocity is only applied to moving units
      if (_units[i].Speed() <= 0.25f) {
        // (1) apply stationary unit density field
        computeDensityField(_units[i]);
      } else {
        // (2) moving units apply predictive density/velocity field
        applyPredictiveVelocity(_units[i]);
      }
    }

    // these next values are derived from rho, vAve, and g_P, so we simply iterate
    // through the tiles and ONLY update the ones that have had their values changed
    foreach (CC_Tile cct in _tiles.Values) {
      //if (cct.UPDATE_TILE) {
      // (3) 	now that the velocity field and density fields are computed,
      // 		divide the velocity by density to get average velocity field
      computeAverageVelocityField(cct);
      // (4)	now that the average velocity field is computed, and the density
      // 		field is in place, we calculate the speed field, f
      computeSpeedField(cct);
      // (5) 	the cost field depends only on f and g, so it can be computed in its
      //		entirety now as well
      computeCostField(cct);
      //}
    }
  }

  public void AddNewCCUnit(CC_Unit ccu)
  {
    _units.Add(ccu);
  }

  public void RemoveCCUnit(CC_Unit ccu)
  {
    if (_units.Contains(ccu)) {
      _units.Remove(ccu);
    }
  }

  public CC_Tile GetCCTile(Location corner)
  {
    return _tiles[corner];
  }

  private void purgeUnits()
  {
    var remove = new List<CC_Unit>();
    for (int i = 0; i < _units.Count; i++) {
      if (_units[i] == null) {
        remove.Add(_units[i]);
      }
    }
    for (int i = 0; i < remove.Count; i++) {
      RemoveCCUnit(remove[i]);
    }
  }

  // ******************************************************************************************
  // 							FIELD SOLVING FUNCTIONS
  // ******************************************************************************************
  private void computeDensityField(CC_Unit ccu)
  {
    // grab properties of the CC Unit
    var footprint = ccu.GetFootprint().Rotate(-ccu.Rotation());
    var anchor = ccu.Position();

    // compute the footprint's half-dimensions
    var xHalf = footprint.GetLength(0) / 2f;
    var yHalf = footprint.GetLength(1) / 2f;
    // translate the anchor so it's centered about our unit
    anchor -= new Vector2(xHalf, yHalf);
    // finally, perform bilinear interpolation of the footprint at our anchor
    var footprintInterp = footprint.BilinearInterpolation(anchor);

    // offsets - floor produces smoothest interpolated position stamps
    var xOffset = Mathf.FloorToInt(anchor.x);
    var yOffset = Mathf.FloorToInt(anchor.y);

    // scan the grid, stamping the footprint onto the tile
    for (int x = 0; x < footprintInterp.GetLength(0); x++) {
      for (int y = 0; y < footprintInterp.GetLength(1); y++) {
        // get the rho value
        float rho = footprintInterp[x, y];
        // only perform storage functions if there is a density value
        if (rho > 0) {
          var xIndex = x + xOffset;
          var yIndex = y + yOffset;
          // add rho to the in-place density
          addDataToPoint_rho(xIndex, yIndex, rho);
        }
      }
    }
  }

  // **********************************************************************
  // 		Predictive velocity fields
  // **********************************************************************
  private void applyPredictiveVelocity(CC_Unit ccu)
  {
    // TODO: Only apply predictive velocity to continuous segments, ie.
    //      if a portion of this predictive path is blocked by impassable
    //      terrain, we should not apply predictive velocity beyond that
    //      point

    // fetch unit properties
    var speed = ccu.Speed();

    // compute values
    var distance = (int)Math.Ceiling(speed * CCValues.S.v_predictiveSeconds);
    var footprint = ccu.GetFootprint();
    var height = footprint.GetLength(1);

    // (1) create a rect with Length = predictive distance, Height = Unit footprint height
    //var footprintEnd = (int)Math.Floor(footprint.GetLength(0) / 2f);
    var footprintEnd = Mathf.FloorToInt(ccu.Falloff() + ccu.SizeX - 1);
    var predictive = new float[footprintEnd + distance, height];

    // (2) build half of the footprint into the predictive rect
    for (int i = 0; i < footprintEnd; i++) {
      for (int k = 0; k < height; k++) {
        predictive[i, k] = footprint[i, k];
      }
    }

    // (3a) record the "vertical slice" of the unit footprint
    var slice = new float[height];
    for (int i = 0; i < slice.Length; i++) {
      slice[i] = footprint[footprintEnd, i];
    }

    // (3b) scale the vertical slice along the length of the rect
    // determine falloff rates
    var start = 1;
    var end = CCValues.S.f_rhoMin;
    // track iteration
    int c = 0;
    for (int i = footprintEnd; i < predictive.GetLength(0); i++) {
      // taper from <start> down to <end>
      var scalar = (end - start) / distance * c + start;
      c++;
      for (int k = 0; k < height; k++) {
        // build the predictive velocity rect in front of the footprint
        predictive[i, k] = slice[k] * scalar;
      }
    }

    // (4) rotate the rect
    var yEuler = ccu.Rotation();
    // Unity y-euler rotations start at +z (+y in 2D) and move CW.
    // Academic rotations are described as CCW from the +x axis, which is what
    // many of our derivations are based, so we convert here.
    var degrees = Mathf.Repeat(90 - yEuler, 360);
    var radians = degrees * Mathf.Deg2Rad;
    var rotated = predictive.Rotate(degrees);

    // (5) determine anchor position - do this by taking the "perfect" center
    //     and applying the same translations/rotations that our rotate process
    //     applies
    //   (i) declare unit location in original footprint center
    var unitOffset = new Vector2(footprint.GetLength(0) / 2f, height / 2f);
    //   (ii) translate by predictive velocity half-shape to center on (0,0)
    unitOffset += new Vector2(-predictive.GetLength(0) / 2f, -height / 2f);
    //   (iii) rotate the point about (0,0) by our unit's rotation
    unitOffset = unitOffset.Rotate(radians);
    //   (iv) translate back by rotated shape half-space
    unitOffset += new Vector2(rotated.GetLength(0) / 2f, rotated.GetLength(1) / 2f);

    // finally, translate the anchor to be positioned on the unit
    var anchor = ccu.Position() - unitOffset;

    // (6) inteprolate the final result
    var final = rotated.BilinearInterpolation(anchor);

    // offsets - floor produces smoothest interpolated position stamps
    var xOffset = Mathf.FloorToInt(anchor.x);
    var yOffset = Mathf.FloorToInt(anchor.y);

    // (7) add the density and velocity along the length of the path, 
    // scaling each by the value of the rect
    var direction = new Vector2(1, 0).Rotate(radians);
    var velocity = direction * speed;
    for (int x = 0; x < final.GetLength(0); x++) {
      for (int y = 0; y < final.GetLength(1); y++) {
        var xIndex = x + xOffset;
        var yIndex = y + yOffset;
        // add rho and velocity to existing data
        addDataToPoint_rho(xIndex, yIndex, final[x, y]);
        addDataToPoint_vAve(xIndex, yIndex, final[x, y] * velocity);
      }
    }
  }

  // average velocity fields will just iterate over each tile, since information
  // doesnt 'bleed' into or out from nearby tiles
  private void computeAverageVelocityField(CC_Tile cct)
  {
    for (int n = 0; n < tileSize; n++) {
      for (int m = 0; m < tileSize; m++) {
        var v = cct.vAve[n, m];
        float r = cct.rho[n, m];

        if (r != 0) {
          v /= r;
        }
        cct.vAve[n, m] = v;
      }
    }
  }

  private void computeSpeedField(CC_Tile cct)
  {
    for (int n = 0; n < tileSize; n++) {
      for (int m = 0; m < tileSize; m++) {
        for (int d = 0; d < CCValues.S.ENSW.Length; d++) {
          cct.f[n, m][d] = computeSpeedFieldPoint(n, m, cct, CCValues.S.ENSW[d]);
        }
      }
    }
  }

  private float computeSpeedFieldPoint(int tileX, int tileY, CC_Tile cct, Vector2 direction)
  {
    int xLocalInto = tileX + (int)direction.x;
    int yLocalInto = tileY + (int)direction.y;

    int xGlobalInto = cct.Corner.x * tileSize + xLocalInto;
    int yGlobalInto = cct.Corner.y * tileSize + yLocalInto;

    // otherwise, run the speed field calculation
    float ff, ft, fv;

    float r;
    // test to see if the point we're looking INTO is in another tile, and if so, pull it
    if (xLocalInto < 0 || xLocalInto > tileSize - 1 || yLocalInto < 0 || yLocalInto > tileSize - 1) {
      // if we're looking off the map, dont store this value
      if (!isPointValid(xGlobalInto, yGlobalInto)) {
        return CCValues.S.f_speedMin;
      }
      r = readDataFromPoint_rho(xGlobalInto, yGlobalInto);
    } else {
      r = cct.rho[xLocalInto, yLocalInto];
    }

    // test the density INTO WHICH we move:
    if (r < CCValues.S.f_rhoMin) {
      // rho < rho_min calc
      ft = computeTopographicalSpeed(readDataFromPoint_dh(xGlobalInto, yGlobalInto), direction);
      ff = ft;
    } else if (r > CCValues.S.f_rhoMax) {
      // rho > rho_max calc
      fv = computeFlowSpeed(xGlobalInto, yGlobalInto, direction);
      ff = fv;
    } else {
      // rho in-between calc
      fv = computeFlowSpeed(xGlobalInto, yGlobalInto, direction);
      ft = computeTopographicalSpeed(readDataFromPoint_dh(xGlobalInto, yGlobalInto), direction);
      ff = ft + (r - CCValues.S.f_rhoMin) / (CCValues.S.f_rhoMax - CCValues.S.f_rhoMin) * (fv - ft);
    }
    ff = Mathf.Clamp(ff, CCValues.S.f_speedMin, CCValues.S.f_speedMax);
    return Math.Max(CCValues.S.f_speedMin, ff);
  }

  private float computeTopographicalSpeed(Vector2 dh, Vector2 direction)
  {
    // first, calculate the gradient in the direction we are looking.
    // By taking the dot with Direction,
    // we extract the direction we're looking and assign it a proper sign
    // i.e. if we look left (x = -1) we want -dhdx(x,y), because the
    // gradient is assigned with a positive x
    // 		therefore:		also, Vector.left = [-1,0]
    //						Vector2.Dot(Vector.left, dh[x,y]) = -dhdx;
    float dhInDirection = direction.x * dh.x + direction.y * dh.y;
    // calculate the speed field from the equation
    return CCValues.S.f_speedMax
        + (dhInDirection - CCValues.S.f_slopeMin) / (CCValues.S.f_slopeMax - CCValues.S.f_slopeMin)
        * (CCValues.S.f_speedMin - CCValues.S.f_speedMax);
  }

  private float computeFlowSpeed(int xI, int yI, Vector2 direction)
  {
    // the flow speed is simply the average velocity field of the region
    // INTO WHICH we are looking, dotted with the direction vector
    var vAvePt = readDataFromPoint_vAve(xI, yI);
    float dot = vAvePt.x * direction.x + vAvePt.y * direction.y;
    return Math.Max(CCValues.S.f_speedMin, dot);
  }

  private void computeCostField(CC_Tile cct)
  {
    for (int n = 0; n < tileSize; n++) {
      for (int m = 0; m < tileSize; m++) {
        for (int d = 0; d < CCValues.S.ENSW.Length; d++) {
          cct.C[n, m][d] = computeCostFieldValue(n, m, d, CCValues.S.ENSW[d], cct);
        }
      }
    }
  }

  private float computeCostFieldValue(int tileX, int tileY, int d, Vector2 direction, CC_Tile cct)
  {
    int xLocalInto = tileX + (int)direction.x;
    int yLocalInto = tileY + (int)direction.y;

    int xGlobalInto = cct.Corner.x * tileSize + xLocalInto;
    int yGlobalInto = cct.Corner.y * tileSize + yLocalInto;

    // if we're looking in an invalid direction, dont store this value
    if (cct.f[tileX, tileY][d] == 0 || !isPointValid(xLocalInto, yLocalInto)) {
      return Mathf.Infinity;
    }

    // initialize g as the map discomfort data value
    float g = readDataFromPoint_g(xGlobalInto, yGlobalInto);

    // test to see if the point we're looking INTO is in a DIFFERENT tile, and if so, pull it
    if (xLocalInto < 0 || xLocalInto > tileSize - 1 ||
        yLocalInto < 0 || yLocalInto > tileSize - 1
    ) {
      g += readDataFromPoint_g(xGlobalInto, yGlobalInto);
    } else {
      g += cct.g[xLocalInto, yLocalInto];
    }

    // clamp g to make sure it's not > 1
    if (g > 1) { g = 1; } else if (g < 0) { g = 0; }

    // compute the cost weighted by our coefficients
    var f = cct.f[tileX, tileY][d];
    float cost = CCValues.S.C_alpha
                + CCValues.S.C_beta * 1 / f
                + CCValues.S.C_gamma * g / f;

    return cost;
  }

  // *****************************************************************************
  //			TOOLS AND UTILITIES
  // *****************************************************************************
  private bool isPointValid(int xGlobal, int yGlobal)
  {
    // check to make sure the point is not outside the tile
    if (xGlobal < 0 || yGlobal < 0 || xGlobal > tileSize - 1 || yGlobal > tileSize - 1) {
      return false;
    }
    // check to make sure the point is not on a place of absolute discomfort (like inside a building)
    // check to make sure the point is not in a place dis-allowed by terrain (slope)
    if (readDataFromPoint_g(xGlobal, yGlobal) >= 1) {
      return false;
    }
    return true;
  }

  private Location tileCornerFromGlobalCoords(int xGlobal, int yGlobal)
  {
    return new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
  }

  private Location tileCoordsFromGlobal(Location l, int xGlobal, int yGlobal)
  {
    return new Location(
        xGlobal - l.x * tileSize,
        yGlobal - l.y * tileSize
    );
  }

  // ******************************************************************************************
  //                 TILE READ/WRITE OPS
  //
  //  Primary focus of this area is to convert global points (what CC Dynamic Global Fields
  //  works with) into local points, and then find the relevant tile
  // ******************************************************************************************
  // *** read ops ***
  private float readDataFromPoint_h(int xGlobal, int yGlobal)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    return _tiles[l].readData_h(tileCoordsFromGlobal(l, xGlobal, yGlobal));
  }

  private Vector2 readDataFromPoint_dh(int xGlobal, int yGlobal)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    return _tiles[l].readData_dh(tileCoordsFromGlobal(l, xGlobal, yGlobal));
  }

  private float readDataFromPoint_rho(int xGlobal, int yGlobal)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    return _tiles[l].readData_rho(tileCoordsFromGlobal(l, xGlobal, yGlobal));
  }

  private float readDataFromPoint_g(int xGlobal, int yGlobal)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    return _tiles[l].readData_g(tileCoordsFromGlobal(l, xGlobal, yGlobal));
  }

  private Vector2 readDataFromPoint_vAve(int xGlobal, int yGlobal)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    return _tiles[l].readData_vAve(tileCoordsFromGlobal(l, xGlobal, yGlobal));
  }

  private Vector4 readDataFromPoint_f(int xGlobal, int yGlobal)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    return _tiles[l].readData_f(tileCoordsFromGlobal(l, xGlobal, yGlobal));
  }

  private Vector4 readDataFromPoint_C(int xGlobal, int yGlobal)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    return _tiles[l].readData_C(tileCoordsFromGlobal(l, xGlobal, yGlobal));
  }

  // *** write ops ***
  private void writeDataToPoint_rho(int xGlobal, int yGlobal, float val)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    _tiles[l].writeData_rho(tileCoordsFromGlobal(l, xGlobal, yGlobal), val);
  }

  private void writeDataToPoint_g(int xGlobal, int yGlobal, float val)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    _tiles[l].writeData_g(tileCoordsFromGlobal(l, xGlobal, yGlobal), val);
  }

  private void writeDataToPoint_vAve(int xGlobal, int yGlobal, Vector2 val)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    _tiles[l].writeData_vAve(tileCoordsFromGlobal(l, xGlobal, yGlobal), val);
  }

  private void writeDataToPoint_f(int xGlobal, int yGlobal, Vector4 val)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    _tiles[l].writeData_f(tileCoordsFromGlobal(l, xGlobal, yGlobal), val);
  }

  private void writeDataToPoint_C(int xGlobal, int yGlobal, Vector4 val)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    _tiles[l].writeData_C(tileCoordsFromGlobal(l, xGlobal, yGlobal), val);
  }

  // *** additive write ops ***
  private void addDataToPoint_rho(int xGlobal, int yGlobal, float val)
  {
    if (isPointValid(xGlobal, yGlobal)) {
      // add rho to the in-place density
      var t = readDataFromPoint_rho(xGlobal, yGlobal);
      writeDataToPoint_rho(xGlobal, yGlobal, val + t);
    }
  }

  private void addDataToPoint_g(int xGlobal, int yGlobal, float val)
  {
    if (isPointValid(xGlobal, yGlobal)) {
      // add rho to the in-place density
      var rt = readDataFromPoint_g(xGlobal, yGlobal);
      writeDataToPoint_g(xGlobal, yGlobal, val + rt);
    }
  }

  private void addDataToPoint_vAve(int xGlobal, int yGlobal, Vector2 val)
  {
    if (isPointValid(xGlobal, yGlobal)) {
      // add rho to the in-place density
      var rt = readDataFromPoint_vAve(xGlobal, yGlobal);
      writeDataToPoint_vAve(xGlobal, yGlobal, val + rt);
    }
  }

  private void addDataToPoint_f(int xGlobal, int yGlobal, Vector4 val)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    _tiles[l].writeData_f(tileCoordsFromGlobal(l, xGlobal, yGlobal), val);
  }

  private void addDataToPoint_C(int xGlobal, int yGlobal, Vector4 val)
  {
    var l = tileCornerFromGlobalCoords(xGlobal, yGlobal);
    _tiles[l].writeData_C(tileCoordsFromGlobal(l, xGlobal, yGlobal), val);
  }
}
