using System.Collections.Generic;
using System.Linq;

public class TileMesh
{
  private List<Node> mesh;

  public TileMesh(List<MapTile> tiles)
  {
    mesh = new List<Node>();
    // create all nodes of the mesh from borders
    foreach (var tile in tiles) {
      foreach (var border in tile.Borders) {
        mesh.Add(Node.CreateFromBorder(border));
      }
    }
    // connect all nodes of the mesh to each other
    foreach (var tile in tiles) {
      foreach (var border in tile.Borders) {
        var node = mesh.FirstOrDefault(n => n.Center.Equals(border.Average));
        foreach (var neighbor in border.Neighbors()) {
          var connection = mesh.FirstOrDefault(n => n.Center.Equals(neighbor.Average));
          if (!node.Equals(null) && !connection.Equals(null) && !node.Equals(connection)) {
            node.AddNeighbor(connection, border.Cost(neighbor));
          }
        }
      }
    }
  }

  

  private struct Node : IPathable<Node>
  {
    // static "constructor"
    public static Node CreateFromBorder(Border b)
    {
      return new Node() {
        Center = b.Average,
        Width = b.GetLocations().Count()
      };
    }

    // Node properties
    public Location Center { get; private set; }
    public int Width { get; private set; }

    private Dictionary<Node, float> costByNode;
    public void AddNeighbor(Node neighbor, float cost)
    {
      costByNode[neighbor] = cost;
    }

    // IPathable
    public float Cost(Node neighbor)
    {
      return costByNode.ContainsKey(neighbor) ? costByNode[neighbor] : float.MaxValue;
    }

    public float Heuristic(Node endGoal)
    {
      return (float)(Center - endGoal.Center).magnitude();
    }

    public IEnumerable<Node> Neighbors()
    {
      foreach (var node in costByNode.Keys) {
        yield return node;
      }
    }
  }
}
