using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Continuum Crowds dynamic global fields.
/// </summary>
[System.Serializable]
public class CCDynamicGlobalFields
{
  public int tileSize = 20;

  // the meat of the CC Dynamic Global Fields computer
  private Dictionary<Location, CC_Tile> _tiles;
  [SerializeField] private List<CC_Unit> _units;

  // map dimensions
  private int _mapX, _mapY;

  bool isDone = false;

  // cached 2x2 float[] for linear1stOrderSplat (GC redux)
  private float[,] mat = new float[2, 2];

  // *************************************************************************
  //    THREAD OPERATIONS
  // *************************************************************************
  public void Threaded_UpdateTiles()
  {
    isDone = false;

    var task = Task.Run(() => {
      // _updateAll_CCTiles ();
    });
    task.Wait();

    isDone = true;
  }

  public IEnumerator WaitFor()
  {
    while (!tUpdate()) {
      yield return null;
    }
  }

  private bool tUpdate()
  {
    if (isDone) {
      return true;
    }
    return false;
  }

  // ******************************************************************************************
  // 				PUBLIC ACCESSORS and CONSTRUCTORS
  // ******************************************************************************************
  public CCDynamicGlobalFields()
  {
    _tiles = new Dictionary<Location, CC_Tile>(); // (new LocationComparator());
    _units = new List<CC_Unit>();

    //theMapData = new Map_Data_Package();
  }

  public void UpdateCCUnits()
  {
    for (int i = 0; i < _units.Count; i++) {
      _units[i].UpdatePhysics();
    }
  }

  public void SetTileSize(int s)
  {
    tileSize = s;
  }

  public bool InitiateTiles()
  {
    // take map dimensions
    // if tileSize and map dimensions dont fit perfectly, drop a flag
    // otherwise, create all the tiles

    // make sure the map dimensions are divisible by tileSize
    if ((((float)_mapX) % ((float)tileSize) != 0) ||
      (((float)_mapY) % ((float)tileSize) != 0)) {
      // this should NEVER HAPPEN, so send an error if it does
      return false;
    }

    Location loc;

    int numTilesX = _mapX / tileSize;
    int numTilesY = _mapY / tileSize;

    // instantiate all our tiles
    for (int x = 0; x < numTilesX; x++) {
      for (int y = 0; y < numTilesY; y++) {
        // create a new tile based on this location
        loc = new Location(x, y);
        CC_Tile cct = new CC_Tile(tileSize, loc);
        // save the tile
        _tiles.Add(loc, cct);
      }
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
    List<CC_Tile> activeTiles = new List<CC_Tile>();
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
      if (cct.UPDATE_TILE) {
        // (3) 	now that the velocity field and density fields are implemented,
        // 		divide the velocity by density to get average velocity field
        computeAverageVelocityField(cct);
        // (4)	now that the average velocity field is computed, and the density
        // 		field is in place, we calculate the speed field, f
        computeSpeedField(cct);
        // (5) 	the cost field depends only on f and g, so it can be computed in its
        //		entirety now as well
        computeCostField(cct);
      }
    }
  }

  public void AddNewCCUnit(CC_Unit ccu)
  {
    _units.Add(ccu);
  }

  public void RemoveCCUnit(CC_Unit ccu)
  {
    if (_units.Contains(ccu))
      _units.Remove(ccu);
  }

  //public CC_Map_Package BuildCCMapPackage(Rect r)
  //{
  //  float[,] gt;
  //  Vector4[,] ft, Ct;

  //  int xs = (int)Math.Floor((double)r.x);
  //  int ys = (int)Math.Floor((double)r.y);

  //  int xf = (int)Math.Ceiling((double)(r.x + r.width));
  //  int yf = (int)Math.Ceiling((double)(r.y + r.height));

  //  if (xs < 0)
  //    xs = 0;
  //  if (xf > _mapX)
  //    xf = _mapX;
  //  if (ys < 0)
  //    ys = 0;
  //  if (yf > _mapY)
  //    yf = _mapY;

  //  int xdim = xf - xs;
  //  int ydim = yf - ys;

