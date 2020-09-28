using System;
using UnityEngine;

public static class LocationExtensions
{
  public static double sqrMagnitude(this Location l)
  {
    return (l.x * l.x) + (l.y * l.y);
  }
  public static double magnitude(this Location l)
  {
    return Math.Sqrt((l.x * l.x) + (l.y * l.y));
  }
  public static Vector2 ToVector2(this Location l)
  {
    return new Vector2(l.x, l.y);
  }
  public static Vector3 ToVector3(this Location l)
  {
    return new Vector3(l.x, 0, l.y);
  }
  public static Vector3 ToVector3(this Location l, float y)
  {
    return new Vector3(l.x, y, l.y);
  }
}

public partial class Location
{
  // *******************************************************************
  //    Extensions
  // *******************************************************************
  public static Location zero { get { return new Location(0, 0); } }
  public static Location one { get { return new Location(1, 1); } }

  public static Location operator +(Location l1, Location l2)
  {
    return new Location(l1.x + l2.x, l1.y + l2.y);
  }
  public static Location operator -(Location l1, Location l2)
  {
    return new Location(l1.x - l2.x, l1.y - l2.y);
  }
  public static Location operator /(Location l, float d)
  {
    return new Location(l.x / d, l.y / d);
  }
  // *******************************************************************
  //    Extensions for Unity classes
  // *******************************************************************
  public static Location operator +(Location l1, Vector2 l2)
  {
    return new Location(l1.x + (int)Math.Round(l2.x, 0), l1.y + (int)Math.Round(l2.y, 0));
  }
  public static Location operator -(Location l1, Vector2 l2)
  {
    return new Location(l1.x - (int)Math.Round(l2.x, 0), l1.y - (int)Math.Round(l2.y, 0));
  }
  public static Location operator +(Location l1, Vector2Int l2)
  {
    return new Location(l1.x + l2.x, l1.y + l2.y);
  }
  public static Location operator -(Location l1, Vector2Int l2)
  {
    return new Location(l1.x - l2.x, l1.y - l2.y);
  }
}
