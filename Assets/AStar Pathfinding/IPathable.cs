using System.Collections.Generic;

public interface IPathable<T>
{
  IEnumerable<IPathable<T>> Neighbors();
  Dictionary<IPathable<T>, float> CostByNeighbor();
  float Heuristic(IPathable<T> endGoal);
  float Cost(IPathable<T> neighbor);
}