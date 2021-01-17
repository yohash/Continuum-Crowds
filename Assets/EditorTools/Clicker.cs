using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Linq;

public class Clicker : MonoBehaviour
{
  void Start()
  {
    DoStuff();
  }

  private async Task DoStuff()
  {
    var handle = ToolsHandle.Instant;
    handle.TileGenButton.onClick.Invoke();

    var tilegen = TileGenerator.Instant;
    tilegen.LoadCsvFiles();
    await tilegen.GenerateTilesAsync();
    handle.BackButton.onClick.Invoke();

    handle.ContinuumCrowdsButton.onClick.Invoke();
    var cctester = CCTester.Instant;
    cctester.SolveButton.onClick.Invoke();
  }
}
