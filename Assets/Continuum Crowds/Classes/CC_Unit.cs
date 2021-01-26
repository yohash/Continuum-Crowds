﻿using UnityEngine;
using System;

[System.Serializable]
public class CC_Unit
{
  // class vars
  private static readonly float fadeout = 2.5f;

  // store unit size
  public int sizeX;
  public int sizeY;

  public Vector2 GetVelocity() { return getVelocity(); }
  public Vector2 GetPosition() { return getPosition(); }

  private Func<Vector2> getVelocity;
  // in xz euler angles
  private Func<Vector2> getRotation;
  private Func<Vector2> getPosition;

  // local vars
  private float[,] baseprint;

  public CC_Unit(Func<Vector2> getVelocity, Func<Vector2> getRotation, Func<Vector2> getPosition, Vector2 unitDimensions)
  {
    sizeX = (int)Math.Round(unitDimensions.x);
    sizeY = (int)Math.Round(unitDimensions.y);
    // verify 
    if (sizeX < 1 || sizeY < 1) {
      Debug.LogWarning("CC_Unit created with unit size dimension <1");
    }

    // ensure we're clamped above 1
    sizeX = Math.Max(sizeX, 1);
    sizeY = Math.Max(sizeY, 1);

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



    // TODO: figure out, based on sizeX and sizeY, and rotation, how big footprint should be
    float[,] footprint = new float[sizeX, sizeY];

    return footprint;
  }

  private float[,] computeBaseFootprint()
  {
    // (1) initialize 'positions' with the standard grid and dimensions provided 
    int buffer = (int)Math.Ceiling(fadeout);
    int cols = sizeX + buffer * 2;
    int rows = sizeY + buffer * 2;

    Vector2 center = new Vector2((cols - 1) / 2f, (rows - 1) / 2f);

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
          float xVar = x < buffer ? buffer - x :
                       x > buffer + sizeX ? sizeX + buffer - x :
                       0;
          // get y distance
          float yVar = y < buffer ? buffer - y :
                       y > buffer + sizeY ? sizeY + buffer - y :
                       0;
          // linearly fade to the nearest
          footprint[x, y] = (float)Math.Sqrt(xVar * xVar + yVar * yVar);
        } else {
          footprint[x, y] = 1;
        }
      }
    }

    return footprint;
  }
}
