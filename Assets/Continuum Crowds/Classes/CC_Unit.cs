using UnityEngine;
using System;

[System.Serializable]
public class CC_Unit
{
  // store unit size
  public int sizeX;
  public int sizeY;

  // private variables
  [SerializeField] private Vector2 _ccu_anchor;

  // getters and setters
  public float[,] GetFootprint() { return getFootprint(); }
  public Vector2 GetAnchorPoint()
  {
    //_CC_Footprint_Anchor = _myUnit.GetCurrent2DPosition();
    _ccu_anchor += -new Vector2(((float)getFootprint().GetLength(0)) / 2f,
                                ((float)getFootprint().GetLength(1)) / 2f);
    return _ccu_anchor;
  }
  public Vector2 GetVelocity() { return getVelocity(); }

  private Func<Vector2> getVelocity;
  private Func<Vector2> getAnchor;
  private Func<float[,]> getFootprint;


  public CC_Unit(Func<Vector2> getVelocity, Func<Vector2> getAnchor, Func<float[,]> getFootprint)
  {
    this.getVelocity = getVelocity;
    this.getAnchor = getAnchor;
    this.getFootprint = getFootprint;
  }
}
