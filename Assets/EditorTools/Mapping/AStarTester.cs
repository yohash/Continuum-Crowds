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

  public TextMeshProUGUI CostText;

  // local vars
  private Location startLocation;
  private Location endLocation;

  private MapTile startTile;
  private MapTile endTile;

  private List<Location> path;
  private float cost;

  private NavSystem navSystem;

  // ***************************************************************************
  //  Monobehaviours
  // ***************************************************************************
  private void Awake()
  {
    navSystem = new NavSystem(TileGenerator.Instant.Tiles);

    startTile = null;
    endTile = null;
  }

  private void Update()
  {
    switch (State) {
      case A_STAR_TESTER_STATE.NONE:
        break;
      case A_STAR_TESTER_STATE.SELECT_START:
      case A_STAR_TESTER_STATE.SELECT_END:
        if (Input.GetMouseButtonDown(0)) {
          // raycast to find tap point
          RaycastHit hit;
          var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

          // ensure raycast hits terrain
          if (Physics.Raycast(ray, out hit)) {
            var loc = new Location(hit.point.x, hit.point.z);

            // store tapped location
            if (State == A_STAR_TESTER_STATE.SELECT_START) {
              startTile = navSystem.GetTileForLocation(loc);
              startLocation = startTile.GetLocation(loc);
            }
            if (State == A_STAR_TESTER_STATE.SELECT_END) {
              endTile = navSystem.GetTileForLocation(loc);
              endLocation = endTile.GetLocation(loc);
            }

            State = A_STAR_TESTER_STATE.NONE;
          }
        }
        break;
      case A_STAR_TESTER_STATE.SHOWING_PATH:
        drawPath();
        break;
    }

    setTextDisplay();
  }

  // ***************************************************************************
  //  AStar Tester
  // ***************************************************************************
  public void PathStartPoint()
  {
    if (State == A_STAR_TESTER_STATE.SELECT_START) {
      State = A_STAR_TESTER_STATE.NONE;
    } else {
      State = A_STAR_TESTER_STATE.SELECT_START;
    }
  }

  public void PathEndPoint()
  {
    if (State == A_STAR_TESTER_STATE.SELECT_END) {
      State = A_STAR_TESTER_STATE.NONE;
    } else {
      State = A_STAR_TESTER_STATE.SELECT_END;
    }
  }

  public void Clear()
  {
    State = A_STAR_TESTER_STATE.NONE;
    startLocation = Location.zero;
    endLocation = Location.zero;

    startTile = null;
    endTile = null;

    path.Clear();
    cost = 0;

    setTextDisplay();
  }

  public void FindPath()
  {
    if (startLocation != null && endLocation != null) {
      var search = new AStarSearch<Location>();
      search.ComputePath(startLocation, endLocation, storePath);
    }
  }

  private void storePath(List<Location> path, float cost)
  {
    State = A_STAR_TESTER_STATE.SHOWING_PATH;
    this.path = path;
    this.cost = cost;
    setTextDisplay();
  }

  private void setTextDisplay()
  {
    StateText.text = State.ToString();
    StartLocationText.text = startLocation?.ToString();
    EndLocationText.text = endLocation?.ToString();
    StartTileText.text = startTile?.Corner.ToString();
    EndTileText.text = endTile?.Corner.ToString();
    CostText.text = cost.ToString();
  }

  private void drawPath()
  {
    if (path.Count < 2) {
      Debug.LogWarning("Path has insufficient points. Points: " + path.Count);
      return;
    }

    for (int i = 0; i < path.Count - 1; i++) {
      float y1 = startTile.Height(path[i]);
      float y2 = startTile.Height(path[i + 1]);
      Debug.DrawLine(path[i].ToVector3(y1), path[i + 1].ToVector3(y2), Color.yellow);
    }
  }
}
