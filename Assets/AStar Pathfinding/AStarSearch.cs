using System;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

public class AStarSearch
{
  private static float sqrt2 = 1.414213562373095f;

  public AStarSearch() { }

  /// <summary>
  /// Generic form of AStarSearch will search Locations in a
  /// Tile.
  ///
  /// Requires a location inside a tile, and a MapTile class
  /// that will contains all searchable points
  /// </summary>
  public void ComputePath(
      Location start,
      Location end,
      MapTile tile,
      Action<bool, List<Location>, float> onComplete)
  {
    // init the queues and dictionary
    var frontier = new SimplePriorityQueue<Location>();
    var cameFrom = new Dictionary<Location, Location>();
    var costSoFar = new Dictionary<Location, float>();

    // initialize our tracking components and queue
    frontier.Enqueue(start, 0);
    cameFrom[start] = start;
    costSoFar[start] = 0;

    Location currentNode;

    // start A*
    while (frontier.Count > 0) {
      currentNode = frontier.Dequeue();
      // termination condition
      if (currentNode.Equals(end)) { break; }
      // create inline heuristic function
      float Heuristic(Location node)
      {
        return (float)(end - node).magnitude();
      }

      foreach (var direction in Location.Ordinal()) {
        // compute neighbors in each direction
        var neighbor = currentNode + direction;
        // skip this node if it's not pathable
        if (!tile.IsPathable(neighbor)) { continue; }
        // add the cost of traversal from currentNode -> neighbor
        var sqrMag = (currentNode - neighbor).sqrMagnitude();
        float newCost = costSoFar[currentNode] +
          (sqrMag == 1 ? 1 : sqrMag == 2 ? sqrt2 : (float)Math.Sqrt(sqrMag));

        if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]) {
          // track the cost so far for this node
          costSoFar[neighbor] = newCost;
          float priority = newCost + Heuristic(neighbor);
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
    var path = new List<Location>();
    try {
      // assemble the path backwards
      currentNode = end;
      while (!currentNode.Equals(start)) {
        path.Add(currentNode);
        currentNode = cameFrom[currentNode];
      }
      path.Add(start);
      // reverse the path so it reads forwards
      path.Reverse();
      // call the completion callback
      onComplete(true, path, costSoFar[end]);
    } catch {
      // we did not find a path, return start position
      path.Add(start);
      onComplete(false, path, float.MaxValue);
    }
  }

  /// <summary>
  /// AStarSearch for any type that implements IPathable
  /// </summary>
  /// <param name="start"></param>
  /// <param name="end"></param>
  /// <param name="onComplete"></param>
  public void ComputePath(
      IPathable start,
      IPathable end,
      Action<bool, List<IPathable>, float> onComplete)
  {
    // init the queues and dictionary
    var frontier = new SimplePriorityQueue<IPathable>();
    var cameFrom = new Dictionary<IPathable, IPathable>();
    var costSoFar = new Dictionary<IPathable, float>();

    // initialize our tracking components and queue
    frontier.Enqueue(start, 0);
    cameFrom[start] = start;
    costSoFar[start] = 0;

    IPathable currentNode;

    // start A*
    while (frontier.Count > 0) {
      currentNode = frontier.Dequeue();
      // termination condition
      if (currentNode.Equals(end)) { break; }

      foreach (var neighbor in currentNode.Neighbors()) {
        // add the cost of traversal from currentNode -> neighbor
        float newCost = costSoFar[currentNode] + currentNode.Cost(neighbor);

        if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]) {
          // track the cost so far for this node
          costSoFar[neighbor] = newCost;
          float priority = newCost + neighbor.Heuristic(end.AsLocation());
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
    var path = new List<IPathable>();

    try {
      // assemble the path backwards
      currentNode = end;
      while (!currentNode.Equals(start)) {
        path.Add(currentNode);
        currentNode = cameFrom[currentNode];
      }
      path.Add(start);
      // reverse the path so it reads forwards
      path.Reverse();
      // call the completion callback
      onComplete(true, path, costSoFar[end]);
    } catch {
      // we did not find a path, return start position
      path.Add(start);
      onComplete(false, path, float.MaxValue);
    }
  }
}
