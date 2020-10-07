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
  }

  public override string ToString()
  {
    return string.Join("\n", mesh);
  }

  private struct Node : IPathable<Location>
  {
    // static "constructor"
    public static Node CreateFromBorder(Border b)
    {
      var dict = new Dictionary<Location, float>();
      foreach (var neighb in b.Neighbors()) {
        dict[neighb.Average] = b.Cost(neighb);
      }

      return new Node() {
        Center = b.Average,
        Width = b.GetLocations().Count(),
        costByNode = dict
      };
    }

    // Node properties
    public Location Center { get; private set; }
    public int Width { get; private set; }

    private Dictionary<Location, float> costByNode;

    // IPathable
    public float Cost(Location neighbor)
    {
      return costByNode.ContainsKey(neighbor) ?
        costByNode[neighbor] :
        float.MaxValue;
    }

    public float Heuristic(Location endGoal)
    {
      return (float)(Center - endGoal).magnitude();
    }

    public IEnumerable<Location> Neighbors()
    {
      foreach (var node in costByNode.Keys) {
        yield return node;
      }
    }

    public override string ToString()
    {
      return $"{Center} - x{Width}:\t" +
        $"{(costByNode == null ? "no neighbors\n" : string.Join(", ", costByNode.Keys))}";
    }
  }
}
