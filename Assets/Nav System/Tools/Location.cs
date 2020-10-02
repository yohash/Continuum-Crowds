using System;
using System.Collections.Generic;

[Serializable]
public partial class Location : IEquatable<Location>
{
  public readonly int x;
  public readonly int y;

  private Dictionary<Location, float> costByNeighbor;

  public Location(int x, int y)
  {
    this.x = x;
    this.y = y;
    costByNeighbor = new Dictionary<Location, float>();
  }

  public Location(double x, double y)
  {
    this.x = (int)Math.Round(x, 0);
    this.y = (int)Math.Round(y, 0);
    costByNeighbor = new Dictionary<Location, float>();
  }

  public void AddNeighbor(Location loc)
  {
    if (this == loc) { return; }
    // TODO: integrate height difference for more realistic cost
    costByNeighbor[loc] = 1;
  }

  public override string ToString()
  {
    return $"({x}, {y})";
  }

  // *******************************************************************
  //    IEquatable
  // *******************************************************************
  public bool Equals(Location l2)
  {
    return l2 != null && x == l2.x && y == l2.y;
  }

  public override bool Equals(object obj)
  {
    return obj is Location l && Equals(l);
  }

  public static bool Equals(Location l1, Location l2)
  {
    return l1.Equals(l2);
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
    if (((object)l1) == null || ((object)l2) == null) {
      return object.Equals(l1, l2);
    }
    return l1.Equals(l2);
  }

  public static bool operator !=(Location l1, Location l2)
  {
    if (((object)l1) == null || ((object)l2) == null) {
      return !object.Equals(l1, l2);
    }
    return !l1.Equals(l2);
  }
}
