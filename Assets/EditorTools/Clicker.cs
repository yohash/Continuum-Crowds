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

    Func<Vector2, Vector2> tileSolution = (v) => {
      Debug.LogWarning("NOT ASSIGNED");
      return Vector2.zero;
    };

    // get locations between tile 1 and 2
    var locs = navSolution.Tiles.First.Value.Borders.Where(
      b => b.Direction == DIRECTION.NORTH).First().GetLocations().ToList();

    var corner = navSolution.Tiles.First.Value.Corner;
    Debug.Log("Tile CC solution initiatied, goal locs: " + string.Join(", ", locs));
    foreach (Location loc in locs) {
      Drawings.DrawSquare(loc.ToVector3().WithY(navSolution.Tiles.First.Value.Height(loc)), Color.black, 25 );
    }
    Debug.Log("ccdynamicfields C: \n" + astar.NavSystem.ccFields.GetCCTile(corner).C.ToString<Vector4>());

    navSolution.RequestCCSolution(
        navSolution.Tiles.First(),
        locs,
        (callback) => {
          tileSolution = callback;
        }
    );

    Debug.Log("Tile CC solution received, some sample velocities:");
    void test(Vector2 v)
    {
      Debug.Log($"\t{v}: {tileSolution(v)}");
    }
    test(Vector2.one * 5);
    test(Vector2.one * 15);
    test(new Vector2(10, 20));
    test(new Vector2(14, 30));

    var tile = navSolution.Tiles.First();
    for (int i = 0; i < tile.TileSize; i++) {
      for (int k = 0; k < tile.TileSize; k++) {
        float x = i + 0.5f;
        float y = k + 0.5f;
        var loc = new Vector2(x, y);
        var vel = tileSolution(loc).normalized;
        var height = tile.Height((int)x, (int)y);
        var ss = new Vector3(x - vel.x / 2, height, y - vel.y / 2);
        var ee = new Vector3(x + vel.x / 2, height, y + vel.y / 2);

        Debug.DrawLine(ss, ee, Color.blue, 24);
      }
    }
  }
}
