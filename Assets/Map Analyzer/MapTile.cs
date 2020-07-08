using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MapTile
{
  [SerializeField] Vector2Int corner;
  public Vector2Int Corner { get { return corner; } private set { corner = value; } }

  public List<Border> Borders;
  public List<MapTile> NeighborTiles;
  public List<MapTile> DiagonalNeighbors;

  private int tileSize;

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

    tileSize = h.GetLength(0);

    NeighborTiles = new List<MapTile>();
    DiagonalNeighbors = new List<MapTile>();

    assemble();
  }

  public void AssembleInterconnects()
  {
    assembleNeighborConnections();
  }

  private void assemble()
  {
    assembleBorders();
    assembleInternalConnections();
  }

  private void assembleBorders()
  {
    Borders = new List<Border>();

    // cache
    Border border = new Border(this);

    // this algorithm assumes (0,0) is the upper-left of the h[,] matrix
    //    - GetLength(0) is the length in the x direction
    //    - GetLength(1) is the length in the y direction
    int x;
    int y;

    // trigger to help build continuous borders
    bool valid = false;

    // scan the top
    y = 0;
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this);
        }
        valid = false;
      }
    }
    if (valid) { Borders.Add(border); }
    // scan the bottom
    border = new Border(this);
    y = g.GetLength(1) - 1;
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this);
        }
        valid = false;
      }
    }
    if (valid) { Borders.Add(border); }
    // scan the left
    border = new Border(this);
    x = 0;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this);
        }
        valid = false;
      }
    }
    if (valid) { Borders.Add(border); }
    // scan the right
    border = new Border(this);
    x = g.GetLength(0) - 1;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this);
        }
        valid = false;
      }
    }
    if (valid) { Borders.Add(border); }
  }

  private void assembleInternalConnections()
  {
    // init our grouping tracker
    var groupedBorders = new List<List<Border>>();
    // track our current group
    var currentGroup = new List<Border>();
    // copy our list of borders
    // we'll remove borders from this list as they are connected
    var unknownBorders = new List<Border>();
    unknownBorders.AddRange(Borders);

    // remove borders from this list as they are connected
    while (unknownBorders.Count > 0) {
      // seed with the border at [0]
      var seed = unknownBorders[0];
      unknownBorders.RemoveAt(0);
      // start this group with this border
      currentGroup.Add(seed);

      // create a list of known locations that we'll fill up as we go
      var knownLocations = new List<Vector2Int>();
      // fill known locations with the entire seed border
      foreach (Vector2Int loc in seed.GetLocations()) { knownLocations.Add(loc); }

      // create a queue we'll pull locations from
      var testLocations = new Queue<Vector2Int>();
      // seed with location at [0]
      testLocations.Enqueue(knownLocations[0]);

      // begin to flood the borders
      while (testLocations.Count > 0) {
        var currentLocation = testLocations.Dequeue();
        // is this new test location part of a border?
        if (Borders.Any(border => border.Contains(currentLocation))) {
          // if so, is this border allready in the current group of borders?
          if (currentGroup.Any(border => border.Contains(currentLocation))) {
            // this border is already tracked in the current group, do nothing
          } else {
            // if not, then this is a new border to add to the group!
            foreach (var newBorder in Borders.Where(border => border.Contains(currentLocation))) {
              // add to current group
              currentGroup.Add(newBorder);
              // add all locations to known locations
              foreach (Vector2Int loc in newBorder.GetLocations()) { knownLocations.Add(loc); }
              // remove from our unknown borders list
              if (unknownBorders.Contains(newBorder)) {
                unknownBorders.RemoveAt(unknownBorders.IndexOf(newBorder));
              }
            }
          }
        }

        // add all the neighbors of this test location, subject to conditions
        foreach (var direction in CCvals.ENSWint) {
          var neighbor = currentLocation + direction;
          var internalLoc = neighbor - corner;
          // make sure this neighbor is not off the tile
          if (internalLoc.x >= tileSize ||
              internalLoc.x  < 0 ||
              internalLoc.y >= tileSize ||
              internalLoc.y < 0) {
            continue;
          }
          // make sure this neighbor is not already a known location
          if (knownLocations.Contains(neighbor)) {
            continue;
          }
          // make sure this border isn't already in our test queue
          if (testLocations.Contains(neighbor)) {
            continue;
          }
          // make sure this neighbor is not an impassable border
          if (g[neighbor.x - corner.x, neighbor.y - corner.y] >= 1) {
            continue;
          }
          // add this neighbor to test locations!
          testLocations.Enqueue(neighbor);
        }

        // now that we've tested all neighbors, add this node to known locations
        knownLocations.Add(currentLocation);
      }

      // border has been flooded, and all connected border borders should have been
      // added to the currentGroup. Start a new group
      groupedBorders.Add(currentGroup);
      currentGroup = new List<Border>();
    }

    // store all internal connections to borders
    foreach (var borderGroup in groupedBorders) {
      foreach (var border in borderGroup) {
        foreach (var connection in borderGroup) {
          border.AddConnection(connection);
        }
      }
    }
  }

  private void assembleNeighborConnections()
  {
    // start with a border
    foreach (var border in Borders) {
      // foreach neighbor tile
      foreach (var tile in NeighborTiles) {
        // cycle through each of their borders
        foreach (var testBorder in tile.Borders) {
          // if any point in any border is within 1-space from this border, it is connecting
          // cycle through each point in this border
          foreach (var location in border.GetLocations()) {
            // cycle through each NSEW direction
            foreach (var direction in CCvals.ENSWint) {
              var testLocation = location + direction;

            }
          }
        }
      }
    }
  }
}
