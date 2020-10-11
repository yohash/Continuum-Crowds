using System.Collections.Generic;
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
          mesh[border.Center].AddConnection(mesh[neighbor.Center], border.Cost(neighbor));
        }
      }
    }
  }

  public override string ToString()
  {
    return "Mesh: " + string.Join("\n", mesh);
  }
}

public class Portal : IPathable<Portal>
{
  // Portal properties
  public Location Center { get; private set; }
  public int Width { get; private set; }

  // linking data
  public MapTile tile1 { get; private set; }
  public MapTile tile2 { get; private set; }

  // IPathable cost dictionary
  private Dictionary<Portal, float> costByNode = new Dictionary<Portal, float>();

  public Portal(Border b)
  {
    var portalLocations = new List<Location>();

    // get the maptile of this border
    tile1 = b.Tile;

    // assign tiles
    // get any neighbor whose cost is 1
    var neighbor = b.Neighbors().Where(neighb => b.Cost(neighb) == 1)
        .FirstOrDefault();
    if (neighbor == null) {
      UnityEngine.Debug.LogError("Portal has no neighboring border");
    } else {
      tile2 = neighbor.Tile;
    }

    Center = b.Center;
    Width = b.GetLocations().Count();
  }

  public void AddConnection(Portal node, float cost)
  {
    if (node == this) { return; }
    costByNode.Add(node, cost);
  }

  // IPathable
  public float Cost(Portal neighbor)
  {
    return costByNode.ContainsKey(neighbor) ?
      costByNode[neighbor] :
      float.MaxValue;
  }

  public float Heuristic(Portal endGoal)
  {
    return (float)(Center - endGoal.Center).magnitude();
  }

  public IEnumerable<Portal> Neighbors()
  {
    foreach (var node in costByNode.Keys) {
      yield return node;
    }
  }

  public override string ToString()
  {
    return $"{GetType()}: {Center} - x{Width}";
  }
}
