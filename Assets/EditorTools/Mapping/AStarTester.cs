using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AStarTester : MonoBehaviour
{
  public enum A_STAR_TESTER_STATE
  {
    NONE,
    SELECT_START,
    SELECT_END,
    SHOWING_PATH
  }
  private A_STAR_TESTER_STATE State = A_STAR_TESTER_STATE.NONE;

  [Header("Assign UI")]
  public TextMeshProUGUI StateText;

  public TextMeshProUGUI StartLocationText;
  public TextMeshProUGUI StartTileText;

  public TextMeshProUGUI EndLocationText;
  public TextMeshProUGUI EndTileText;

  private Location startLocation;
  private Location endLocation;

  private List<Location> path;
  private float cost;

  // ***************************************************************************
  //  Monobehaviours
  // ***************************************************************************
  private void Update()
  {
    StateText.text = State.ToString();

    if (State == A_STAR_TESTER_STATE.SELECT_END || State == A_STAR_TESTER_STATE.SELECT_START) {
      if (Input.GetMouseButtonDown(0)) {
        // raycast to find tap point
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // ensure raycast hits terrain
        if (Physics.Raycast(ray, out hit)) {
          Location loc = new Location(hit.point.x, hit.point.z);

          // store tapped location
          if (State == A_STAR_TESTER_STATE.SELECT_START) {
            startLocation = loc;
            StartLocationText.text = loc.ToString();
          }
          if (State == A_STAR_TESTER_STATE.SELECT_END) {
            endLocation = loc;
            EndLocationText.text = loc.ToString();
          }

          State = A_STAR_TESTER_STATE.NONE;
        }
      }
    }
  }

  // ***************************************************************************
  //  AStar Tester
  // ***************************************************************************
  public void StartPathfinding()
  {
    if (State == A_STAR_TESTER_STATE.SELECT_START) {
      State = A_STAR_TESTER_STATE.NONE;
    } else {
      State = A_STAR_TESTER_STATE.SELECT_START;
    }
  }

  public void EndPathfinding()
  {
    if (State == A_STAR_TESTER_STATE.SELECT_END) {
      State = A_STAR_TESTER_STATE.NONE;
    } else {
      State = A_STAR_TESTER_STATE.SELECT_END;
    }
  }
}
