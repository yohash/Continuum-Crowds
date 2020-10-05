using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Border : IPathable<Border>
{
  public DIRECTION Direction { get; private set; }

  // TODO: Remove this when debug lines are no longer needed
  public Dictionary<Border, List<Location>> PathByNeighbor
    = new Dictionary<Border, List<Location>>();

  public Guid ID { get { return _id; } private set { _id = value; } }
  [SerializeField] private Guid _id;

  private Dictionary<Border, float> costByNeighbor;

  /// <summary>
  /// Basic constructor takes a containg-tile and the border's
  /// direction relative to the tile
  /// </summary>
  /// <param name="tile"></param>
  /// <param name="d"></param>
  public Border(MapTile tile, DIRECTION d)
  {
    this.tile = tile;
    Direction = d;

    locations = new List<Location>();
    costByNeighbor = new Dictionary<Border, float>();

    ID = Guid.NewGuid();
  }

  /// <summary>
  /// Add a border that is connecting, either in neighboring tile
  /// or within the same tile, sharing a region
  /// </summary>
  /// <param name="neighbor"></param>
  public void AddNeighbor(Border neighbor, float cost, List<Location> path)
  {
    if (this == neighbor) { return; }
    costByNeighbor[neighbor] = cost;
    PathByNeighbor[neighbor] = path;
  }

  // *******************************************************************
  //    Border
  // *******************************************************************
  /// <summary>
  /// Reference to the containing tile
  /// </summary>
  private MapTile tile;
  public MapTile Tile { get { return tile; } private set { tile = value; } }

  /// <summary>
  /// All integer locations that comprise this border
  /// </summary>
  private List<Location> locations;
  public IEnumerable<Location> GetLocations()
  {
    foreach (var location in locations) { yield return location; }
  }
  public void AddLocation(int x, int y)
  {
    AddLocation(new Location(x, y));
  }
  public void AddLocation(Location location)
  {
    if (!locations.Contains(location)) { locations.Add(location); }
  }
  public bool Contains(Location v)
  {
    return locations.Contains(v);
  }

  // *******************************************************************
  //    IPathable
  // *******************************************************************
  public IEnumerable<Border> Neighbors()
  {
    foreach (var border in costByNeighbor.Keys) { yield return border; }
  }

  public float Heuristic(Border endGoal)
  {
    return (float)(Average - endGoal.Average).magnitude();
  }

  public float Cost(Border neighbor)
  {
    return costByNeighbor.TryGetValue(neighbor, out var v) ? v : float.MaxValue;
  }

  private Location _average = Location.Zero;
  public Location Average {
    get {
      if (_average.Equals(Location.Zero)) {
        _average = locations.Count == 0 ?
          Location.Zero :
          getAvg();
      }
      return _average;
    }
  }

  private Location getAvg()
  {
    Location l = Location.Zero;
    foreach (var item in locations) {
      l += item;
    }
    l /= locations.Count;
    return l;
  }
}
