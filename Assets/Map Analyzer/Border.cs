using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Border : IPathable
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

    locations = new List<Vector2Int>();
    neighborBorders = new List<Border>();

    ID = Guid.NewGuid();
  }

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

  // *******************************************************************
  //    IPathable
  // *******************************************************************
  public List<IPathable> Neighbors()
  {
    throw new NotImplementedException();
  }

  public Dictionary<IPathable, float> CostByNeighbor()
  {
    throw new NotImplementedException();
  }

  public float Heuristic(IPathable endGoal)
  {
    throw new NotImplementedException();
  }

  public float Cost(IPathable neighbor)
  {
    throw new NotImplementedException();
  }
}
