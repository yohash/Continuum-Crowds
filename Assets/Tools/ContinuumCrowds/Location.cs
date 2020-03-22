using System;

[Serializable]
public struct Location : IEquatable<Location>
{
  public readonly int x;
  public readonly int y;

  public Location(int x, int y)
  {
    this.x = x;
    this.y = y;
  }

  public override bool Equals(object obj)
  {
    return Equals((Location)obj);
  }

  public bool Equals(Location l2)
  {
    return (x == l2.x) && (y == l2.y);
  }

  public override int GetHashCode()
  {
    int hash = 17;
    hash = (31 * hash) + x;
    hash = (31 * hash) + y;
    return hash;
  }

  public static bool operator ==(Location l1, Location l2)
  {
    return ((l1.x == l2.x) && (l1.y == l2.y));
  }

  public static bool operator !=(Location l1, Location l2)
  {
    return (!(l1 == l2));
  }
}
