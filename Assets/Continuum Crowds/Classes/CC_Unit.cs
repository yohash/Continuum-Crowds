using UnityEngine;
using System;

[System.Serializable]
public class CC_Unit
{
  // store unit size
  public int sizeX;
  public int sizeY;

  public Vector2 GetVelocity() { return getVelocity(); }
  public Vector2 GetPosition() { return getPosition(); }

  private Func<Vector2> getVelocity;
  // in xz euler angles
  private Func<Vector2> getRotation;
  private Func<Vector2> getPosition;

  public CC_Unit(Func<Vector2> getVelocity, Func<Vector2> getRotation, Func<Vector2> getPosition, Vector2 unitDimensions)
  {
    sizeX = (int)Math.Round(unitDimensions.x);
    sizeY = (int)Math.Round(unitDimensions.y);

    this.getVelocity = getVelocity;
    this.getRotation = getRotation;
    this.getPosition = getPosition;
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
}