  //  gt = new float[xdim, ydim];
  //  ft = new Vector4[xdim, ydim];
  //  Ct = new Vector4[xdim, ydim];

  //  for (int xI = 0; xI < xdim; xI++)
  //  {
  //    for (int yI = 0; yI < ydim; yI++)
  //    {
  //      gt[xI, yI] = /*theMapData.getDiscomfortMap(xs + xI, ys + yI) +*/ readDataFromPoint_g(xs + xI, ys + yI);
  //      ft[xI, yI] = readDataFromPoint_f(xs + xI, ys + yI);
  //      Ct[xI, yI] = readDataFromPoint_C(xs + xI, ys + yI);
  //    }
  //  }
  //  CC_Map_Package map = new CC_Map_Package(gt, ft, Ct);
  //  return map;
  //}
  // ******************************************************************************************
  // ******************************************************************************************
  // ******************************************************************************************
  // 							FIELD SOLVING FUNCTIONS
  // ******************************************************************************************
  private void computeDensityField(CC_Unit cc_u)
  {
    // grab the NxN footprint matrix
    float[,] cc_u_footprint = cc_u.GetFootprint();
    Vector2 gridAnchor = cc_u.GetAnchorPoint();

    int xOff, yOff;
    int xInd, yInd;

    // cache the x - offset
    if (cc_u.UnitX % 2 == 0) {
      // if even, use Math.Round
      xOff = (int)Math.Round(gridAnchor.x);
    } else {
      // is odd, use Math.Floor
      xOff = (int)Math.Floor(gridAnchor.x);
    }

    // cache y - offset
    if (cc_u.UnitY % 2 == 0) {
      // if even, use Math.Round
      yOff = (int)Math.Round(gridAnchor.y);
    } else {
      // is odd, use Math.Floor
      yOff = (int)Math.Floor(gridAnchor.y);
    }

    // scan the grid, stamping the footprint onto the tile
    for (int x = 0; x < cc_u_footprint.GetLength(0); x++) {
      for (int y = 0; y < cc_u_footprint.GetLength(1); y++) {
        // get the rho value
        float rho = cc_u_footprint[x, y];
        float rt = 0f;

        xInd = x + xOff;
        yInd = y + yOff;

        if (isPointValid(xInd, yInd)) {
          rt = readDataFromPoint_rho(xInd, yInd);
          writeDataToPoint_rho(xInd, yInd, rho + rt);
        }
        // compute velocity field with this density
        computeVelocityFieldPoint(xInd, yInd, cc_u.GetVelocity(), rt);
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
  // 		PREDICTIVE DISCOMFORT is being phased out
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
        Vector2 v = cct.vAve[n, m];
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
        for (int d = 0; d < CCvals.ENSW.Length; d++) {
          cct.f[n, m][d] = computeSpeedFieldPoint(n, m, cct, CCvals.ENSW[d]);
        }
      }
    }
  }

  // IMPORTANT: in this function call, x and y are LOCAL to the tile
  private float computeSpeedFieldPoint(int tileX, int tileY, CC_Tile cct, Vector2 direction)
  {
    int xLocalInto = tileX + (int)direction.x;
    int yLocalInto = tileY + (int)direction.y;

    int xGlobalInto = cct.myLoc.x * tileSize + xLocalInto;
    int yGlobalInto = cct.myLoc.y * tileSize + yLocalInto;

    // otherwise, run the speed field calculation
    float ff = 0, ft = 0, fv = 0;
    float r;
    // test to see if the point we're looking INTO is in another tile, and if so, pull it
    if ((xLocalInto < 0) || (xLocalInto > tileSize - 1) || (yLocalInto < 0) || (yLocalInto > tileSize - 1)) {
      // if we're looking off the map, dont store this value
      if (!isPointValid(xGlobalInto, yGlobalInto)) {
        return CCvals.f_speedMin;
      }
      r = readDataFromPoint_rho(xGlobalInto, yGlobalInto);
    } else {
      r = cct.rho[xLocalInto, yLocalInto];
    }

    // test the density INTO WHICH we move:
    if (r < CCvals.f_rhoMin) {
      // rho < rho_min calc
      //ft = computeTopographicalSpeed(tileX, tileY, theMapData.getHeightGradientMap(xGlobalInto, yGlobalInto), direction);
      ff = ft;
    } else if (r > CCvals.f_rhoMax) {
      // rho > rho_max calc
      fv = computeFlowSpeed(xGlobalInto, yGlobalInto, direction);
      ff = fv;
    } else {
      // rho in-between calc
      fv = computeFlowSpeed(xGlobalInto, yGlobalInto, direction);
      //ft = computeTopographicalSpeed(tileX, tileY, theMapData.getHeightGradientMap(xGlobalInto, yGlobalInto), direction);
      ff = ft + (r - CCvals.f_rhoMin) / (CCvals.f_rhoMax - CCvals.f_rhoMin) * (fv - ft);
    }
    // ff = Mathf.Clamp (ff, CCvals.f_speedMin, CCvals.f_speedMax);
    return Math.Max(CCvals.f_speedMin, ff);
  }


