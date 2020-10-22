using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using UnityEngine.EventSystems;
using System.ComponentModel;

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

  public static AStarTester Instance;

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

  private Stopwatch timer;

  // ***************************************************************************
  //  Monobehaviours
  // ***************************************************************************
  private void Awake()
  {
    Instance = this;
    navSystem = new NavSystem(TileGenerator.Instant.Tiles);

    startTile = null;
    endTile = null;

    path = new List<Location>();

    timer = new Stopwatch();
  }

  private void Update()
  {
    switch (State) {
      case A_STAR_TESTER_STATE.NONE:
        break;
      case A_STAR_TESTER_STATE.SELECT_START:
      case A_STAR_TESTER_STATE.SELECT_END:
        break;
      case A_STAR_TESTER_STATE.SHOWING_PATH:
        drawPath();
        break;
    }

    if (!EventSystem.current.IsPointerOverGameObject()) {
      if (Input.GetMouseButtonDown(0)) {
        // raycast to find tap point
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // ensure raycast hits terrain
        if (Physics.Raycast(ray, out hit)) {
          var loc = new Location(hit.point.x, hit.point.z);
          // store start location
          startTile = navSystem.GetTileForLocation(loc);
          startLocation = loc;
        }
      } else if (Input.GetMouseButtonDown(1)) {
        // raycast to find tap point
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // ensure raycast hits terrain
        if (Physics.Raycast(ray, out hit)) {
          var loc = new Location(hit.point.x, hit.point.z);
          // store end location
          endTile = navSystem.GetTileForLocation(loc);
          endLocation = loc;
        }
      }
    }

    setTextDisplay();
  }

  // ***************************************************************************
  //  AStar Tester
  // ***************************************************************************
  public void DeclareStartPoint(Location loc)
  {
    startTile = navSystem.GetTileForLocation(loc);
    startLocation = loc;
  }
  public void DeclareEndPoint(Location loc)
  {
    endTile = navSystem.GetTileForLocation(loc);
    endLocation = loc;
  }

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
    startLocation = Location.Zero;
    endLocation = Location.Zero;

    startTile = null;
    endTile = null;

    path.Clear();
    cost = 0;

    setTextDisplay();
  }

  public async void FindPath()
  {
    if (!Location.Equals(startLocation, endLocation)) {
      timer.Reset();
      timer.Start();
      //simplified pathfinding, point to point
      if (startTile == endTile) {
        await Task.Run(() => {
          var search = new AStarSearch();
          search.ComputePath(startLocation, endLocation, startTile, storeLocationPath);
        });
      }
      // complex pathfinding, navigate the tile mesh through nav system
      else {
        await navSystem.GetPathThroughMesh(startLocation, endLocation, storePathablePath);
      }
    }
  }

  private void storePathablePath(bool successful, List<IPathable> path, float cost)
  {
    timer.Stop();
    string s = successful ? "success" : "fail";
    UnityEngine.Debug.Log($"Computed path ({s} - {cost}), elapsed: {timer.Elapsed}");
    State = A_STAR_TESTER_STATE.SHOWING_PATH;
    this.path = path.Select(pathable => pathable.AsLocation()).ToList();
    this.cost = cost;
  }

  private void storeLocationPath(bool successful, List<Location> path, float cost)
  {
    timer.Stop();
    string s = successful ? "success" : "fail";
    UnityEngine.Debug.Log($"Computed path ({s} - {cost}), elapsed: {timer.Elapsed}");
    State = A_STAR_TESTER_STATE.SHOWING_PATH;
    this.path = path;
    this.cost = cost;
  }

  private void setTextDisplay()
  {
    StateText.text = State.ToString();
    StartLocationText.text = startLocation.ToString();
    EndLocationText.text = endLocation.ToString();
    StartTileText.text = startTile?.Corner.ToString();
    EndTileText.text = endTile?.Corner.ToString();
    CostText.text = cost.ToString();
  }

  private void drawPath()
  {
    if (path.Count < 2) {
      UnityEngine.Debug.LogWarning("Path has insufficient points. Points: " + path.Count);
      return;
    }

    for (int i = 0; i < path.Count - 1; i++) {
      float y1 = navSystem.GetTileForLocation(path[i]).Height(path[i]);
      float y2 = navSystem.GetTileForLocation(path[i + 1]).Height(path[i + 1]);
      UnityEngine.Debug.DrawLine(path[i].ToVector3(y1), path[i + 1].ToVector3(y2), Color.yellow);
    }
  }
}
