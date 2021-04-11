using System.Collections.Generic;
using UnityEngine;

public static class Drawings
{
  public static void DrawSquare(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, Color c, float duration = 0)
  {
    Debug.DrawLine(c1, c2, c, duration);
    Debug.DrawLine(c2, c3, c, duration);
    Debug.DrawLine(c3, c4, c, duration);
    Debug.DrawLine(c4, c1, c, duration);
  }

  public static void DrawSquare(Vector3 corner, Color c, float duration = 0)
  {
    Debug.DrawLine(corner, corner + Vector3.forward, c, duration);
    Debug.DrawLine(corner + Vector3.forward, corner + Vector3.forward + Vector3.right, c, duration);
    Debug.DrawLine(corner + Vector3.forward + Vector3.right, corner + Vector3.right, c, duration);
    Debug.DrawLine(corner + Vector3.right, corner, c, duration);
  }

  public static void DrawX(Vector3 v, Color c, float size = 0.3f, float duration = 0)
  {
    Debug.DrawLine(v + new Vector3(size, 0, size), v - new Vector3(size, 0, size), c, duration);
    Debug.DrawLine(v + new Vector3(-size, 0, size), v - new Vector3(-size, 0, size), c, duration);
  }

  public static void DrawPath(List<Vector2> path, float height, Color c, float duration = 0)
  {
    for (int i = 1; i < path.Count; i++) {
      Debug.DrawLine(path[i - 1].ToXYZ(height) + new Vector3(0.5f, 0.5f, 0.5f),
        path[i].ToXYZ(height) + new Vector3(0.5f, 0.5f, 0.5f),
        c, duration);
    }
  }

  public static void DrawPath(List<Location> path, float height, Color c, float duration = 0)
  {
    for (int i = 1; i < path.Count; i++) {
      Debug.DrawLine(path[i - 1].ToVector3(height) + new Vector3(0.5f, 0.5f, 0.5f),
        path[i].ToVector3(height) + new Vector3(0.5f, 0.5f, 0.5f),
        c, duration);
    }
  }
}