  private float computeTopographicalSpeed(int x, int y, Vector2 dh, Vector2 direction)
  {
    // first, calculate the gradient in the direction we are looking. 
    // By taking the dot with Direction,
    // we extract the direction we're looking and assign it a proper sign
    // i.e. if we look left (x=-1) we want -dhdx(x,y), because the 
    // gradient is assigned with a positive x
    // 		therefore:		also, Vector.left = [-1,0]
    //						Vector2.Dot(Vector.left, dh[x,y]) = -dhdx;
    float dhInDirection = (direction.x * dh.x + direction.y * dh.y);
    // calculate the speed field from the equation
    return (CCvals.f_speedMax + (dhInDirection - CCvals.f_slopeMin) / (CCvals.f_slopeMax - CCvals.f_slopeMin) * (CCvals.f_speedMin - CCvals.f_speedMax));
  }

  private float computeFlowSpeed(int xI, int yI, Vector2 direction)
  {
    // the flow speed is simply the average velocity field of the region 
    // INTO WHICH we are looking,
    // dotted with the direction vector
    Vector2 vAvePt = readDataFromPoint_vAve(xI, yI);
    float theDotPrd = (vAvePt.x * direction.x + vAvePt.y * direction.y);
    return Math.Max(CCvals.f_speedMin, theDotPrd);
  }

  private void computeCostField(CC_Tile cct)
  {
    for (int n = 0; n < tileSize; n++) {
      for (int m = 0; m < tileSize; m++) {
        for (int d = 0; d < CCvals.ENSW.Length; d++) {
          cct.C[n, m][d] = computeCostFieldValue(n, m, d, CCvals.ENSW[d], cct);
        }
      }
    }
  }

  private float computeCostFieldValue(int tileX, int tileY, int d, Vector2 direction, CC_Tile cct)
  {
    int xLocalInto = tileX + (int)direction.x;
    int yLocalInto = tileY + (int)direction.y;

    int xGlobalInto = cct.myLoc.x * tileSize + xLocalInto;
    int yGlobalInto = cct.myLoc.y * tileSize + yLocalInto;

    // if we're looking in an invalid direction, dont store this value
    if (cct.f[tileX, tileY][d] == 0) {
      return Mathf.Infinity;
    } else if (!isPointValid(xGlobalInto, yGlobalInto)) {
      return Mathf.Infinity;
    }

    // initialize g as the map discomfort data value
    float g = 0; // = theMapData.getDiscomfortMap(xGlobalInto, yGlobalInto);
    float r;
    // test to see if the point we're looking INTO is in a DIFFERENT tile, and if so, pull it
    if ((xLocalInto < 0) || (xLocalInto > tileSize - 1) || (yLocalInto < 0) || (yLocalInto > tileSize - 1)) {
      g += readDataFromPoint_g(xGlobalInto, yGlobalInto);
      r = readDataFromPoint_rho(xGlobalInto, yGlobalInto);
    } else {
      g += cct.g[xLocalInto, yLocalInto];
      r = cct.rho[xLocalInto, yLocalInto];
    }
    // clamp g to make sure it's not > 1
    g = Mathf.Clamp(g, 0f, 1f);

    float cost = (CCvals.C_alpha * cct.f[tileX, tileY][d]
                + CCvals.C_beta
                + CCvals.C_gamma * g
                + CCvals.C_delta * r)
                    / cct.f[tileX, tileY][d];

    return cost;
  }


