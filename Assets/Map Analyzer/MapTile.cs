using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MapTile
{
  [SerializeField] Vector2Int corner;
  public Vector2Int Corner { get { return corner; } private set { corner = value; } }

  public List<Region> BorderRegions;
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

  private void assemble()
  {
    assembleRegions();
    assembleInternalConnections();
    assembleNeighborConnections();
  }

  private void assembleRegions()
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
        region.AddLocation(new Vector2(x, y) + corner);
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
        region.AddLocation(new Vector2(x, y) + corner);
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
        region.AddLocation(new Vector2(x, y) + corner);
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
        region.AddLocation(new Vector2(x, y) + corner);
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

  private void assembleInternalConnections()
  {
    // init our grouping tracker
    var groupedRegions = new List<List<Region>>();
    // track our current group
    var currentGroup = new List<Region>();
    // copy our list of regions
    // we'll remove regions from this list as they are connected
    var unknownRegions = new List<Region>();
    unknownRegions.AddRange(BorderRegions);

    // remove regions from this list as they are connected
    while (unknownRegions.Count > 0) {
      // seed with the region at [0]
      var seedRegion = unknownRegions[0];
      unknownRegions.RemoveAt(0);
      // start this group with this region
      currentGroup.Add(seedRegion);

      // create a list of known locations that we'll fill up as we go
      var knownLocations = new List<Vector2Int>();
      // fill known locations with the entire seed region
      foreach (Vector2Int loc in seedRegion.GetLocations()) { knownLocations.Add(loc); }

      // create a queue we'll pull locations from
      var testLocations = new Queue<Vector2Int>();
      // seed with location at [0]
      testLocations.Enqueue(knownLocations[0]);

      // begin to flood the regions
      while (testLocations.Count > 0) {
        var currentLocation = testLocations.Dequeue();
        // is this new test location part of a region?
        if (BorderRegions.Any(region => region.Contains(currentLocation))) {
          // if so, is this region allready in the current group of regions?
          if (currentGroup.Any(region => region.Contains(currentLocation))) {
            // this region is already tracked in the current group, do nothing
          } else {
            // if not, then this is a new region to add to the group!
            foreach (var newRegion in BorderRegions.Where(region => region.Contains(currentLocation))) {
              // add to current group
              currentGroup.Add(newRegion);
              // add all locations to known locations
              foreach (Vector2Int loc in newRegion.GetLocations()) { knownLocations.Add(loc); }
              // remove from our unknown regions list
              if (unknownRegions.Contains(newRegion)) {
                unknownRegions.RemoveAt(unknownRegions.IndexOf(newRegion));
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
          // make sure this region isn't already in our test queue
          if (testLocations.Contains(neighbor)) {
            continue;
          }
          // make sure this neighbor is not an impassable region
          if (g[neighbor.x - corner.x, neighbor.y - corner.y] >= 1) {
            continue;
          }
          // add this neighbor to test locations!
          testLocations.Enqueue(neighbor);
        }

        // now that we've tested all neighbors, add this node to known locations
        knownLocations.Add(currentLocation);
      }

      // region has been flooded, and all connected border regions should have been
      // added to the currentGroup. Start a new group
      groupedRegions.Add(currentGroup);
      currentGroup = new List<Region>();
    }

    // store all internal connections to regions
    foreach (var regionGroup in groupedRegions) {
      foreach (var region in regionGroup) {
        foreach (var connection in regionGroup) {
          region.AddConnection(connection);
        }
      }
    }
  }

  private void assembleNeighborConnections()
  {
    // start with a region
    foreach (var region in BorderRegions) {
      // foreach neighbor tile
      foreach (var tile in NeighborTiles) {
        // cycle through each of their regions
        foreach (var testRegion in tile.BorderRegions) {
          // if any point in any region is within 1-space from this region, it is connecting
          // cycle through each point in this region
          foreach (var location in region.GetLocations()) {
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
