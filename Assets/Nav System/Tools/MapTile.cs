using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MapTile
{
  /// <summary>
  /// The integer-location of the corner of this tile
  /// </summary>
  public Location Corner { get { return corner; } }
  [SerializeField] private Location corner;

  /// <summary>
  /// The (square) size of this tile
  /// </summary>
  public int TileSize { get { return tileSize; } }
  [SerializeField] private int tileSize;

  /// <summary>
  /// The borders of this tile
  /// </summary>
  public List<Border> Borders { get { return borders; } }
  [NonSerialized] private List<Border> borders;

  private Dictionary<DIRECTION, MapTile> neighborTiles;

  // defining tile data
  private float[,] h;
  private float[,] g;
  private Vector2[,] dh;

  /// <summary>
  /// Construct this map tile with the height map
  /// </summary>
  /// <param name="corner"></param>
  /// <param name="h"></param>
  /// <param name="g"></param>
  /// <param name="dh"></param>
  public MapTile(Location corner, float[,] h, float[,] g, Vector2[,] dh)
  {
    this.h = h;
    this.g = g;
    this.dh = dh;
    this.corner = corner;

    tileSize = h.GetLength(0);

    neighborTiles = new Dictionary<DIRECTION, MapTile>();

    assembleBorders();
  }

  public override string ToString()
  {
    return "Tile: " + corner;
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
    // TODO: Replace this with a ref to a public global var that
    //        defines the max pathable slope
    return Discomfort(l.x, l.y) < 1;
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
  /// <summary>
  /// STEP 1 - Add all neighbors to this tile
  /// </summary>
  /// <param name="neighbor"></param>
  /// <param name="d"></param>
  public void AddNeighbor(MapTile neighbor, DIRECTION d)
  {
    neighborTiles[d] = neighbor;
  }

  /// <summary>
  /// STEP 2 - Connect borders to neighboring borders AFTER all neighboring
  /// tiles have been added
  /// </summary>
  public void ConnectBordersAcrossTiles()
  {
    // iterate over all borders
    foreach (var border in Borders) {
      // get the neighboring tile in the same direction as this border
      if (neighborTiles.TryGetValue(border.Direction, out var tile)) {
        // get this tile's borders that align with the edge of the border in question
        var opposingBorders = tile.Borders.Where(b => b.Direction == border.Direction.Opposite());
        var dir = border.Direction.ToVector();

        // iterate over all locations in this border being tested
        foreach (var location in border.GetLocations()) {
          // collect the borders that are adjacent to the border being tested
          var neighbor = opposingBorders.Where(b => b.Contains(location + dir));
          foreach (var confirmed in neighbor) {
            border.AddNeighbor(confirmed, 1, new List<Location>() { border.Center, confirmed.Center });
          }
        }
      }
    }
  }

  /// <summary>
  /// STEP 3 - Delete all borders with (a) no neighboring tile -or-
  /// (b) no neighboring border IN their neighboring tile
  /// </summary>
  public void PurgeBorders()
  {
    var deleteBorders = new List<Border>();
    // iterate over all borders in this region
    foreach (var border in Borders) {
      // get the neighboring tile in the same direction as this border
      if (!neighborTiles.ContainsKey(border.Direction)) {
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

  /// <summary>
  /// STEP 4 - connect all remaing borders internally, completing
  /// the border mesh
  /// </summary>
  public async Task AssembleInternalBorderMesh()
  {
    var pathTasks = new List<Task>();
    foreach (var b1 in Borders) {
      foreach (var b2 in Borders.Where(b => !b.Equals(b1))) {
        pathTasks.Add(
          Task.Run(() => {
            // get border's central location and search from it
            var loc1 = b1.Center;
            var loc2 = b2.Center;
            var aStar = new AStarSearch();

            // perform the search, and record the cost with the neighbors
            aStar.ComputePath(loc1, loc2, this, (successful, path, cost) => {
              if (successful) {
                b1.AddNeighbor(b2, cost, path);
                b2.AddNeighbor(b1, cost, path);
              }
            });
          })
        );
      }
    }
    // wait for the pathfinding to complete
    await Task.WhenAll(pathTasks);
  }

  // ***************************************************************************
  //  PRIVATE METHODS
  // ***************************************************************************
  private void assembleBorders()
  {
    borders = new List<Border>();
    // trigger to help build continuous borders
    bool valid = false;

    // create inline method to simplify the different scan direction below
    void scan(IEnumerable<int> xLocations, IEnumerable<int> yLocations, DIRECTION direction)
    {
      var border = new Border(this, direction);
      foreach (var x in xLocations) {
        foreach (var y in yLocations) {
          if (g[x, y] < 1) {
            border.AddLocation(new Location(x, y) + corner);
            valid = true;
          } else {
            // sharp break in discomfort, unpassable location, close off this border
            if (valid) {
              Borders.Add(border);
              border = new Border(this, direction);
            }
            valid = false;
          }
        }
      }
      // after the edge was scanned, see if we add this final border up to the corner
      if (valid) { Borders.Add(border); }
    }

    // scan the bottom (y=0, SOUTH)
    scan(Enumerable.Range(0, g.GetLength(0)), Enumerable.Range(0, 1), DIRECTION.SOUTH);
    // scan the top (y=length, NORTH)
    scan(Enumerable.Range(0, g.GetLength(0)), Enumerable.Range(g.GetLength(1) - 1, 1), DIRECTION.NORTH);
    // scan the left (x=0, WEST)
    scan(Enumerable.Range(0, 1), Enumerable.Range(0, g.GetLength(1)), DIRECTION.WEST);
    // scan the right (x=length, EAST)
    scan(Enumerable.Range(g.GetLength(0) - 1, 1), Enumerable.Range(0, g.GetLength(1)), DIRECTION.EAST);
  }
}
