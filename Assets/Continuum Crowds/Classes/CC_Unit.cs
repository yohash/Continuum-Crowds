using UnityEngine;
using System;

public class CC_Unit
{
  // class vars
  private static readonly float fadeout = 3f;

  public Vector2 GetVelocity() { return getVelocity(); }
  public Vector2 GetPosition() { return getPosition(); }

  private Func<Vector2> getVelocity;
  // in xz euler angles
  private Func<Vector2> getRotation;
  private Func<Vector2> getPosition;
  private Func<Vector2> getSize;
  // local vars
  private float[,] baseprint;

  private Vector2Int size;
  public int SizeX { get { return size.x; } }
  public int SizeY { get { return size.y; } }

  public CC_Unit(Func<Vector2> getVelocity, Func<Vector2> getRotation, Func<Vector2> getPosition, Func<Vector2> unitDimensions)
  {
    getSize = unitDimensions;
    // verify
    if (getSize().x < 1 || getSize().y < 1) {
      Debug.LogWarning("CC_Unit created with unit size dimension <1");
    }

    this.getVelocity = getVelocity;
    this.getRotation = getRotation;
    this.getPosition = getPosition;

    baseprint = computeBaseFootprint();
  }

  public float[,] GetFootprint()
  {
    // use getPosition(), getRotation(), and unit size to compute
    // the footprint, properly interpolated and splatted into a 2x2 grid

    // wrap getPostion to the range 0 < position < 1 (grid size)
    float xOffset = getPosition().x.Modulus(1f);
    float yOffset = getPosition().y.Modulus(1f);

    // TEMPORARY
    baseprint = computeBaseFootprint();

    // TODO: integrate rotation
    return baseprint.BilinearInterpolation(xOffset, yOffset);
  }

  private float[,] computeBaseFootprint()
  {
    var sizeX = (int)Math.Round(getSize().x);
    var sizeY = (int)Math.Round(getSize().y);
    // ensure we're clamped above 1
    sizeX = Math.Max(sizeX, 1);
    sizeY = Math.Max(sizeY, 1);
    // cache
    size = new Vector2Int(sizeX, sizeY);

    // initialize 'positions' with the standard grid and dimensions provided
    int buffer = (int)Math.Ceiling(fadeout);
    int cols = sizeX + buffer * 2;
    int rows = sizeY + buffer * 2;

    // init the footprint
    var footprint = new float[cols, rows];

    bool xInside(int x) { return x >= buffer && x < buffer + sizeX; }
    bool yInside(int y) { return y >= buffer && y < buffer + sizeY; }

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

          float xVar = x < buffer ? x + 1 : cols - x;
          float yVar = y < buffer ? y + 1 : rows - y;

          float dd = (float)Math.Sqrt(xVar * xVar + yVar * yVar);
          // scale
          if (dd > buffer) dd = buffer;
          footprint[x, y] = dd / (buffer + 1);


          //// determine the corner from which we measure distance
          //int cornerY = y < buffer ? buffer : buffer + sizeY - 1;
          //int cornerX = x < buffer ? buffer : buffer + sizeX - 1;
          //// compute delta from said corner
          //float dx = Math.Abs(cornerX - x);
          //float dy = Math.Abs(cornerY - y);

          //// use distance formula
          //float dist = (float)Math.Sqrt(dx * dx + dy * dy);
          //// invert by value of buffer
          //float value = buffer - dist;
          //// clamp above 0
          //if (value < 0) value = 0;
          //footprint[x, y] = value / (buffer);
        }
      }
    }
    Debug.Log(footprint.ToString<float>("{0:.00}"));
    return footprint;
  }
}
