using System.Collections.Generic;

public class Border : IPathable
{
  public DIRECTION Direction { get; private set; }

  // TODO: Remove this when debug lines are no longer needed
  public Dictionary<Border, List<Location>> PathByNeighbor
      = new Dictionary<Border, List<Location>>();

  private Dictionary<IPathable, float> costByNeighbor
      = new Dictionary<IPathable, float>();

  public Location Center { get; private set; }

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
  }

  public override string ToString()
  {
    return "Border: " + Center;
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
    // re-compute the center
    Center = locations.Average();
  }
  public bool Contains(Location v)
  {
    return locations.Contains(v);
  }

  // *******************************************************************
  //    IPathable
  // *******************************************************************
  public IEnumerable<IPathable> Neighbors()
  {
    foreach (var border in costByNeighbor.Keys) { yield return border; }
  }

  public float Heuristic(Location endGoal)
  {
    return (float)(Center - endGoal).magnitude();
  }

  public float Cost(IPathable neighbor)
  {
    return costByNeighbor.TryGetValue(neighbor, out var v) ? v : float.MaxValue;
  }

  public Location AsLocation()
  {
    return Center;
  }
}
