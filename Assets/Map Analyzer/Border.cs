using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Border
{
  public DIRECTION Direction { get; private set; }

  public Guid ID { get { return _id; } private set { _id = value; } }
  [SerializeField] private Guid _id;

  /// <summary>
  /// All integer locations that comprise this border
  /// </summary>
  private List<Vector2Int> locations;

  public IEnumerable<Vector2Int> GetLocations()
  {
    foreach (var location in locations) { yield return location; }
  }
  public void AddLocation(Vector2 location)
  {
    AddLocation(Vector2Int.RoundToInt(location));
  }
  public void AddLocation(Vector2Int location)
  {
    if (!locations.Contains(location)) { locations.Add(location); }
  }
  public bool Contains(Vector2Int v)
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

  public Border(MapTile tile, DIRECTION d)
  {
    this.tile = tile;
    Direction = d;

    locations = new List<Vector2Int>();
    neighborBorders = new List<Border>();

    ID = Guid.NewGuid();
  }

  [SerializeField] private Vector2Int _average;
  public Vector2Int Average {
    get {
      _average = locations.Average();
      return _average;
    }
  }
}
