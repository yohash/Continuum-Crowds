using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Region
{
  [NonSerialized] private MapTile tile;
  [SerializeField] private List<Vector2Int> locations;

  public Region(MapTile tile)
  {
    this.tile = tile;
    locations = new List<Vector2Int>();
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

  public IEnumerable GetLocations()
  {
    foreach (var location in locations) {
      yield return location;
    }
  }
}