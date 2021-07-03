using System;
using System.Collections.Generic;
using UnityEngine;

public class CcDestination : IEquatable<CcDestination>
{
  /// <summary>
  /// The corner location of the tile
  /// </summary>
  [SerializeField] private readonly Location _tile;
  public Location Tile {
    get { return _tile; }
  }

  /// <summary>
  /// List of locations defining this destination
  /// </summary>
  [SerializeField] private readonly List<Location> _goal;
  public List<Location> Goal {
    get { return _goal; }
  }

  // cache the hash code in effort to reduce re-compute
  private int _hash = 0;

  public CcDestination(CC_Tile tile, List<Location> goal) => (_tile, _goal) = (tile.Corner, goal);
  public CcDestination(Location tile, List<Location> goal) => (_tile, _goal) = (tile, goal);

  // *******************************************************************
  //    IEquatable
  // *******************************************************************
  public bool Equals(CcDestination destination)
  {
    return destination.Tile == Tile &&
      EqualityComparer<List<Location>>.Default.Equals(destination.Goal, Goal);
  }

  public override bool Equals(object obj)
  {
    return obj is CcDestination destination &&
           _tile.Equals(destination._tile) &&
           EqualityComparer<List<Location>>.Default.Equals(_goal, destination._goal);
  }

  public override int GetHashCode()
  {
    if (_hash == 0) {
      _hash = generateHash();
    }
    return _hash;
  }

  private int generateHash()
  {
    int hashCode = -1244036622;
    hashCode = hashCode * -1521134295 + _tile.GetHashCode();
    hashCode = hashCode * -1521134295 + EqualityComparer<List<Location>>.Default.GetHashCode(_goal);
    return hashCode;
  }
}
