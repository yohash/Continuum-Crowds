using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
  public string Filename = "Test";
  public void SetFilename(string filename) { Filename = filename; }

  public int TileSize = 25;

  public List<MapTile> Tiles;

  // terrain fields
  private float[,] h;
  private float[,] g;
  private Vector2[,] dh;

  // viewables
  private bool viewTiles;
  private bool viewRegions;
  private bool viewBorders;
  private bool viewNeighborBorders;
  public void ViewTiles(bool show) { viewTiles = show; }
  public void ViewRegions(bool show) { viewRegions = show; }
  public void ViewBorders(bool show) { viewBorders = show; }
  public void ViewNeighbors(bool show) { viewNeighborBorders = show; }
  public void ViewTileIndex(string i)
  {
    if (int.TryParse(i, out int index)) { tileIndex = index; }
  }
  public void ShuffleColors() { borderColors.Clear(); }

  private int tileIndex;

  private List<Color> borderColors = new List<Color>();

  // ***************************************************************************
  //  MONOBEHAVIOURS
  // ***************************************************************************
  private void Update()
  {
    float dy = 0.1f;

    if (viewTiles && Tiles.Count > tileIndex && tileIndex >= 0) {
      var tile = Tiles[tileIndex];

      float height(int x, int y) { return tile.Height(x, y) + dy; }
      for (int x = tile.Corner.x; x < tile.Corner.x + tile.TileSize - 1; x++) {
        for (int y = tile.Corner.y; y < tile.Corner.y + tile.TileSize - 1; y++) {
          drawSquare(
            new Vector3(x, height(x, y), y),
            new Vector3(x + 1, height(x + 1, y), y),
            new Vector3(x + 1, height(x + 1, y + 1), y + 1),
            new Vector3(x, height(x, y + 1), y + 1),
            Color.green
          );
        }
      }
    }

    if (viewRegions) {
      foreach (var tile in Tiles) {
        foreach (var region in tile.Regions) {
          foreach (var location in region.Locations()) {
            // handy call shortener
            float height(Vector2Int v) { return tile.Height(v.x, v.y); }

            var hrgt = location.ToXYZ(height(location) + dy);
            drawSquare(hrgt, Color.red);
          }
        }
      }
    }

    if (viewBorders) {
      int i = 0;
      foreach (var tile in Tiles) {
        foreach (var border in tile.Borders) {
          if (i + 1 > borderColors.Count) {
            borderColors.Add(new Color(Random.value, Random.value, Random.value));
          }
          var c = borderColors[i++];
          drawBorder(border, c);
        }
      }
    }

    if (viewNeighborBorders) {
      int i = 0;
      var allBorders = new List<Border>();
      foreach (var tile in Tiles) {
        foreach (var border in tile.Borders) {
          allBorders.Add(border);
        }
      }

      while (allBorders.Count > 0) {
        if (i + 1 > borderColors.Count) {
          borderColors.Add(new Color(Random.value, Random.value, Random.value));
        }
        var c = borderColors[i++];
        var border = allBorders[0];
        drawBorder(border, c);
        foreach (var neighbor in border.GetNeighbors()) {
          if (allBorders.Contains(neighbor)) {
            // draw the neighbors
            drawBorder(neighbor, c);
            allBorders.Remove(neighbor);
          }
        }
        allBorders.Remove(border);
      }
    }
  }

  private void drawBorder(Border border, Color c)
  {
    float dy = 0.1f;
    foreach (var location in border.GetLocations()) {
      float height(Vector2Int v)
      {
        return border.Tile.Height(v.x, v.y);
      }
      var hgt = location.ToXYZ(height(location) + dy);
      drawSquare(hgt, c);
    }
  }

  private void drawSquare(Vector3 corner, Color c)
  {
    Debug.DrawLine(corner, corner + Vector3.forward, c);
    Debug.DrawLine(corner + Vector3.forward, corner + Vector3.forward + Vector3.right, c);
    Debug.DrawLine(corner + Vector3.forward + Vector3.right, corner + Vector3.right, c);
    Debug.DrawLine(corner + Vector3.right, corner, c);
  }

  private void drawSquare(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, Color c)
  {
    Debug.DrawLine(c1, c2, c);
    Debug.DrawLine(c2, c3, c);
    Debug.DrawLine(c3, c4, c);
    Debug.DrawLine(c4, c1, c);
  }

  // ***************************************************************************
  //  TILE GENERATION
  // ***************************************************************************
  public void GenerateCCTiles()
  {
    if (h.GetLength(0) != g.GetLength(0) || g.GetLength(0) != dh.GetLength(0) ||
        h.GetLength(1) != g.GetLength(1) || g.GetLength(1) != dh.GetLength(1)) {
      Debug.LogError("Cannot generate Tiles, dimensions disagree. Re-load heightmaps.");
      return;
    }

    int sizeX = h.GetLength(0);
    int sizeY = h.GetLength(1);

    Debug.Log($"Generating tiles (size = {TileSize}) for height map dimension: {sizeX} x {sizeY}");
    int numTilesX = Mathf.CeilToInt((float)sizeX / (float)TileSize);
    int numTilesY = Mathf.CeilToInt((float)sizeY / (float)TileSize);

    Debug.Log($"\t x-tiles ({numTilesX}), y-tiles ({numTilesY}), total ({numTilesX * numTilesY})");
    Tiles = new List<MapTile>();
    for (int x = 0; x < numTilesX; x++) {
      for (int y = 0; y < numTilesY; y++) {
        Vector2Int corner = new Vector2Int(x * TileSize, y * TileSize);
        Tiles.Add(new MapTile(
              corner,
              h.SubMatrix(x * TileSize, y * TileSize, TileSize, TileSize),
              g.SubMatrix(x * TileSize, y * TileSize, TileSize, TileSize),
              dh.SubMatrix(x * TileSize, y * TileSize, TileSize, TileSize)
        ));
      }
    }

    Debug.Log($"\tPairing up tiles with neighbors...");
    foreach (var tile in Tiles) {
      foreach (var neighbor in Tiles) {
        if (tile != neighbor) {
          if ((tile.Corner - neighbor.Corner).magnitude == TileSize) {
            // this tile is within one TileSize, it is a neighbor
            var dir = (neighbor.Corner - tile.Corner).ToDirection();
            tile.NeighborTiles[dir] = neighbor;
          } else if (
              Mathf.Abs(tile.Corner.x - neighbor.Corner.x) == TileSize &&
              Mathf.Abs(tile.Corner.y - neighbor.Corner.y) == TileSize
          ) {
            // this tile is a diagonal neighbor
            //tile.DiagonalNeighbors.Add(neighbor);
          }
        }
      }
    }

    // now that tiles are generated, build all connections between tiles
    foreach (var tile in Tiles) {
      Debug.Log($"Merging tile borders for tile {tile.Corner}...");
      tile.MergeBorders();
    }
    foreach (var tile in Tiles) {
      Debug.Log($"Assembling neighbors for {tile.Corner}...");
      tile.ConnectBordersToNeighbors();
    }
  }

  // ***************************************************************************
  //  FILE IO
  // ***************************************************************************
  public void LoadCsvFiles()
  {
    string path = $"{FileUtility.PATH}/{FileUtility.DATA_FOLDER}/{Filename}/{FileUtility.CSV_FOLDER}/";
    Debug.Log("Loading files at: " + path + Filename + "_*");

    h = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_H.txt");
    Debug.Log($"\tSuccessfully loaded height map, h: {h.GetLength(0)}x{h.GetLength(1)}");

    g = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_g.txt");
    Debug.Log($"\tSuccessfully loaded discomfort, g: {g.GetLength(0)}x{g.GetLength(1)}");

    float[,] dhdx = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_dHdx.txt");
    float[,] dhdy = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_dHdy.txt");

    // populate matrix dh
    dh = new Vector2[dhdx.GetLength(0), dhdx.GetLength(1)];
    for (int i = 0; i < dhdx.GetLength(0); i++) {
      for (int k = 0; k < dhdx.GetLength(1); k++) {
        dh[i, k] = new Vector2(dhdx[i, k], dhdy[i, k]);
      }
    }

    Debug.Log($"\tSuccessfully assembled height gradient, dh: {dh.GetLength(0)}x{dh.GetLength(1)}");
  }

  public void SerializeAndSaveTiles()
  {
    /// TBD
  }
}
