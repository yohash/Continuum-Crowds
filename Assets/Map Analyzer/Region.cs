using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
  private List<Vector2Int> locations;
  private List<Border> borders;

  public Region()
  {
    locations = new List<Vector2Int>();
    borders = new List<Border>();
  }

  public void AddLocation(Vector2Int v)
  {
    if (!locations.Contains(v)) { locations.Add(v); }
  }

  public bool ContainsLocation(Vector2Int v)
  {
    return locations.Contains(v);
  }

  public Vector2Int GetFirst()
  {
    if (locations.Count > 0) { return locations[0]; }
    return Vector2Int.zero;
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

  public IEnumerable<Border> Borders()
  {
    foreach (var b in borders) {
      yield return b;
    }
  }
}
