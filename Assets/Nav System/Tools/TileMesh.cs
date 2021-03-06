﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class TileMesh
{
  private Dictionary<Location, Portal> mesh;

  public TileMesh(List<MapTile> tiles)
  {
    mesh = new Dictionary<Location, Portal>();
    // create all nodes of the mesh from borders
    foreach (var tile in tiles) {
      foreach (var border in tile.Borders) {
        mesh[border.Center] = new Portal(border);
      }
    }

    // connect all nodes
    foreach (var tile in tiles) {
      foreach (var border in tile.Borders) {
        foreach (var neighbor in border.Neighbors()) {
          var portal = mesh[border.Center];
          var connection = mesh[neighbor.AsLocation()];
          portal.AddConnection(connection, border.Cost(neighbor));
        }
      }
    }
  }

  public async Task FindConnectedPortals(
    Location location,
    MapTile tile,
    Dictionary<Portal, float> seeds
  )
  {
    var portals = mesh.Values.Where(portal => portal.tile1 == tile || portal.tile2 == tile);

    // track path tasks for awaiting
    var pathTasks = new List<Task>();
    // determine which portals are connected
    int tot = portals.Count();

    foreach (var portal in portals) {
      // compute the path cost to each neighbor border
      var newTask = Task.Run(() => {
        var aStar = new AStarSearch();
        // perform the search, and record the cost with the neighbors
        aStar.ComputePath(location, portal.Center, tile, (success, path, cost) => {
          // path finding was a success, store this portal
          if (success) { seeds[portal] = cost; }
        });
      });
      // track the pathfinding tasks
      pathTasks.Add(newTask);
    }

    await Task.WhenAll(pathTasks);
  }

  public override string ToString()
  {
    return $"{GetType()}: {string.Join("\n", mesh)}";
  }
}

public struct Portal : IPathable
{
  // Portal properties
  public Location Center { get; private set; }
  public int Width { get; private set; }

  // linking data
  public MapTile tile1 { get; private set; }
  public MapTile tile2 { get; private set; }

  // IPathable cost dictionary
  private Dictionary<IPathable, float> costByNode;

  // *******************************************************************
  //    Portal
  // *******************************************************************
  public Portal(Border b)
  {
    // get the maptile of this border
    tile1 = b.Tile;

    // assign tiles
    // get any neighbor whose cost is 1
    tile2 = b.Neighbors()
        .Where(n => b.Cost(n) == 1)
        .Cast<Border>()
        .FirstOrDefault()?.Tile;

    if (tile2 == null) {
      UnityEngine.Debug.LogError("Portal has no neighboring border");
    }

    Center = b.Center;
    Width = b.GetLocations().Count();

    costByNode = new Dictionary<IPathable, float>();
  }

  public Portal(Location l, MapTile m)
  {
    tile1 = m;
    tile2 = m;

    Center = l;
    Width = 1;

    costByNode = new Dictionary<IPathable, float>();
  }

  public void AddConnection(IPathable node, float cost)
  {
    if (node.Equals(this)) { return; }
    costByNode[node] = cost;
  }

  public override string ToString()
  {
    return $"{GetType()}: {Center} - x{Width}";
  }

  private void purgeConnections()
  {
    var destroy = new List<IPathable>();
    foreach (var pathable in costByNode.Keys) {
      if (pathable == null) { destroy.Add(pathable); }
    }
    foreach (var pathable in destroy) {
      costByNode.Remove(pathable);
    }
  }

  // *******************************************************************
  //    IPathable
  // *******************************************************************
  public float Cost(IPathable neighbor)
  {
    return costByNode.ContainsKey(neighbor) ?
      costByNode[neighbor] :
      float.MaxValue;
  }

  public float Heuristic(Location endGoal)
  {
    return (float)(Center - endGoal).magnitude();
  }

  public IEnumerable<IPathable> Neighbors()
  {
    purgeConnections();
    foreach (var node in costByNode.Keys) {
      yield return node;
    }
  }

  public Location AsLocation()
  {
    return Center;
  }
}