  // ******************************************************************************
  //			TOOLS AND UTILITIES
  //******************************************************************************
  public bool isPointValid(int x, int y)
  {
    // check to make sure the point is not outside the grid
    if ((x < 0) || (y < 0) || (x > _mapX - 1) || (y > _mapY - 1)) {
      return false;
    }
    // check to make sure the point is not on a place of absolute discomfort (like inside a building)
    // check to make sure the point is not in a place dis-allowed by terrain (slope)
    //if (theMapData.getDiscomfortMap(x, y) == 1)
    //{
    //  return false;
    //}
    return true;
  }

  // ******************************************************************************************
  // 				functions used for reading and writing to tiles
  // ******************************************************************************************
  private CC_Tile getLocalTile(Location l)
  {
    if (_tiles.ContainsKey(l)) {
      return _tiles[l];
    } else {
      return (new CC_Tile(0, l));
    }
  }

  // *** read ops ***
  private float readDataFromPoint_rho(int xGlobal, int yGlobal)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //float f = localTile.readData_rho (xTile, yTile);
    //return f;
    return _tiles[l].readData_rho(xTile, yTile);
  }
  private float readDataFromPoint_g(int xGlobal, int yGlobal)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //float f = localTile.readData_g (xTile, yTile);
    //return f;
    return _tiles[l].readData_g(xTile, yTile);
  }
  private Vector2 readDataFromPoint_vAve(int xGlobal, int yGlobal)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //Vector2 f = localTile.readData_vAve (xTile, yTile);
    //return f;
    return _tiles[l].readData_vAve(xTile, yTile);
  }
  private Vector4 readDataFromPoint_f(int xGlobal, int yGlobal)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //Vector4 f = localTile.readData_f (xTile, yTile);
    //return f;
    return _tiles[l].readData_f(xTile, yTile);
  }
  private Vector4 readDataFromPoint_C(int xGlobal, int yGlobal)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //Vector4 f = localTile.readData_C (xTile, yTile);
    //return f;
    return _tiles[l].readData_C(xTile, yTile);
  }

  // *** write ops ***
  private void writeDataToPoint_rho(int xGlobal, int yGlobal, float val)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //localTile.writeData_rho (xTile, yTile, val);
    //_tiles[localTile.myLoc] = localTile;
    _tiles[l].writeData_rho(xTile, yTile, val);
  }
  private void writeDataToPoint_g(int xGlobal, int yGlobal, float val)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //localTile.writeData_g (xTile, yTile, val);
    //_tiles[localTile.myLoc] = localTile;
    _tiles[l].writeData_g(xTile, yTile, val);
  }
  private void writeDataToPoint_vAve(int xGlobal, int yGlobal, Vector2 val)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    _tiles[l].writeData_vAve(xTile, yTile, val);
  }
  private void writeDataToPoint_f(int xGlobal, int yGlobal, Vector4 val)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //localTile.writeData_f (xTile, yTile, val);
    //_tiles[localTile.myLoc] = localTile;
    _tiles[l].writeData_f(xTile, yTile, val);
  }
  private void writeDataToPoint_C(int xGlobal, int yGlobal, Vector4 val)
  {
    Location l = new Location(
      Math.Floor((double)xGlobal / tileSize),
      Math.Floor((double)yGlobal / tileSize));
    //CC_Tile localTile = getLocalTile (l);
    int xTile = xGlobal - l.x * tileSize;
    int yTile = yGlobal - l.y * tileSize;
    //localTile.writeData_C (xTile, yTile, val);
    //_tiles[localTile.myLoc] = localTile;
    _tiles[l].writeData_C(xTile, yTile, val);
  }
}
