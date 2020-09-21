using System.Collections.Generic;

public class Region
{
  public Region()
  {
    locations = new List<Location>();
    borders = new List<Border>();
  }

  // *******************************************************************
  //    REGION
  // *******************************************************************
  /// <summary>
  /// All integer locations that comprise this Region
  /// </summary>
  private List<Location> locations;

  public IEnumerable<Location> Locations()
  {
    foreach (var loc in locations) { yield return loc; }
  }
  public void AddLocation(Location v)
  {
    if (!locations.Contains(v)) { locations.Add(v); }
    // pair up locations with neighbors so they are pathable
    foreach (var dir in Directions.Each()) {
      var neighbor = v + dir.ToVector();
      if (ContainsLocation(neighbor)) {
        // pair up these two locations
        locations.Find(loc => loc == neighbor).AddNeighbor(v);
        v.AddNeighbor(neighbor);
      }
    }
  }
  public bool ContainsLocation(Location v)
  {
    return locations.Contains(v);
  }
  public Location GetFirst()
  {
    if (locations.Count > 0) { return locations[0]; }
    return Location.zero;
  }

  /// <summary>
  /// All borders that are contained within this Region
  /// </summary>
  private List<Border> borders;
  public IEnumerable<Border> Borders()
  {
    foreach (var d in Directions.Each()) {
      foreach (var b in borders) {
        if (b.Direction == d) {
          yield return b;
        }
      }
    }
  }
  public void AddBorder(Border b)
  {
    if (!borders.Contains(b)) {
      borders.Add(b);
      b.Region = this;
    }
  }
  public bool ContainsBorder(Border b)
  {
    return borders.Contains(b);
  }
  public void TryRemoveBorder(Border b)
  {
    if (borders.Contains(b)) { borders.Remove(b); }
  }
}
