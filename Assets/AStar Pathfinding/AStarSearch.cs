﻿using System;
using System.Collections.Generic;
using Priority_Queue;

[Serializable]
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
