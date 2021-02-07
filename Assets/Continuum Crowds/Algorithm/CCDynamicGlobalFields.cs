using UnityEngine;
using System;
using System.Collections.Generic;

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

    // update the unit specific elements (rho, vAve, g_P)
    for (int i = 0; i < _units.Count; i++) {
      // (1) density field and velocity
      computeDensityField(_units[i]);
      // ******************************************************************************************
      // 		PREDICTIVE DISCOMFORT is being phased out due to visually
      //		unappealing behaviour - units would 'dodge/weave' to avoid
      //								their own predictive dicomfort
      //
      //    TODO: Replace with predictive velocity field.
      //
      //			// predictive discomfort is only applied to moving units
      //  if (_units[i].GetVelocity () != Vector2.zero)
      //  {
      //     // (2) predictive discomfort field
      //     applyPredictiveDiscomfort (CCvals.g_predictiveSeconds, _units[i]);
      //  }
      // ******************************************************************************************
    }

    // these next values are derived from rho, vAve, and g_P, so we simply iterate
    // through the tiles and ONLY update the ones that have had their values changed
    foreach (CC_Tile cct in _tiles.Values) {
      //if (cct.UPDATE_TILE) {
      // (3) 	now that the velocity field and density fields are implemented,
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
    // grab the NxN footprint matrix
    var footprint = ccu.GetFootprint();
    var anchor = ccu.GetPosition();

    // cache the x - offset
    int xOffset = ccu.sizeX % 2 == 0 ?
        // if even, use Math.Round
        (int)Math.Round(anchor.x) :
        // is odd, use Math.Floor
        (int)Math.Floor(anchor.x);

    // cache y - offset
    int yOffset = ccu.sizeY % 2 == 0 ?
        // if even, use Math.Round
        (int)Math.Round(anchor.y) :
        // is odd, use Math.Floor
        (int)Math.Floor(anchor.y);

    // cache iterators
    int xIndex, yIndex;
    // scan the grid, stamping the footprint onto the tile
    for (int x = 0; x < footprint.GetLength(0); x++) {
      for (int y = 0; y < footprint.GetLength(1); y++) {
        // get the rho value
        float rho = footprint[x, y];
        float rt = 0f;

        xIndex = x + xOffset;
        yIndex = y + yOffset;

        if (isPointValid(xIndex, yIndex)) {
          rt = readDataFromPoint_rho(xIndex, yIndex);
          writeDataToPoint_rho(xIndex, yIndex, rho + rt);
        }
        // compute velocity field with this density
        computeVelocityFieldPoint(xIndex, yIndex, ccu.GetVelocity(), rt);
      }
    }
  }

  private void computeVelocityFieldPoint(int x, int y, Vector2 v, float rho)
  {
    Vector2 vAve = readDataFromPoint_vAve(x, y);
    if (isPointValid(x, y)) {
      vAve += v * rho;
      writeDataToPoint_vAve(x, y, vAve);
    }
  }

  // **********************************************************************
  // 		PREDICTIVE DISCOMFORT - replace wth predictive velocity fields
  // **********************************************************************
  //	private void applyPredictiveDiscomfort (float numSec, CC_Unit cc_u)
  //	{
  //		Vector2 newLoc;
  //		float sc;
  //
  //		Vector2[] cc_u_pos = cc_u.getPositions ();
  //
  //		for (int k = 0; k < cc_u_pos.Length; k++) {
  //
  //			Vector2 xprime = cc_u_pos[k]  + cc_u.getVelocity () * numSec;
  //
  //			float vfMag = Vector2.Distance (cc_u_pos[k], xprime);
  //
  //			for (int i = 5; i < vfMag; i++) {
  //				newLoc = Vector2.MoveTowards (cc_u_pos[k], xprime, i);
  //
  //				sc = (vfMag - i) / vfMag;				// inverse scale
  //				float[,] g = linear1stOrderSplat (newLoc, sc * CCvals.g_weight);
  //
  //				int xInd = (int)Math.Floor((double)newLoc.x);
  //				int yInd = (int)Math.Floor((double)newLoc.y);
  //
  //				if (isPointValid (xInd, yInd)) {
  //					float gt = readDataFromPoint_g (xInd, yInd);
  //					writeDataToPoint_g (xInd, yInd, gt + g [0, 0]);
  //				}
  //				if (isPointValid (xInd + 1, yInd)) {
  //					float gt = readDataFromPoint_g (xInd + 1, yInd);
  //					writeDataToPoint_g (xInd + 1, yInd, gt + g [1, 0]);
  //				}
  //				if (isPointValid (xInd, yInd + 1)) {
  //					float gt = readDataFromPoint_g (xInd, yInd + 1);
  //					writeDataToPoint_g (xInd, yInd + 1, gt +  g [0, 1]);
  //				}
  //				if (isPointValid (xInd + 1, yInd + 1)) {
  //					float gt = readDataFromPoint_g (xInd + 1, yInd + 1);
  //					writeDataToPoint_g (xInd + 1, yInd + 1, gt + g [1, 1]);
  //				}
  //			}
  //		}
  //	}

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
    // INTO WHICH we are looking,
    // dotted with the direction vector
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
    //float rho;

    // test to see if the point we're looking INTO is in a DIFFERENT tile, and if so, pull it
    if (xLocalInto < 0 || xLocalInto > tileSize - 1 ||
        yLocalInto < 0 || yLocalInto > tileSize - 1
    ) {
      g += readDataFromPoint_g(xGlobalInto, yGlobalInto);
      //rho = readDataFromPoint_rho(xGlobalInto, yGlobalInto);
    } else {
      g += cct.g[xLocalInto, yLocalInto];
      //rho = cct.rho[xLocalInto, yLocalInto];
    }

    // clamp g to make sure it's not > 1
    if (g > 1) { g = 1; } else if (g < 0) { g = 0; }

    // compute the cost weighted by our coefficients
    float cost = (CCValues.S.C_alpha * cct.f[tileX, tileY][d]
                + CCValues.S.C_beta
                + CCValues.S.C_gamma * g
                /*+ CCValues.Instance.C_delta * rho*/)
                    / cct.f[tileX, tileY][d];

    return cost;
  }

  // *****************************************************************************
  //			TOOLS AND UTILITIES
  // *****************************************************************************
  public bool isPointValid(int globalX, int globalY)
  {
    // check to make sure the point is not outside the tile
    if (globalX < 0 || globalY < 0 || globalX > tileSize - 1 || globalY > tileSize - 1) {
      return false;
    }
    // check to make sure the point is not on a place of absolute discomfort (like inside a building)
    // check to make sure the point is not in a place dis-allowed by terrain (slope)
    if (readDataFromPoint_g(globalX, globalY) >= 1) {
      return false;
    }
    return true;
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
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    return _tiles[l].readData_h(xTile, yTile);
  }

  private Vector2 readDataFromPoint_dh(int xGlobal, int yGlobal)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    return _tiles[l].readData_dh(xTile, yTile);
  }

  private float readDataFromPoint_rho(int xGlobal, int yGlobal)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    return _tiles[l].readData_rho(xTile, yTile);
  }

  private float readDataFromPoint_g(int xGlobal, int yGlobal)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    return _tiles[l].readData_g(xTile, yTile);
  }

  private Vector2 readDataFromPoint_vAve(int xGlobal, int yGlobal)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    return _tiles[l].readData_vAve(xTile, yTile);
  }

  private Vector4 readDataFromPoint_f(int xGlobal, int yGlobal)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    return _tiles[l].readData_f(xTile, yTile);
  }

  private Vector4 readDataFromPoint_C(int xGlobal, int yGlobal)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    return _tiles[l].readData_C(xTile, yTile);
  }

  // *** write ops ***
  private void writeDataToPoint_rho(int xGlobal, int yGlobal, float val)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    _tiles[l].writeData_rho(xTile, yTile, val);
  }

  private void writeDataToPoint_g(int xGlobal, int yGlobal, float val)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    _tiles[l].writeData_g(xTile, yTile, val);
  }

  private void writeDataToPoint_vAve(int xGlobal, int yGlobal, Vector2 val)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    _tiles[l].writeData_vAve(xTile, yTile, val);
  }

  private void writeDataToPoint_f(int xGlobal, int yGlobal, Vector4 val)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    _tiles[l].writeData_f(xTile, yTile, val);
  }

  private void writeDataToPoint_C(int xGlobal, int yGlobal, Vector4 val)
  {
    var l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    _tiles[l].writeData_C(xTile, yTile, val);
  }
}
