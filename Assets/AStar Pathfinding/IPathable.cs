using System.Collections.Generic;

public interface IPathable
{
  List<IPathable> Neighbors();
  Dictionary<IPathable, float> CostByNeighbor();
  float Heuristic(IPathable endGoal);
  float Cost(IPathable neighbor);
}