using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MapTile
{
  [SerializeField] Vector2Int corner;
  public Vector2Int Corner { get { return corner; } private set { corner = value; } }

  public List<Border> Borders;
  public Dictionary<DIRECTION, MapTile> NeighborTiles;

  public List<Region> Regions;

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

    NeighborTiles = new Dictionary<DIRECTION, MapTile>();

    assembleTile();
  }

  public void AssembleInterconnects()
  {
    //collapseBorders();
    assembleBorderNeighbors();
  }

  private void assembleTile()
  {
    assembleBorders();
    assembleInternalConnections();
  }

  private void assembleBorders()
  {
    Borders = new List<Border>();

    // cache
    Border border;
    int x, y;

    // trigger to help build continuous borders
    bool valid = false;

    // scan the bottom (y=0, SOUTH)
    y = 0;
    border = new Border(this, DIRECTION.SOUTH);
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this, DIRECTION.SOUTH);
        }
        valid = false;
      }
    }
    if (valid) { Borders.Add(border); }
    // scan the top (y=length, NORTH)
    border = new Border(this, DIRECTION.NORTH);
    y = g.GetLength(1) - 1;
    for (x = 0; x < g.GetLength(0); x++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this, DIRECTION.NORTH);
        }
        valid = false;
      }
    }
    if (valid) { Borders.Add(border); }
    // scan the left (x=0, WEST)
    border = new Border(this, DIRECTION.WEST);
    x = 0;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this, DIRECTION.WEST);
        }
        valid = false;
      }
    }
    if (valid) { Borders.Add(border); }
    // scan the right (x=length, EAST)  
    border = new Border(this, DIRECTION.EAST);
    x = g.GetLength(0) - 1;
    for (y = 0; y < g.GetLength(1); y++) {
      if (g[x, y] < 1) {
        border.AddLocation(new Vector2(x, y) + corner);
        valid = true;
      } else {
        // sharp break in discomfort, unpassable location, close off this border
        if (valid) {
          Borders.Add(border);
          border = new Border(this, DIRECTION.EAST);
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
    // init new list of regions
    Regions = new List<Region>();
    // tracker for our current group
    var currentGroup = new List<Border>();
    // copy our list of borders
    var unknownBorders = new List<Border>(Borders);

    // remove borders from this list as they are connected
    while (unknownBorders.Count > 0) {
      // seed with the border at [0]
      var seed = unknownBorders[0];
      unknownBorders.RemoveAt(0);
      // start this group with this border
      currentGroup.Add(seed);

      // create a list of known locations that we'll fill up as we go
      var region = new Region();
      // fill known locations with the entire seed border
      foreach (var loc in seed.GetLocations()) { region.AddLocation(loc); }

      // create a queue we'll pull locations from
      var testLocations = new Queue<Vector2Int>();
      // seed with location at [0]
      testLocations.Enqueue(region.GetFirst());

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
              foreach (var loc in newBorder.GetLocations()) { region.AddLocation(loc); }
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
              internalLoc.x < 0 ||
              internalLoc.y >= tileSize ||
              internalLoc.y < 0) {
            continue;
          }
          // make sure this neighbor is not already a known location
          if (region.ContainsLocation(neighbor)) {
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
        region.AddLocation(currentLocation);
      }

      // region has been flooded, and all connected borders tagged
      foreach (var border in currentGroup) {
        region.AddBorder(border);
      }
      // save the region
      Regions.Add(region);
      // all connected borders should have been
      // added to the currentGroup. Start a new group
      groupedBorders.Add(currentGroup);
      currentGroup = new List<Border>();
    }
  }

  /// <summary>
  /// 1. For each internal region, look for borders in matching directions
  /// 2. Then, look at the neighbor tile in the direction of these two borders
  /// 3. From said neighbor, find the two borders that match up with these borders
  /// 4. See if the NEIGHBORS borders are from the same [Region]
  /// 5. If so, then the two borders in this region are continuous, and can be merged
  /// </summary>
  private void collapseBorders()
  {
    // iterate over every region
    foreach (var region in Regions) {
      // iterate over all borders in this region
      foreach (var border in region.Borders()) {
        // for every border, iterate over all other borders in the same direction
        // (that aren't this same <border> being tested)
        foreach (var cardinal in region.Borders().Where(b => b.Direction == border.Direction)) {
          // get the tile in the direction of these two borders
          if (NeighborTiles.TryGetValue(border.Direction, out var neighbor)) {
            // and get the two neighboring borders opposing these borders

          }
        }
      }
    }

    // conditions fulfilled:
    //   1 - two borders on this tile share the same region
    //   2 - these two borders share the same avg x-or-y (on same side of the tile)
    //   3 - both borders have an accompanying border on the neighboring tile
    //   4 - these accompanying borders are members of the same region on their tile
  }

  private void assembleBorderNeighbors()
  {
    var deleteBorders = new List<Border>();
    // iterate over every region
    foreach (var region in Regions) {
      // iterate over all borders in this region
      foreach (var border in region.Borders()) {
        // get the neighboring tile in the same direction as this border
        if (NeighborTiles.TryGetValue(border.Direction, out var tile)) {
          // get this tile's borders that align with the edge of the border in question
          var opposingBorders = tile.Borders.Where(b => b.Direction == border.Direction.Opposite());
          var dir = border.Direction.ToVector();

          // iterate over all locations in this border being tested
          foreach (var location in border.GetLocations()) {
            // collect the borders that are adjacent to the border being tested
            var neighbor = opposingBorders.Where(b => b.Contains(location + dir));
            foreach (var confirmed in neighbor) {
              border.AddNeighbor(confirmed);
            }
          }
        } else {
          // there is no neighboring tile. Delete this border, as it borders nothing
          deleteBorders.Add(border);
        }
      }
    }

    // delete all borders found to be irrelevant
    foreach (var delete in deleteBorders) {
      // reomve the border from this tile
      Borders.Remove(delete);
      // remove the border from its region
      delete.Region.TryRemoveBorder(delete);
    }
  }
}
