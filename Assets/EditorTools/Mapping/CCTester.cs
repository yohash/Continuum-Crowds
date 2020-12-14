using System.Collections;
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

  public NavSystem NavSystem;
  public List<MapTile> Tiles;

  [SerializeField] private int currentTileN;
  [SerializeField] private int currentBorderN;

  [SerializeField] private MapTile currentTile;
  [SerializeField] private Border currentBorder;

  // ***************************************************************************
  //  Monobehaviours
  // ***************************************************************************
  private void Awake()
  {
    NavSystem = new NavSystem(TileGenerator.Instant.Tiles);
    Tiles = TileGenerator.Instant.Tiles;

    TilesTotalText.text = Tiles.Count.ToString();
    TileInputChanged("0");
  }

  private void Update()
  {
    if (currentBorder != null) {
      TileGenerator.DrawBorder(currentBorder, Color.blue);
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
  }
}
