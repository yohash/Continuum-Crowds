using UnityEngine;
using System;

[System.Serializable]
public class CC_Unit
{
  // class vars
  private static readonly float fadeout = 2.5f;

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

    for (int x = 0; x < cols; x++) {
      for (int y = 0; y < rows; y++) {
        // check for the different zones
        // (1) within the main footprint range
        // (2) within the buffer to the left/right or top/bottom of the main footprint
        //      where footprint drops off linearly
        // (3) one of the 4 corners, where footprint drops off radially

        if (x < buffer || x > sizeX + buffer ||
            y < buffer || y > sizeY + buffer) {
          // get x distance
          float xVar = x < buffer ? x / fadeout :
                       x > buffer + sizeX ? (cols - x) / cols :
                       0;
          // get y distance
          float yVar = y < buffer ? y / fadeout :
                       y > buffer + sizeY ? (rows - y) / rows :
                       0;
          // clamp above 0
          xVar = xVar < 0 ? 0 : xVar;
          yVar = yVar < 0 ? 0 : yVar;
          // linearly fade to the nearest
          footprint[x, y] = (float)Math.Sqrt(xVar * xVar + yVar * yVar);
        } else {
          // within the main footprint range, everything is 1
          footprint[x, y] = 1;
        }
      }
    }

    return footprint;
  }
}
