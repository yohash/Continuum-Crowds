using System;
using System.Collections.Generic;

[Serializable]
public partial struct Location : IEquatable<Location>, IPathable<Location>
{
  public readonly int x;
  public readonly int y;

  public Location(int x, int y)
  {
    this.x = x;
    this.y = y;
  }

  // *******************************************************************
  //    IPathable
  // *******************************************************************
  public IEnumerable<IPathable<Location>> Neighbors()
  {
    throw new NotImplementedException();
  }

  public Dictionary<IPathable<Location>, float> CostByNeighbor()
  {
    throw new NotImplementedException();
  }

  public float Heuristic(IPathable<Location> endGoal)
  {
    throw new NotImplementedException();
  }

  public float Cost(IPathable<Location> neighbor)
  {
    throw new NotImplementedException();
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
