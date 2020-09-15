using System.Collections.Generic;

public interface IPathable<T>
{
  IEnumerable<T> Neighbors();
  float Heuristic(T endGoal);
  float Cost(T neighbor);
}