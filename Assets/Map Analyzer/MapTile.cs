using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapTile
{
  [SerializeField] Vector2Int corner;
  public Vector2Int Corner { get { return corner; } private set { corner = value; } }

  public List<Region> BorderRegions;
  public List<MapTile> NeighborTiles;

  private float[,] h;
  private float[,] g;
  private Vector2[,] dh;


  public float[,] Height { get { return h; } }
  public float[,] Discomfort { get { return g; } }

  public MapTile(Vector2Int corner, float[,] h, float[,] g, Vector2[,] dh)
  {
    this.h = h;
    this.g = g;
    this.dh = dh;
    this.Corner = corner;

    selfAssemble();
  }

  private void selfAssemble()
  {
    BorderRegions = new List<Region>();

    // cache
    Region region = new Region(this);

    // this algorithm assumes (0,0) is the upper-left of the h[,] matrix
    //    - GetLength(0) is the length in the x direction
    //    - GetLength(1) is the length in the y direction
    int x;
    int y;

    // trigger to help build continuous regions
    bool valid = false;

    // scan the top
    y = 0;
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        if (valid) {
          BorderRegions.Add(region);
          region = new Region(this);
        }
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
    // scan the bottom
    region = new Region(this);
    y = g.GetLength(1) - 1;
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        if (valid) {
          BorderRegions.Add(region);
          region = new Region(this);
        }
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
    // scan the left
    region = new Region(this);
    x = 0;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        if (valid) {
          BorderRegions.Add(region);
          region = new Region(this);
        }
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
    // scan the right
    region = new Region(this);
    x = g.GetLength(0) - 1;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        if (valid) {
          BorderRegions.Add(region);
          region = new Region(this);
        }
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
  }
}
