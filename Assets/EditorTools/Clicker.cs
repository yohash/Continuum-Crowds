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

    handle.AStarButton.onClick.Invoke();
    var astar = AStarTester.Instance;

    var start = new Location(13, 13);
    var end = new Location(35, 44);

    astar.DeclareStartPoint(start);
    astar.DeclareEndPoint(end);

    astar.FindPath();

    var navSolution = await astar.NavSystem.NavigateMap(start, end);

    Debug.Log("Nav Solution complete, num tiles = " + navSolution.Tiles.Count);
    Debug.Log(string.Join("\t", navSolution.Tiles));

    Func<Vector2, Vector2> tileSolution;

    // get locations between tile 1 and 2
    var locs = navSolution.Tiles.First.Value.Borders.Where(
      b => b.Direction == DIRECTION.NORTH).First().GetLocations().ToList();

    navSolution.RequestCCSolution(
        navSolution.Tiles.First(),
        locs,
        (callback) => {
          tileSolution = callback;
        }
    );


  }
}
