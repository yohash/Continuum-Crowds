using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MapTile
{
  public Location Corner { get { return corner; } private set { corner = value; } }
  [SerializeField] private Location corner;

  public int TileSize { get { return tileSize; } }
  [SerializeField] private int tileSize;

  // public tile contents
  [NonSerialized] public List<Border> Borders;
  public Dictionary<DIRECTION, MapTile> NeighborTiles;

  // defining tile data
  private float[,] h;
  private float[,] g;
  private Vector2[,] dh;

  public MapTile(Location corner, float[,] h, float[,] g, Vector2[,] dh)
  {
    this.h = h;
    this.g = g;
    this.dh = dh;
    this.corner = corner;

    tileSize = h.GetLength(0);

    NeighborTiles = new Dictionary<DIRECTION, MapTile>();

    assembleTile();
  }

  // ***************************************************************************
  //  Tile Acecssors
  // ***************************************************************************
  public bool ContainsPoint(Vector2 v)
  {
    return ContainsPoint(v.x, v.y);
  }
  public bool ContainsPoint(Location location)
  {
    return ContainsPoint(location.x, location.y);
  }
  public bool ContainsPoint(float x, float y)
  {
    return x < corner.x + tileSize &&
           x >= corner.x &&
           y < corner.y + tileSize &&
           y >= corner.y;
  }
  public bool IsPathable(Location l)
  {
    return Discomfort(l.x, l.y) >= 1;
  }
  public float Height(int x, int y)
  {
    return ContainsPoint(x, y) ? h[x - corner.x, y - corner.y] : float.MinValue;
  }
  public float Height(Location l)
  {
    return Height(l.x, l.y);
  }
  public float Discomfort(int x, int y)
  {
    return ContainsPoint(x, y) ? g[x - corner.x, y - corner.y] : float.MaxValue;
  }

  // ***************************************************************************
  //  PUBLIC TILE MANAGEMENT TOOLS
  // ***************************************************************************
  public void ConnectBordersToNeighbors()
  {
    // iterate over all borders
    foreach (var border in Borders) {
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
            border.AddNeighbor(confirmed, 1);
          }
        }
      }
    }
  }

  public void PurgeBorders()
  {
    var deleteBorders = new List<Border>();
    // iterate over all borders in this region
    foreach (var border in Borders) {
      // get the neighboring tile in the same direction as this border
      if (!NeighborTiles.TryGetValue(border.Direction, out var tile)) {
        // there is no neighboring tile. Delete this border, as it borders nothing
        deleteBorders.Add(border);
        continue;
      }
      // see if there are any neighbors, if not, delete this border
      if (border.Neighbors().Count() == 0) {
        deleteBorders.Add(border);
        continue;
      }
    }

    // delete all borders found to be irrelevant
    foreach (var delete in deleteBorders) {
      // reomve the border from this tile
      Borders.Remove(delete);
    }
  }

  // ***************************************************************************
  //  PRIVATE METHODS
  // ***************************************************************************
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
        border.AddLocation(new Location(x, y) + corner);
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
        border.AddLocation(new Location(x, y) + corner);
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
        border.AddLocation(new Location(x, y) + corner);
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
        border.AddLocation(new Location(x, y) + corner);
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
    var regions = new List<List<Location>>();
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
      var region = new List<Location>();
      // fill known locations with the entire seed border
      foreach (var loc in seed.GetLocations()) { region.Add(loc); }

      // create a queue we'll pull locations from
      var testLocations = new Queue<Location>();
      // seed with location at [0]
      testLocations.Enqueue(region.First());

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
              foreach (var loc in newBorder.GetLocations()) { region.Add(loc); }
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
          if (region.Contains(neighbor)) {
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
        region.Add(currentLocation);
      }

      // region has been flooded, and all connected borders tagged
      //foreach (var border in currentGroup) {
      //  region.AddBorder(border);
      //}
      // save the region
      regions.Add(region);
      // all connected borders should have been
      // added to the currentGroup. Start a new group
      groupedBorders.Add(currentGroup);
      currentGroup = new List<Border>();
    }
  }
}
