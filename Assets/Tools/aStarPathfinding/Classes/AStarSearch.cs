using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Priority_Queue;

public interface WeightedGraph<L>
{
	float Cost(Location a, Location b);
	IEnumerable<Location> Neighbors(Location id);
}

public class SquareGrid : WeightedGraph<Location>
{
	public static readonly Location[] DIRS = new[]
	{
		new Location(1, 0),
		new Location(0, -1),
		new Location(-1, 0),
		new Location(0, 1)
	};

	public int width, height;
	public HashSet<Location> walls = new HashSet<Location>();
	public Dictionary<Location, float> terrain = new Dictionary<Location, float>();

	public SquareGrid(int width, int height)
	{
		this.width = width;
		this.height = height;
	}

	public bool InBounds(Location id)
	{
		return 0 <= id.x && id.x < width && 0 <= id.y && id.y < height;
	}

	public bool Passable(Location id)
	{
		return !walls.Contains(id);
	}

	public float Cost(Location a, Location b)
	{
		return terrain.ContainsKey(b) ? (1 + terrain[b] - terrain[a]) : 1;
	}

	public IEnumerable<Location> Neighbors(Location id)
	{
		foreach (var dir in DIRS) {
			Location next = new Location(id.x + dir.x, id.y + dir.y);
			if (InBounds(next) && Passable(next)) {
				yield return next;
			}
		}
	}
}

// the A* pathfinding algorithm as implemented by www.redblobgames.com
public class AStarSearch
{
	public Dictionary<Location, Location> cameFrom = new Dictionary<Location, Location>();
	public Dictionary<Location, float> costSoFar = new Dictionary<Location, float>();

	// Note: a generic version of A* would abstract over Location and
	// also Heuristic
	static public float Heuristic(Location a, Location b)
	{
		float dist =
			Mathf.Sqrt(Mathf.Abs(a.x - b.x) * Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) * Mathf.Abs(a.y - b.y));

		return dist;
	}

	public AStarSearch(WeightedGraph<Location> graph, Location start, Location goal)
	{
		SimplePriorityQueue<Location> frontier = new SimplePriorityQueue<Location>();
		frontier.Enqueue(start, 0);

		cameFrom[start] = start;
		costSoFar[start] = 0;

		while (frontier.Count > 0) {
			var current = frontier.Dequeue();
			// termination condition
			if (current.Equals(goal)) {
				break;
			}


			foreach (var next in graph.Neighbors(current)) {
				float newCost = costSoFar[current] + graph.Cost(current, next);
				if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next]) {
					costSoFar[next] = newCost;
					float priority = newCost + Heuristic(next, goal);
					frontier.Enqueue(next, priority);
					cameFrom[next] = current;
				}
			}
		}
	}
}

/// GRID STAR from Terrabomb

//////using System;
//////using System.Linq;
//////using System.Collections.Generic;
//////using UnityEngine;
//////using Priority_Queue;

//////[System.Serializable]
//////public class GridStarRequest
//////{
//////  public Vector3 StartPoint;
//////  public Vector3 EndPoint;
//////  public Action<List<Vector2>> OnComplete;

//////  public GridStarRequest(
//////      Vector3 Start,
//////      Vector3 End,
//////      Action<List<Vector2>> OnCompleteCallback
//////  )
//////  {
//////    StartPoint = Start;
//////    EndPoint = End;
//////    OnComplete = OnCompleteCallback;
//////  }

//////  public override string ToString()
//////  {
//////    return $"GridStarRequest  :\n" +
//////      $"\t Start: {StartPoint}\n" +
//////      $"\t End: {EndPoint}\n" +
//////      $"\t Callback: {OnComplete.Method.ToString()}";
//////  }
//////}

//////[Serializable]
//////public class GridStar
//////{
//////  public List<Vector2> Path;

//////  private FastPriorityQueue<PowerGridNode> frontier;

//////  private Dictionary<PowerGridNode, PowerGridNode> cameFrom;
//////  private Dictionary<PowerGridNode, float> costSoFar;

//////  public GridStar()
//////  {
//////    // init the queues and dictionary
//////    frontier = new FastPriorityQueue<PowerGridNode>(1);

//////    Path = new List<Vector2>();

//////    cameFrom = new Dictionary<PowerGridNode, PowerGridNode>();
//////    costSoFar = new Dictionary<PowerGridNode, float>();
//////  }

//////  public void ComputePath(GridStarRequest request, List<PowerGridNode> grid)
//////  {
//////    // init the queues and dictionary
//////    if (frontier.MaxSize != grid.Count) { frontier.Resize(grid.Count); }
//////    frontier.Clear();
//////    cameFrom.Clear();
//////    costSoFar.Clear();

//////    // determine start nodes
//////    PowerGridNode startNode = grid.OrderBy(node =>
//////        (request.StartPoint.XY() - node.Node).sqrMagnitude)
//////        .First();
//////    PowerGridNode endNode = grid.OrderBy(node =>
//////        (request.EndPoint.XY() - node.Node).sqrMagnitude)
//////        .First();

//////    // initialize our tracking components and queue
//////    frontier.Enqueue(startNode, 0);
//////    cameFrom[startNode] = startNode;
//////    costSoFar[startNode] = 0;

//////    PowerGridNode currentNode;

//////    // start Grid* (A* pathfinding for the power grid)
//////    while (frontier.Count > 0) {
//////      currentNode = frontier.Dequeue();
//////      // termination condition
//////      if (currentNode == endNode) {
//////        break;
//////      }

//////      foreach (var prospect in currentNode.Neighbors) {
//////        // since the graph is equaly spaced, all cost increases are 1
//////        // (7-neighbors) is used to prefer nodes away from walls
//////        float newCost = costSoFar[currentNode] + (7 - currentNode.Neighbors.Count);

//////        if (!costSoFar.ContainsKey(prospect) || newCost < costSoFar[prospect]) {
//////          // track the cost so far for this node
//////          costSoFar[prospect] = newCost;
//////          float priority = newCost + Heuristic(prospect, endNode);
//////          // make sure frontier doesn't include this prospect
//////          if (!frontier.Contains(prospect)) {
//////            frontier.Enqueue(prospect, priority);
//////          }
//////          // store the "came from" chain into the dictionary
//////          cameFrom[prospect] = currentNode;
//////        }
//////      }
//////    }

//////    // create the path array and start it with the end point
//////    Path.Clear();
//////    // assemble the path backwards
//////    currentNode = endNode;
//////    while (currentNode != startNode) {
//////      Path.Add(currentNode.Node);
//////      currentNode = cameFrom[currentNode];
//////    }
//////    Path.Add(startNode.Node);
//////    // reverse the path so it reads forwards
//////    Path.Reverse();
//////    request.OnComplete(Path);
//////  }

//////  private float Heuristic(PowerGridNode A, PowerGridNode B)
//////  {
//////    return (A.Node - B.Node).magnitude;
//////  }
//////}
