﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
      Action<List<IPathable>, float> onComplete)
  {
    var startTile = GetTileForLocation(start);
    var endTile = GetTileForLocation(end);

    var startTask = mesh.FindConnectedPortals(start, startTile);
    var endTask = mesh.FindConnectedPortals(end, endTile);

    // create dictionaries for task returns
    Dictionary<Portal, float> startPortals = new Dictionary<Portal, float>();
    Dictionary<Portal, float> endPortals = new Dictionary<Portal, float>();

    // store and await the node tasks
    var seedTasks = new List<Task<Dictionary<Portal, float>>>() { startTask, endTask };
    while (seedTasks.Count > 0) {
      var t = await Task.WhenAny(seedTasks);
      if (t == startTask) { startPortals = t.Result; }
      if (t == endTask) { endPortals = t.Result; }
      seedTasks.Remove(t);
    }

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

  public MapTile GetTileForLocation(Location location)
  {
    foreach (var tile in mapTiles) {
      if (tile.ContainsPoint(location)) { return tile; }
    }
    return mapTiles[0];
  }
}

public class NavSystemException : Exception
{
  public NavSystemException() { }
  public NavSystemException(string message) : base(message) { }
}