using System;
using System.Diagnostics;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CCTester : MonoBehaviour
{
  public static CCTester Instant;

  [Header("Assign UI")]
  public TextMeshProUGUI TilesTotalText;
  public TMP_InputField TilesInputField;

  public TextMeshProUGUI BordersTotalText;
  public TMP_InputField BordersInputField;

  public TextMeshProUGUI LoadTimeText;

  // toggles and their bools
  public void SetSolve(bool value) { solve = value; }
  [SerializeField] private bool solve;
  public Toggle SolveToggle;

  public void ShowUnit(bool value) { unit = value; }
  [SerializeField] private bool unit;
  public Toggle UnitToggle;

  public void ShowDensity(bool value) { density = value; }
  [SerializeField] private bool density;
  public Toggle DensityToggle;

  public void ShowVelocity(bool value) { velocity = value; }
  [SerializeField] private bool velocity;
  public Toggle VelocityToggle;

  public NavSystem NavSystem;
  public List<MapTile> Tiles;

  [Header("Visualizer constants")]
  public Color FlowFieldColor = Color.blue;
  public Color VelocityFieldColor = Color.green;

  [Header("Public refs")]
  // vars to track the current solution MapTile and Border
  [SerializeField] private int currentTileN;
  [SerializeField] private int currentBorderN;
  [SerializeField] private MapTile currentTile;
  [SerializeField] private Border currentBorder;

  // visualization tool handlers
  private TileMap tilemap;
  public GameObject TestUnit;

  // vars to track CC solution solving process
  private bool solutionProcessing = false;

  // vars to track CC solution itself
  private bool tileSolutionAvailable;
  private Func<Vector2, Vector2> tileSolution;

  // track speed of solution
  private Stopwatch stopwatch;
  // store path requested by the user
  private List<Vector3> testPath;

  // ***************************************************************************
  //    Monobehaviours
  // ***************************************************************************
  private void Awake()
  {
    Instant = this;
    stopwatch = new Stopwatch();

    NavSystem = new NavSystem(TileGenerator.Instant.Tiles);
    Tiles = TileGenerator.Instant.Tiles;

    TilesTotalText.text = Tiles.Count.ToString();

    tilemap = new TileMap("CCTester Tilemap");

    TileInputChanged("0");
  }

  private void Update()
  {
    if (currentBorder != null) {
      TileGenerator.DrawBorder(currentBorder, Color.green);
    }

    SolveToggle.interactable = currentTile != null && currentBorder != null;
    if (solve) {
      if (!solutionProcessing) {
        Solve();
      }
    }

    if (tileSolutionAvailable) {
      displaySolution();

      // see if we should compute a new test path
      if (Input.GetMouseButton(0)) {
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

    if (unit) {
      // create a test unit
      if (TestUnit == null) {
        TestUnit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        TestUnit.AddComponent<CCTestUnit>();
        NavSystem.TrackUnit(new CC_Unit(
            () => TestUnit.GetComponent<CCTestUnit>().velocity,
            () => TestUnit.transform.eulerAngles.y,
            () => TestUnit.transform.position.XYZtoXZ(),
            () => TestUnit.GetComponent<CCTestUnit>().dimensions,
            () => TestUnit.GetComponent<CCTestUnit>().falloff
        ));
      }
      TestUnit.SoftSetActive(true);
    } else {
      // disable the test unit
      TestUnit.SoftSetActive(false);
    }

    if (density) {
      NavSystem.ForceTileUpdate();
      var tile = NavSystem.GetCCTile(currentTile.Corner);
      tilemap.BuildTexture(TextureGenerator.TextureFromMatrix(tile.rho, Color.clear, Color.blue));
      tilemap.BuildMesh(tile.h);
    }

    if (velocity) {
      NavSystem.ForceTileUpdate();
      displayVelocityField();
    }
  }

  // ***************************************************************************
  //    Public
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
    NavSystem.ForceTileUpdate();
    solveTileCcAtBorder(currentTile, currentBorder);
  }

  // ***************************************************************************
  //    Private
  // ***************************************************************************
  private void solveTileCcAtBorder(MapTile m, Border b)
  {
    solutionProcessing = true;
    stopwatch.Reset();
    stopwatch.Start();
    NavSystem.SolveCCforTileWithCallback(
        m,
        b.GetLocations().ToList(),
        (callback) => {
          tileSolution = callback;
          solutionProcessing = false;
        }
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
    for (int x = 0; x < currentTile.TileSize; x++) {
      for (int y = 0; y < currentTile.TileSize; y++) {
        var loc = new Vector2(x, y);
        var vel = tileSolution(loc).normalized;
        float worldX = x + .5f + currentTile.Corner.x;
        float worldY = y + .5f + currentTile.Corner.y;
        var height = currentTile.Height(worldX, worldY);
        var start = new Vector3(worldX - vel.x / 2, height + .2f, worldY - vel.y / 2);
        var end = new Vector3(worldX + vel.x / 2, height + .2f, worldY + vel.y / 2);
        UnityEngine.Debug.DrawLine(start, end, FlowFieldColor);
      }
    }
  }

  private void displayVelocityField()
  {
    var tile = NavSystem.GetCCTile(currentTile.Corner);
    var velMtx = tile.vAve.Normalize();

    for (int x = 0; x < velMtx.GetLength(0); x++) {
      for (int y = 0; y < velMtx.GetLength(1); y++) {
        var vel = velMtx[x,y];
        float worldX = x + .5f + currentTile.Corner.x;
        float worldY = y + .5f + currentTile.Corner.y;
        var height = currentTile.Height(worldX, worldY);
        var start = new Vector3(worldX - vel.x / 2, height + .2f, worldY - vel.y / 2);
        var end = new Vector3(worldX + vel.x / 2, height + .2f, worldY + vel.y / 2);
        UnityEngine.Debug.DrawLine(start, end, VelocityFieldColor);
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
      testPath.Add(next
        + currentTile.Corner.ToVector3()
        + Vector3.up * (currentTile.Height(next.x, next.z) + .2f));
      // get dir at location NEXT
      dir = tileSolution(next.XYZtoXZ());
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
      UnityEngine.Debug.DrawLine(testPath[i], testPath[i - 1], new Color(1, 0.919884f, 0.5235849f));
    }
  }
}
