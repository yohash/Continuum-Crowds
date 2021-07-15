using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CcSolution
{
  /// <summary>
  /// The status of this solutions solving process
  /// </summary>
  public enum SolutionStatus { Is_Solving, Has_Solution }
  [SerializeField] private SolutionStatus _status;
  public SolutionStatus Status {
    get { return _status; }
  }

  /// <summary>
  /// The destination of this Continuum Crowds solution
  /// </summary>
  [SerializeField] private readonly CcDestination _destination;
  public CcDestination Destination {
    get { return _destination; }
  }

  /// <summary>
  /// The time remaining until this solution needs update again
  /// </summary>
  [SerializeField] private float _nextUpdate;
  public float NextUpdate {
    get { return _nextUpdate; }
  }

  /// <summary>
  /// This Id is associated with the most recent tile solution.
  /// It is iterated each time the eikonal solution processes.
  /// </summary>
  [SerializeField] private int _lastUpdateId;
  public float LastUpdateId {
    get { return _lastUpdateId; }
  }

  /// <summary>
  /// These units will be provided velocity solutions every update.
  /// </summary>
  [SerializeField] private List<CcUnit> _assignedUnits;
  public void AssignUnit(CcUnit unit)
  {
    _assignedUnits.Add(unit);
  }
  public void RemoveUnit(CcUnit unit)
  {
    _assignedUnits.Remove(unit);
  }

  public CcSolution(CcDestination destination)
  {
    _destination = destination;
    _lastUpdateId = 0;
  }

  public bool shouldUpdate(float dt)
  {
    _nextUpdate -= dt;
    return _nextUpdate < 0;
  }
}
