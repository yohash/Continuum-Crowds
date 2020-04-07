using UnityEngine;
using System.Collections;

[System.Serializable]
public class CC_Unit
{
  public int UnitX, UnitY;

  // private variables
  [SerializeField] Vector2 _CC_Unit_velocity;

  private float[,] _CC_Unit_Footprint;

  [SerializeField] private Vector2 _CC_Footprint_Anchor;

  // getters and setters
  public float[,] GetFootprint() { return _CC_Unit_Footprint; }
  public Vector2 GetAnchorPoint() { return _CC_Footprint_Anchor; }
  public Vector2 GetVelocity() { return _CC_Unit_velocity; }


  public CC_Unit(/*Unit u*/)
  {
    //_myUnit = u;

    //UnitX = u.SizeX;
    //UnitY = u.SizeZ;

    //_CC_Unit_Footprint = u.Footprint;
    assign_CC_Footprint_Anchor();
  }

  // the continuumCrowds code considers units lower-left location (similar to a rect)
  // currently, my units are CENTERED on their transform, so we subtract half their size
  public void UpdatePhysics()
  {
    //_CC_Unit_Footprint = _myUnit.GetInterpolatedFootprint();

    //_CC_Unit_velocity = _myUnit.Velocity;

    assign_CC_Footprint_Anchor();
  }


  private void assign_CC_Footprint_Anchor()
  {
    //_CC_Footprint_Anchor = _myUnit.GetCurrent2DPosition();
    _CC_Footprint_Anchor += (-(new Vector2(((float)_CC_Unit_Footprint.GetLength(0)) / 2f,
                                            ((float)_CC_Unit_Footprint.GetLength(1)) / 2f)));
  }
}
