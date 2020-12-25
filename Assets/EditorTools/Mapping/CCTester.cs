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

    tileSolution = (v) => {
      UnityEngine.Debug.LogWarning("NOT ASSIGNED");
      return Vector2.zero;
    };
  }

  private void displaySolution()
  {
    for (int i = 0; i < currentTile.TileSize; i++) {
      for (int k = 0; k < currentTile.TileSize; k++) {
        float tileX = i + 0.5f;
        float tileY = k + 0.5f;
        var loc = new Vector2(tileX, tileY);
        var vel = tileSolution(loc).normalized;
        float worldX = tileX + currentTile.Corner.x;
        float worldY = tileY + currentTile.Corner.y;
        var height = currentTile.Height(worldX, worldY);
        var start = new Vector3(worldX - vel.x / 2, height, worldY - vel.y / 2);
        var end = new Vector3(worldX + vel.x / 2, height, worldY + vel.y / 2);

        UnityEngine.Debug.DrawLine(start, end, Color.blue);
      }
    }
  }
}
