using System;
using System.Collections.Generic;
using Priority_Queue;

[Serializable]
public class AStarSearch<T>
{
  public List<IPathable<T>> Path;

  private SimplePriorityQueue<IPathable<T>> frontier;

  private Dictionary<IPathable<T>, IPathable<T>> cameFrom;
  private Dictionary<IPathable<T>, float> costSoFar;

  public AStarSearch()
  {
    // init the queues and dictionary
    frontier = new SimplePriorityQueue<IPathable<T>>();

    Path = new List<IPathable<T>>();

    cameFrom = new Dictionary<IPathable<T>, IPathable<T>>();
    costSoFar = new Dictionary<IPathable<T>, float>();
  }

  public void ComputePath(IPathable<T> start, IPathable<T> end, Action<List<IPathable<T>>> onComplete)
  {
    // init the queues and dictionary
    frontier.Clear();
    cameFrom.Clear();
    costSoFar.Clear();

    // initialize our tracking components and queue
    frontier.Enqueue(start, 0);
    cameFrom[start] = start;
    costSoFar[start] = 0;

    IPathable<T> currentNode;

    // start Grid* (A* pathfinding for the power grid)
    while (frontier.Count > 0) {
      currentNode = frontier.Dequeue();
      // termination condition
      if (currentNode == end) { break; }

      foreach (var neighbor in currentNode.Neighbors()) {
        // add the cost of traversal from currentNode -> neighbor
        float newCost = costSoFar[currentNode] + currentNode.Cost(neighbor);

        if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]) {
          // track the cost so far for this node
          costSoFar[neighbor] = newCost;
          float priority = newCost + neighbor.Heuristic(end);
          // make sure frontier doesn't include this prospect
          if (!frontier.Contains(neighbor)) {
            frontier.Enqueue(neighbor, priority);
          }
          // store the "came from" chain into the dictionary
          cameFrom[neighbor] = currentNode;
        }
      }
    }

    // create the path array and start it with the end point
    Path.Clear();
    // assemble the path backwards
    currentNode = end;
    while (currentNode != start) {
      Path.Add(currentNode);
      currentNode = cameFrom[currentNode];
    }
    Path.Add(start);
    // reverse the path so it reads forwards
    Path.Reverse();
    // call the completion callback
    onComplete(Path);
  }
}
