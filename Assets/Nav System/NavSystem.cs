using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class NavSystem
{
  /// <summary>
  /// The dictionary allows O(1) access to the units, with a separate
  /// list of ints maintained for quick iteration
  /// </summary>
  private Dictionary<int, CcUnit> unitsById;
  private List<int> unitIds;

  /// <summary>
  /// Continuum Crowd Solutions are stored in a list for easy iteration.
  /// We do not require O(1) access to this list, rather, just need to iterate.
  /// </summary>
  private Dictionary<CcDestination, CcSolution> ccSolutionsByDestination;

  /// <summary>
  /// List of all Continuum Crowd destinations that have been requested. 
  /// Kept in list for fast iterations;
  /// </summary>
  private List<CcDestination> ccDestinations;

  /// <summary>
  /// The Eikonal solutions are hashed by CcDestation, for O(1) access. The
  /// Destinations are stored with the CcSolutions.
  /// </summary>
  private Dictionary<CcDestination, CCEikonalSolver> ccSolversByDestination;

  /// <summary>
  /// All the Continuum Crowd tiles, hashed by location
  /// </summary>
  private Dictionary<Location, CcTile> ccTilesByLocation;

  // ************ OLD **********************
  // tile nav components
  private List<MapTile> mapTiles;
  private TileMesh mesh;

  // continuum crowds
  public CCDynamicGlobalFields __ccFields;
  private List<CCEikonalSolver> __ccSolutions;
  // ************ OLD **********************

  public NavSystem(List<MapTile> tiles)
  {
    mapTiles = new List<MapTile>();
    mapTiles.AddRange(tiles);

    mesh = new TileMesh(tiles);

    __ccFields = new CCDynamicGlobalFields(tiles);
    __ccSolutions = new List<CCEikonalSolver>();
  }

  public CC_Tile GetCCTile(Location location)
  {
    return __ccFields.GetCCTile(location);
  }

  public void AddUnit(ICcUnit iccu)
  {
    unitIds.Add(iccu.UniqueId());
    unitsById.Add(iccu.UniqueId(), new CcUnit(iccu));
  }

  public void RemoveUnit(ICcUnit iccu)
  {
    // TODO - remove unit from all registered cc tiles
    // TODO - remove unit from all registered cc solutions
    unitIds.Remove(iccu.UniqueId());
    unitsById.Remove(iccu.UniqueId());
  }

  public void UnitNavigateTo(ICcUnit iccu, Vector2 destination)
  {
    unitsById[iccu.UniqueId()].Destination = destination;
  }

  public void UnitCancel(ICcUnit iccu)
  {
    unitsById[iccu.UniqueId()].Destination = iccu.Position();
  }

  private void solve(CcDestination destination)
  {
    // (1) check existing cc solutions for this destination. Does exist?
    // (1 - YES) (a) do nothing
    // (1 - NO) (b) create a new cc destination and associated eikonal solution
    // (2) compare cc tile last update id and eikonal sol last update id. They ==?
    // (2 - YES) (a) start the eikonal solution solving
    // (2 - NO) (b) look for cc tile update job. Does one exist?
    // (2b - YES) await this cc tile update job
    // (2b - NO) start a new one and await it
    // callbacks on solutions should take care of this!
  }

  public void Update(float dt)
  {
    // (1) iterate over all units -- did the unit move?
    // (1 - YES) check position and velocity, compute the affected tiles
    //            DIFF the affects tiles with the last set
    //            - UN-sub from old ones
    //            - Sub to new ones
    //            Check Unit's position in their solution, and if its changed
    //            then we've moved to a new tile:
    //            - UN-sub from old cc solution
    //            - Sub to new cc solution
    // (1 - NO) do nothing
    for (int i = 0; i < unitIds.Count; i++) {
      if (unitsById[unitIds[i]].DidUnitMove()) {
        var tiles = computeUnitImpactedTiles(unitsById[unitIds[i]]);

      }
    }


    
    // (2) itrate over all cc solutions -- is it time for a solution update?
    // (2 - YES) if Is_Solving, do nothing ... we're seeking a solution faster than
    //            the last one is solving
    //           if Has_Solution, then solve(cc destination)
    // (2 - NO) if Is_Solving, do nothing
    //          if Has_Solution, update all units via callbacks
  }

  private List<Location> computeUnitImpactedTiles(CcUnit ccu)
  {

    return null;
  }

  public void AddUnit(CC_Unit ccu)
  {
    __ccFields.AddNewCCUnit(ccu);
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
    // await the tasks
    await Task.WhenAll(startTask, endTask);

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
    // don't await this final astar search, we don't need to hold computation
    aStar.ComputePath(startPortal, endPortal, onComplete);
  }

  /// <summary>
  /// This method accepts a path of map-tiles, and provides vector
  /// flow navigation data through them
  /// </summary>
  public async Task<NavigationSolution> NavigateMap(Location start, Location end)
  {
    var solution = new NavigationSolution();

    await GetPathThroughMesh(start, end, (successful, path, cost) => {
      if (successful) {
        solution.Tiles.AddFirst(GetTileForLocation(start));
        foreach (IPathable portal in path) {
          var nextTile = GetTileForLocation(portal.AsLocation());
          if (nextTile != solution.Tiles.Last.Value) {
            solution.Tiles.AddLast(nextTile);
          }
        }
      }
    });

    // assign callback
    solution.RequestCCSolution = SolveCCforTileWithCallback;
    return solution;
  }

  public void SolveCCforTileWithCallback(
      MapTile tile,
      List<Location> goals,
      Action<Func<Vector2, Vector2>> tileSolutionCallback
  )
  {
    // (1) perform continuum crowd solution on provided tile
    var eikonalSolution = new CCEikonalSolver();
    var ccTile = __ccFields.GetCCTile(tile.Corner);

    eikonalSolution.SolveContinuumCrowdsForTile(ccTile, goals.Select(g => g - tile.Corner).ToList());

    tileSolutionCallback(vel => eikonalSolution.velocity.Interpolate(vel.x, vel.y));

    // (2) store the velocity field (solution) in a list with some identifier
    //      to clearly show what we've solved and can therefore reference later
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

  public void ForceTileUpdate()
  {
    __ccFields.UpdateTiles();
  }
}

public class NavigationSolution
{
  /// <summary>
  /// The ordered list of the MapTiles that we will traverse to reach our goal
  /// </summary>
  public LinkedList<MapTile> Tiles;

  /// <summary>
  /// Call this systam action with a MapTile to request a cc solution for the
  /// given tile.
  /// Also provide a callback that takes the solution: a system function
  /// accepting a position (vector2) in the tile and returning a velocity
  /// (vector 2) from the cc solution for the provided position.
  /// </summary>
  public Action<MapTile, List<Location>, Action<Func<Vector2, Vector2>>> RequestCCSolution;

  public NavigationSolution()
  {
    Tiles = new LinkedList<MapTile>();
  }
}

public class NavSystemException : Exception
{
  public NavSystemException() { }
  public NavSystemException(string message) : base(message) { }
}
