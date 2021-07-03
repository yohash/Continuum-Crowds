using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CcUnit
{
  /// <summary>
  /// The status of this unit's Nav solution
  /// </summary>
  public enum SolutionStatus { Waiting, HasSolution, None }
  [SerializeField] private SolutionStatus _status;
  public SolutionStatus Status {
    get { return _status; }
  }

  /// <summary>
  /// This unit's ultimate destination
  /// </summary>
  [SerializeField] private Vector2 _destination;
  public Vector2 Destination {
    get { return _destination; }
    set { _destination = value; }
  }



}
