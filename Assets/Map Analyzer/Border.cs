using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Border
{
  [NonSerialized] private MapTile tile;
  [SerializeField] private List<Vector2Int> locations;

  [NonSerialized] private List<Border> neighborBorders;

  public List<Vector2Int> connections = new List<Vector2Int>();

  public void AddConnection(Border connection)
  {
    if (connection != this && !neighborBorders.Contains(connection)) {
      neighborBorders.Add(connection);
      connections.Add(connection.Average);
    }
  }

  public void AddNeighbor(Border neighbor)
  {
    if (neighbor != this && !neighborBorders.Contains(neighbor)) {
      neighborBorders.Add(neighbor);
    }
  }

  public Border(MapTile tile)
  {
    this.tile = tile;
    locations = new List<Vector2Int>();

    neighborBorders = new List<Border>();
  }

  public void AddLocation(Vector2 location)
  {
    AddLocation(Vector2Int.RoundToInt(location));
  }
  public void AddLocation(Vector2Int location)
  {
    if (locations.Contains(location)) {
      Debug.LogWarning("Region already contains location: " + location);
      return;
    }
    locations.Add(location);
  }

  public bool Contains(Vector2Int v)
  {
    return locations.Contains(v);
  }

  public IEnumerable<Vector2Int> GetLocations()
  {
    foreach (var location in locations) {
      yield return location;
    }
  }

  public IEnumerable<Border> GetConnections()
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
