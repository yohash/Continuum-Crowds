using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Border
{
  [SerializeField] private List<Vector2Int> locations;

  [NonSerialized] private List<Border> neighborBorders;

  public DIRECTION Direction { get; private set; }

  // containers
  private MapTile tile;

  private Region region;
  public Region Region { get { return region; } set { region = value; } }

  public Border(MapTile tile, DIRECTION d)
  {
    this.tile = tile;
    locations = new List<Vector2Int>();

    Direction = d;

    neighborBorders = new List<Border>();
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
  public void AddNeighbor(Border neighbor)
  {
    if (neighbor != this && !neighborBorders.Contains(neighbor)) {
      neighborBorders.Add(neighbor);
    }
  }
  public IEnumerable<Vector2Int> GetLocations()
  {
    foreach (var location in locations) {
      yield return location;
    }
  }

  public IEnumerable<Border> GetNeighbors()
  {
    foreach (var border in neighborBorders) {
      yield return border;
    }
  }

  [SerializeField] private Vector2Int _average;
  public Vector2Int Average {
    get {
      _average = locations.Average();
      return _average;
    }
  }
}
