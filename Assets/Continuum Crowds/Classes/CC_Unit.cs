using UnityEngine;
using System;

[System.Serializable]
public class CC_Unit
{
  // private variables
  [SerializeField] Vector2 _ccu_velocity;
  private float[,] _ccu_footprint;
  [SerializeField] private Vector2 _ccu_anchor;

  // getters and setters
  public float[,] GetFootprint() { return _ccu_footprint; }
  public Vector2 GetAnchorPoint() { return _ccu_anchor; }
  public Vector2 GetVelocity() { return _ccu_velocity; }

  private Func<Vector2> getVelocity;
  private Func<Vector2> getAnchor;
  private Func<float[,]> getFootprint;


  public CC_Unit(Func<Vector2> getVelocity, Func<Vector2> getAnchor, Func<float[,]> getFootprint)
  {
    this.getVelocity = getVelocity;
    this.getAnchor = getAnchor;
    this.getFootprint = getFootprint;
  }

  // the continuumCrowds code considers units lower-left location (similar to a rect)
  // currently, my units are CENTERED on their transform, so we subtract half their size
  public void UpdatePhysics()
  {
    //_ccu_footprint = _myUnit.GetInterpolatedFootprint();
    //_ccu_velocity = _myUnit.Velocity;
    _ccu_footprint = _myUnit.GetInterpolatedFootprint();
    _ccu_velocity = _myUnit.Velocity;

    assign_CC_Footprint_Anchor();
  }


  private void assign_CC_Footprint_Anchor()
  {
    //_CC_Footprint_Anchor = _myUnit.GetCurrent2DPosition();
    _ccu_anchor += -new Vector2(((float)_ccu_footprint.GetLength(0)) / 2f,
                                ((float)_ccu_footprint.GetLength(1)) / 2f);
  }
}
