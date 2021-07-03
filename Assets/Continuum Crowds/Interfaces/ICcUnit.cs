using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICcUnit
{
  Vector2 Velocity();
  Vector2 Position();
  Quaternion Rotation();
  float[] Density();
}