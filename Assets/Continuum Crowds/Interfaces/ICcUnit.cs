using System;
using UnityEngine;

public interface ICcUnit
{
  int UniqueId();
  Vector2 Velocity();
  Vector2 Position();
  Quaternion Rotation();
  float[] Density();
  void SetVelocity(Action<Vector2> velocity);
}
