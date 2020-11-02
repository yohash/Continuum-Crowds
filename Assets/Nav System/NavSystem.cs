using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class NavSystem
{
  private List<MapTile> mapTiles;
  private TileMesh mesh;

  private int tileSize;

  public NavSystem(List<MapTile> tiles)
  {
    mapTiles = new List<MapTile>();
    mapTiles.AddRange(tiles);
    if (mapTiles.Count == 0) {
      throw new NavSystemException("NavSystem initiated with no tiles");
    }

    mesh = new TileMesh(tiles);
    tileSize = mapTiles[0].TileSize;
  }

  /// <summary>
  /// Provided start and end locations, this will plot an A* path
  /// </summary>
  /// <param name="start"></param>
  /// <param name="end"></param>
  public async Task GetPathThroughMesh(
      Location start,
      Location end,
      Action<bool, List<IPathable>, float> onComplete)
  {
    var startTile = GetTileForLocation(start);
    var endTile = GetTileForLocation(end);

    // create dictionaries for task returns
    var startPortals = new Dictionary<Portal, float>();
    var endPortals = new Dictionary<Portal, float>();

    // populate the portal dictionaries
    var startTask = mesh.FindConnectedPortals(start, startTile, startPortals);
    var endTask = mesh.FindConnectedPortals(end, endTile, endPortals);

    // store and await the node tasks
    var seedTasks = new List<Task>() { startTask, endTask };
    // await the tasks
    await Task.WhenAll(seedTasks);

    // create "dummy" portals to represent start and end locations
    var startPortal = new Portal(start, startTile);
    var endPortal = new Portal(end, endTile);

    // add connections for IPathable
    foreach (var portal in startPortals) {
      startPortal.AddConnection(portal.Key, portal.Value);
    }
    foreach (var portal in endPortals) {
      portal.Key.AddConnection(endPortal, portal.Value);
    }

    // start pathfinding
    var aStar = new AStarSearch();
    // don't awayt this final astar search, we don't need to hold computation
    aStar.ComputePath(startPortal, endPortal, onComplete);
  }

  /// <summary>
  /// This method accepts a path of map-tiles, and provides vector
  /// flow navigation data through them
  /// </summary>
  public void NavigateMesh()
  {

  }

  public void SolveCCforTileWithCallback(MapTile tile, Func<Vector2, Vector2> callback)
  {
    // (1) perform continuum crowd solution on provided tile
    // (2) store the velocity field (solution) in a list with some identifier
    //      to clearly show what we've solved and can therefore refeence later
    // (3) send back a function that will take in a position (vector2) and 
    //      return the interpolated velocity on the cc solution (vector2)
  }

  public MapTile GetTileForLocation(Location location)
  {
    foreach (var tile in mapTiles) {
      if (tile.ContainsPoint(location)) { return tile; }
    }
    return mapTiles[0];
  }
}

public class NavigationSolution
{
  /// <summary>
  /// The ordered list of the Maptiles that we will traverse to reach our goal
  /// </summary>
  public LinkedList<MapTile> Tiles;

  /// <summary>
  /// Call this systam action with a MapTile to request a cc solution for the 
  /// given tile. 
  /// Also provide a callback that takes the solution: a system function 
  /// accepting a position (vector2) in the tile and returning a velocity
  /// (vector 2) from the cc solution for the provided position.
  /// </summary>
  public Action<MapTile, Action<Func<Vector2, Vector2>>> RequestCCSolution;

  /// <summary>
  /// 
  /// </summary>
  public void CCSolutionHandle(Func<Vector2, Vector2> tileSolution) {

  }
}

public class NavSystemException : Exception
{
  public NavSystemException() { }
  public NavSystemException(string message) : base(message) { }
}
