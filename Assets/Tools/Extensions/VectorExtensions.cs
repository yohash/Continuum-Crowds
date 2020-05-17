using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
  public static Vector3 ToXZ(this Vector2 v)
  {
    return new Vector3(v.x, 0, v.y);
  }
  public static Vector3 ToXZ(this Vector2Int v)
  {
    return new Vector3(v.x, 0, v.y);
  }
  public static Vector3 ToXYZ(this Vector2 v, float y)
  {
    return new Vector3(v.x, y, v.y);
  }
  public static Vector3 ToXYZ(this Vector2Int v, float y)
  {
    return new Vector3(v.x, y, v.y);
  }
}
