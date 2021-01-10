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

  public static Vector3 WithX(this Vector3 v, float x)
  {
    return new Vector3(x, v.y, v.z);
  }
  public static Vector3 WithY(this Vector3 v, float y)
  {
    return new Vector3(v.x, y, v.z);
  }
  public static Vector3 WithZ(this Vector3 v, float z)
  {
    return new Vector3(v.x, v.y, z);
  }
  public static Vector2 XYZtoXY(this Vector3 v)
  {
    return new Vector2(v.x, v.z);
  }

  public static Vector2Int Average(this List<Vector2Int> v)
  {
    if (v.Count == 1) return v[0];
    Vector2 average = new Vector2(0, 0);
    foreach (var vector in v) {
      average += vector;
    }
    return Vector2Int.RoundToInt(average / v.Count);
  }
}
