using UnityEngine;

public enum DIRECTION { NORTH, SOUTH, EAST, WEST }
public static class DirectionExtensions
{
  public static Vector2Int ToVector(this DIRECTION d)
  {
    switch (d) {
      case DIRECTION.NORTH: return new Vector2Int(0, 1);
      case DIRECTION.SOUTH: return new Vector2Int(0, -1);
      case DIRECTION.EAST: return new Vector2Int(1, 0);
      case DIRECTION.WEST:
      default: return new Vector2Int(0, -1);
    }
  }

  public static DIRECTION Opposite(this DIRECTION d)
  {
    switch (d) {
      case DIRECTION.NORTH: return DIRECTION.SOUTH;
      case DIRECTION.SOUTH: return DIRECTION.NORTH;
      case DIRECTION.EAST: return DIRECTION.WEST;
      case DIRECTION.WEST:
      default: return DIRECTION.EAST;
    }
  }

  public static DIRECTION ToDirection(this Vector2Int v)
  {
    return ((Vector2)v).ToDirection();
  }

  public static DIRECTION ToDirection(this Vector2 v)
  {
    var n = v.normalized;
    if (Vector2.Dot(n, new Vector2(0, 1)) >= 0.707f) return DIRECTION.NORTH;
    if (Vector2.Dot(n, new Vector2(1, 0)) >= 0.707f) return DIRECTION.EAST;
    if (Vector2.Dot(n, new Vector2(-1, 0)) >= 0.707f) return DIRECTION.WEST;
    return DIRECTION.SOUTH;
  }
}
