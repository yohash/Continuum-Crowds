using UnityEngine;

public class CcUnit
{
  /// <summary>
  /// The status of this unit's Nav solution
  /// </summary>
  public enum SolutionStatus { Waiting, Has_Path, None }
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
    set {
      _destination = value;
      _status = SolutionStatus.Waiting;
    }
  }

  /// <summary>
  /// Continuum Crowds unit interface access
  /// </summary>
  private ICcUnit _ccUnitInterface;
  public ICcUnit CcUnitInterface {
    get { return _ccUnitInterface; }
  }

  private Vector2 _position;

  public CcUnit(ICcUnit ccUnitInterface)
  {
    _ccUnitInterface = ccUnitInterface;
    _position = ccUnitInterface.Position();
  }

  public bool DidUnitMove()
  {
    var oldPosition = _position;
    _position = _ccUnitInterface.Position();
    return !(oldPosition.x == _position.x && oldPosition.y == _position.y);
  }
}
