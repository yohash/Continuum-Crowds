using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapTile
{
  private float[,] h;
  private float[,] g;
  private Vector2[,] dh;

  [SerializeField] private Vector2 corner;

  public List<Region> BorderRegions;
  public List<MapTile> NeighborTiles;

  public Texture2D HeightMap;
  public Texture2D GradientMap;
  public Texture2D DiscomfortMap;

  public MapTile(Vector2 corner, float[,] h, float[,] g, Vector2[,] dh)
  {
    this.h = h;
    this.g = g;
    this.dh = dh;
    this.corner = corner;

    HeightMap = TextureGenerator.TextureFromMatrix(h);
    DiscomfortMap = TextureGenerator.TextureFromMatrix(g);
    GradientMap = TextureGenerator.TextureFromMatrix(dh);

    selfAssemble();
  }

  private void selfAssemble()
  {
    BorderRegions = new List<Region>();

    // this algorithm assumes (0,0) is the upper-left of the h[,] matrix
    //    - GetLength(0) is the length in the x direction
    //    - GetLength(1) is the length in the y direction
    int x;
    int y;

    bool valid = false;
    Region region = new Region();


    // scan the top
    y = 0;
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        BorderRegions.Add(region);
        region = new Region();
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
    // scan the bottom
    region = new Region();
    y = g.GetLength(1) - 1;
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        BorderRegions.Add(region);
        region = new Region();
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
    // scan the left
    region = new Region();
    x = 0;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        BorderRegions.Add(region);
        region = new Region();
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
    // scan the right
    region = new Region();
    x = g.GetLength(0) - 1;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        region.AddLocation(new Vector2(x, y));
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this region
        BorderRegions.Add(region);
        region = new Region();
        valid = false;
      }
    }
    if (valid) { BorderRegions.Add(region); }
  }
}
