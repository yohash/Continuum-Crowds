using System;
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
  public async Task GetPathThroughMesh(Location start, Location end)
  {
    var startTile = GetTileForLocation(start);
    var endTile = GetTileForLocation(end);

    var startNodes = mesh.FindMeshPortalsConnectedToLocation(start, startTile);
    var endNodes = mesh.FindMeshPortalsConnectedToLocation(end, endTile);

    // store and await the node tasks
    var seedTasks = new List<Task>() { startNodes, endNodes };
    await Task.WhenAll(seedTasks);

    // perform navigation through the mesh

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