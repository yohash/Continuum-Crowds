using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Region
{
  [NonSerialized] private MapTile tile;
  [SerializeField] private List<Vector2Int> locations;

  [NonSerialized] private List<Region> neighborRegions;
  [NonSerialized] private List<Region> internalConnections;

  public List<Vector2Int> connections = new List<Vector2Int>();

  public void AddConnection(Region connection)
  {
    if (connection != this && !internalConnections.Contains(connection)) {
      internalConnections.Add(connection);
      connections.Add(connection.Average);
    }
  }

  public void AddNeighbor(Region neighbor)
  {
    if (neighbor != this && !neighborRegions.Contains(neighbor)) {
      neighborRegions.Add(neighbor);
    }
  }

  public Region(MapTile tile)
  {
    this.tile = tile;
    locations = new List<Vector2Int>();

    neighborRegions = new List<Region>();
    internalConnections = new List<Region>();
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

  public IEnumerable GetLocations()
  {
    foreach (var location in locations) {
      yield return location;
    }
  }

  public IEnumerable GetInternalConnections()
  {
    foreach (var region in internalConnections) {
      yield return region;
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
