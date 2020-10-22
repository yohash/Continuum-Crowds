
using System.Threading.Tasks;
using UnityEngine;

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
    tilegen.GenerateTiles();
    handle.BackButton.onClick.Invoke();

    handle.AStarButton.onClick.Invoke();
    var astar = AStarTester.Instance;
    astar.DeclareStartPoint(new Location(13, 13));
    astar.DeclareEndPoint(new Location(35, 14));

    //astar.FindPath();
  }
}
