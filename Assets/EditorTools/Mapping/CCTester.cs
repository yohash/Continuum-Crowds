using System;
using System.Diagnostics;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class CCTester : MonoBehaviour
{
  [Header("Assign UI")]
  public TextMeshProUGUI TilesTotalText;
  public TMP_InputField TilesInputField;

  public TextMeshProUGUI BordersTotalText;
  public TMP_InputField BordersInputField;

  public TextMeshProUGUI LoadTimeText;

  [Header("Public refs")]
  public NavSystem NavSystem;
  public List<MapTile> Tiles;

  [SerializeField] private int currentTileN;
  [SerializeField] private int currentBorderN;

  [SerializeField] private MapTile currentTile;
  [SerializeField] private Border currentBorder;

  private bool tileSolutionAvailable;
  private CCEikonalSolver solution;

  private Func<Vector2, Vector2> tileSolution;

  private Stopwatch stopwatch;

  private List<Vector3> testPath;

  // ***************************************************************************
  //  Monobehaviours
  // ***************************************************************************
  private void Awake()
  {
    stopwatch = new Stopwatch();

    NavSystem = new NavSystem(TileGenerator.Instant.Tiles);
    Tiles = TileGenerator.Instant.Tiles;

    TilesTotalText.text = Tiles.Count.ToString();
    TileInputChanged("0");
  }

  private void Update()
  {
    if (currentBorder != null) {
      TileGenerator.DrawBorder(currentBorder, Color.green);
    }

    if (tileSolutionAvailable) {
      displaySolution();

      // see if we should compute a new test path
      if (Input.GetMouseButtonDown(0)) {
        // raycast to find tap point
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // ensure raycast hits terrain
        if (Physics.Raycast(ray, out hit)) {
          computeNewTestPathFromSolution(hit.point);
        }
      }
      // see if we should draw the test path
      if (testPath.Count > 0) {
        drawTestPath();
      }
    }
  }

  // ***************************************************************************
  //  INTERFACE
  // ***************************************************************************
  public void TileInputChanged(string s)
  {
    if (int.TryParse(s, out int val)) {
      currentTileN = (int)Mathf.Repeat(val, Tiles.Count);
      currentTile = Tiles[currentTileN];

      BordersTotalText.text = currentTile.Borders.Count().ToString();
      BordersInputField.text = "0";
      BorderInputChanged("0");
    }
  }

  public void BorderInputChanged(string s)
  {
    if (int.TryParse(s, out int val)) {
      currentBorderN = (int)Mathf.Repeat(val, currentTile.Borders.Count());
      currentBorder = currentTile.Borders[currentBorderN];
    }
    resetSolution();
  }

  public void Solve()
  {
    if (currentTile == null || currentBorder == null) {
      UnityEngine.Debug.LogWarning("Tile or Border are null");
      return;
    }
    solveTileCcAtBorder(currentTile, currentBorder);
  }

  // ***************************************************************************
  //  Private methods
  // ***************************************************************************
  private void solveTileCcAtBorder(MapTile m, Border b)
  {
    stopwatch.Start();
    NavSystem.SolveCCforTileWithCallback(
        m,
        b.GetLocations().ToList(),
        (callback) => { tileSolution = callback; }
    );
    stopwatch.Stop();
    LoadTimeText.text = Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString();
    tileSolutionAvailable = true;
  }

  private void resetSolution()
  {
    tileSolutionAvailable = false;
    testPath = new List<Vector3>();

    tileSolution = (v) => {
      UnityEngine.Debug.LogWarning("NOT ASSIGNED");
      return Vector2.zero;
    };
  }

  private void displaySolution()
  {
    for (int i = 0; i < currentTile.TileSize; i++) {
      for (int k = 0; k < currentTile.TileSize; k++) {
        float tileX = i;
        float tileY = k;
        var loc = new Vector2(tileX, tileY);
        var vel = tileSolution(loc).normalized;
        float worldX = tileX + .5f + currentTile.Corner.x;
        float worldY = tileY + .5f + currentTile.Corner.y;
        var height = currentTile.Height(worldX, worldY);
        var start = new Vector3(worldX - vel.x / 2, height, worldY - vel.y / 2);
        var end = new Vector3(worldX + vel.x / 2, height, worldY + vel.y / 2);

        UnityEngine.Debug.DrawLine(start, end, Color.blue);
      }
    }
  }

  private void computeNewTestPathFromSolution(Vector3 start)
  {
    testPath = new List<Vector3>();
    start -= currentTile.Corner.ToVector3();

    var dir = new Vector2(start.x, start.z);
    var next = dir.ToXYZ(0);

    while (dir != Vector2.zero) {
      // store next
      testPath.Add(next + currentTile.Corner.ToVector3());
      // get dir at location NEXT
      dir = tileSolution(next.XYZtoXY());
      // normalize dir if not zero
      if (dir != Vector2.zero) {
        dir.Normalize();
        // advance next by dir * time.deltaTime
        next += Time.deltaTime * dir.ToXYZ(0);
      }
    }
  }

  private void drawTestPath()
  {
    for (int i = 1; i < testPath.Count; i++) {
      UnityEngine.Debug.DrawLine(testPath[i], testPath[i - 1], Color.green);
    }
  }
}
