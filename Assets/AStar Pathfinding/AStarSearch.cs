using System;
using System.Collections.Generic;
using Priority_Queue;

/// <summary>
/// Generic form of AStarSaarch will search Locations in a 
/// Tile.
/// 
/// Requires a location inside a tile, and a MapTile class
/// that will contains all searchable points
/// </summary>
public class AStarSearch
{
  private List<Location> path;

  private SimplePriorityQueue<Location> frontier;

  private Dictionary<Location, Location> cameFrom;
  private Dictionary<Location, float> costSoFar;

  public AStarSearch()
  {
    // init the queues and dictionary
    frontier = new SimplePriorityQueue<Location>();

    path = new List<Location>();

    cameFrom = new Dictionary<Location, Location>();
    costSoFar = new Dictionary<Location, float>();
  }

  public void ComputePath(
      Location start,
      Location end,
      MapTile tile,
      Action<bool, List<Location>, float> onComplete)
  {
    // init the queues and dictionary
    frontier.Clear();
    cameFrom.Clear();
    costSoFar.Clear();

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

      foreach (var direction in Location.Directions()) {
        // compute neighbors in each direction
        var neighbor = currentNode + direction;
        // skip this node if it's not pathable
        if (!tile.IsPathable(neighbor)) { continue; }
        // add the cost of traversal from currentNode -> neighbor
        float newCost = costSoFar[currentNode] + 1; // currentNode.Cost(neighbor);

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
    path.Clear();
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

/// <summary>
/// Typed AStarSearch for any type that implements IPathable
/// </summary>
/// <typeparam name="T"></typeparam>
public class AStarSearch<T> where T : IPathable<T>
{
  private List<T> path;

  private SimplePriorityQueue<T> frontier;

  private Dictionary<T, T> cameFrom;
  private Dictionary<T, float> costSoFar;

  public AStarSearch()
  {
    // init the queues and dictionary
    frontier = new SimplePriorityQueue<T>();

    path = new List<T>();

    cameFrom = new Dictionary<T, T>();
    costSoFar = new Dictionary<T, float>();
  }

  /// <summary>
  /// This default constructor relies on the start node implementing IPathable to 
  /// have a collection of neighbors and costs pre-loaded
  /// </summary>
  /// <param name="start"></param>
  /// <param name="end"></param>
  /// <param name="onComplete"></param>
  public void ComputePath(T start, T end, Action<List<T>, float> onComplete)
  {
    // init the queues and dictionary
    frontier.Clear();
    cameFrom.Clear();
    costSoFar.Clear();

    // initialize our tracking components and queue
    frontier.Enqueue(start, 0);
    cameFrom[start] = start;
    costSoFar[start] = 0;

    T currentNode;

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
    path.Clear();
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
    onComplete(path, costSoFar[end]);
  }
}
