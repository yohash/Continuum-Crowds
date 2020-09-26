using System;
using System.Collections.Generic;

public class NavSystem
{
  private List<MapTile> mapTiles;

  private int tileSize;

  public NavSystem(List<MapTile> tiles)
  {
    mapTiles = new List<MapTile>();
    mapTiles.AddRange(tiles);

    if (mapTiles.Count == 0) {
      throw new NavSystemException("NavSystem initiated with no tiles");
    }

    tileSize = mapTiles[0].TileSize;
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