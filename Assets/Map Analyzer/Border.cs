using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Border : IPathable<Border> 
{
  public DIRECTION Direction { get; private set; }

  public Guid ID { get { return _id; } private set { _id = value; } }
  [SerializeField] private Guid _id;

  // *******************************************************************
  //    Border
  // *******************************************************************
  /// <summary>
  /// Reference to the containing tile
  /// </summary>
  private MapTile tile;
  public MapTile Tile { get { return tile; } private set { tile = value; } }

  /// <summary>
  /// Reference to the containing Region
  /// </summary>
  private Region region;
  public Region Region { get { return region; } set { region = value; } }

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
    neighborBorders = new List<Border>();

    ID = Guid.NewGuid();
  }

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

  /// <summary>
  /// Neighboring borders in nearby tiles
  /// </summary>
  private List<Border> neighborBorders;
  public IEnumerable<Border> GetNeighbors()
  {
    foreach (var border in neighborBorders) { yield return border; }
  }
  public void AddNeighbor(Border neighbor)
  {
    if (neighbor != this && !neighborBorders.Contains(neighbor)) {
      neighborBorders.Add(neighbor);
    }
  }

  // *******************************************************************
  //    IPathable
  // *******************************************************************
  public IEnumerable<Border> Neighbors()
  {
    throw new NotImplementedException();
  }

  public float Heuristic(Border endGoal)
  {
    throw new NotImplementedException();
  }

  public float Cost(Border neighbor)
  {
    throw new NotImplementedException();
  }
}
