using UnityEngine;
using System;

public class CC_Unit
{
  public Vector2 GetVelocity() { return getVelocity(); }
  public Vector2 GetPosition() { return getPosition(); }
  public float GetRotation() { return getRotation(); }

  private Func<Vector2> getVelocity;
  // in y euler angles
  private Func<float> getRotation;
  private Func<Vector2> getPosition;
  private Func<Vector2> getSize;
  private Func<float> getFalloff;
  // local vars
  private float[,] baseprint;

  private Vector2Int size;
  public int SizeX { get { return size.x; } }
  public int SizeY { get { return size.y; } }

  public CC_Unit(
      Func<Vector2> getVelocity,
      Func<float> getRotation,
      Func<Vector2> getPosition,
      Func<Vector2> unitDimensions,
      Func<float> getFalloff
  )
  {
    getSize = unitDimensions;
    // verify
    if (getSize().x < 1 || getSize().y < 1) {
      Debug.LogWarning("CC_Unit created with unit size dimension <1");
    }

    this.getVelocity = getVelocity;
    this.getRotation = getRotation;
    this.getPosition = getPosition;
    this.getFalloff = getFalloff;

    baseprint = computeBaseFootprint();
    setLocalSize();
  }

  public float[,] GetFootprint()
  {
    // use getPosition(), getRotation(), and unit size to compute
    // the footprint, properly interpolated and splatted into a 2x2 grid

    // wrap getPostion to the range 0 < position < 1 (grid size)
    float xOffset = getPosition().x.Modulus(1f);
    float yOffset = getPosition().y.Modulus(1f);

    // TEMPORARY - dont re-compute each time footprint is sought
    // performing this action now for the sake of debugging
    baseprint = computeBaseFootprint();

    return baseprint;
  }

  private void setLocalSize()
  {
    var sizeX = (int)Math.Round(getSize().x);
    var sizeY = (int)Math.Round(getSize().y);
    // ensure we're clamped above 1
    sizeX = Math.Max(sizeX, 1);
    sizeY = Math.Max(sizeY, 1);
    // cache
    size = new Vector2Int(sizeX, sizeY);
  }

  private float[,] computeBaseFootprint()
  {
    // TODO: eliminate this when done debugging
    // cache local references
    setLocalSize();
    var fadeout = getFalloff();

    // initialize 'positions' with the standard grid and dimensions provided
    int buffer = (int)Math.Ceiling(fadeout);
    int cols = SizeX + buffer * 2;
    int rows = SizeY + buffer * 2;

    // init the footprint
    var footprint = new float[cols, rows];

    bool xInside(int x) { return x >= buffer && x < buffer + SizeX; }
    bool yInside(int y) { return y >= buffer && y < buffer + SizeY; }

    for (int x = 0; x < cols; x++) {
      for (int y = 0; y < rows; y++) {
        // check for the different zones
        // (1) within the main footprint range
        // (2) within the buffer to the left/right or top/bottom of the main footprint
        //      where footprint drops off linearly
        // (3) one of the 4 corners, where footprint drops off radially

        // if we're inside the footprint
        if (xInside(x) && yInside(y)) {
          // within the main footprint range, everything is 1
          footprint[x, y] = 1;
        }
        // if we're outside the x range, but inside y
        else if (!xInside(x) && yInside(y)) {
          // footprint drops off linearly over x
          // get x distance
          float xVar = x < buffer ? x + 1 : cols - x;
          footprint[x, y] = xVar / (buffer + 1);
        }
        // if we're outside the y range, but inside x
        else if (xInside(x) && !yInside(y)) {
          // footprint drops off linearly over y
          float yVar = y < buffer ? y + 1 : rows - y;
          footprint[x, y] = yVar / (buffer + 1);
        }
        // anything else, we're in a corner, drop off radially
        else {
          // determine how far x and y are away from closest corner
          float xVar = x < buffer ? buffer - x : buffer + x + 1 - cols;
          float yVar = y < buffer ? buffer - y : buffer + y + 1 - rows;
          // use distance formula to determine distance from corner
          float dd = (float)Math.Sqrt(xVar * xVar + yVar * yVar);

          // invert the distance by buffer+1
          dd = buffer + 1 - dd;
          if (dd < 0) dd = 0;
          // scale and record
          footprint[x, y] = dd / (buffer + 1);
        }
      }
    }

    return footprint;
  }
}
