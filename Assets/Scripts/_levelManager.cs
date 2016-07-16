using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class _levelManager : MonoBehaviour {


	public bool DEBUG_MODE = false;
	public bool START = false;

	public bool ___Locations___;
	public Vector2 start;
	public Vector2 goal;

	Location startLoc;
	Location goalLoc;

	public List<Location> path;
	List<Vector3> pathLocations;
	List<Vector3> pathNormals;

	public Texture2D hMap, gradMap;

	public meshLineGenerator lineGenerator;

	public float gradientThreshold = 0.5f;

	// ***************************************
	float[,] heightMap, gradientMap;
	public int xSteps, zSteps;


	// ***************************************
	void Start () {
		lineGenerator = GetComponentInChildren<meshLineGenerator>();
		lineGenerator.setLineWidth(0.5f);

	}

	void Update () {
		if (START) {
			startAStarSearch();
			START = false;
		}
	}

	void startAStarSearch() {
		pullMapData() ;
		path = new List<Location>();

		if (DEBUG_MODE) {
			Debug.DrawRay(new Vector3(start.x, 20f, start.y), Vector3.down*50f, Color.blue, 500f);
			Debug.DrawRay(new Vector3(goal.x, 20f, goal.y), Vector3.down*50f, Color.cyan, 500f);
		}

		SquareGrid grid = new SquareGrid(xSteps,zSteps);

		grid = setupGridData(grid);

		float stepSize = heightMapDataHarvester.S.stepSize;

		startLoc = new Location((int)(start.x/stepSize),(int)(start.y/stepSize));
		goalLoc = new Location((int)(goal.x/stepSize),(int)(goal.y/stepSize));

		AStarSearch astar = new AStarSearch(grid, startLoc, goalLoc);

		//saveGrid(grid, astar);

		path = constructOptimalPath(astar, goalLoc, startLoc);

		pathLocations = new List<Vector3>();
		pathNormals = new List<Vector3>();
		Vector3[] pathData;
		foreach (Location l in path) {
			pathData = heightMapDataHarvester.S.getHeightAndNormalDataForPoint(l);
			pathLocations.Add(pathData[0]);
			pathNormals.Add(pathData[1]);
		}

		lineGenerator.setLinePoints(pathLocations.ToArray(), pathNormals.ToArray(),0.5f);
		lineGenerator.generateMesh();
	}


	List<Location> constructOptimalPath(AStarSearch astar, Location theGoal, Location theStart) {
		List<Location> newPath = new List<Location>();
		Location current = theGoal;

		newPath.Add(theGoal);

		while(current != theStart) {
			current = astar.cameFrom[current];
			newPath.Add(current);
		}
		return newPath;
	}


	// *****************************************************************************************************************
	// *****************************************************************************************************************
	SquareGrid setupGridData(SquareGrid g) {
		// fill out the grid cost (heightMap)
		// and walls (gradientMap) in the grid provided

		foreach(Location l in heightMapDataHarvester.S.getUnpassableTerrain(gradientThreshold)) {
			g.walls.Add(l);
		}

		Location ltemp;
		for (int i=0; i<xSteps; i++) {
			for (int k=0; k<zSteps; k++) {
				ltemp = new Location(i,k);
				g.terrain.Add(ltemp, heightMapDataHarvester.S.getHeightData(ltemp));
			}
		}

		return g;
	}



	void pullMapData() {
		heightMap = heightMapDataHarvester.S.generateHeightMap();
		hMap = heightMapDataHarvester.S.mapToTexture(heightMap);

		gradientMap = heightMapDataHarvester.S.generateGradientMaps();
		gradMap = heightMapDataHarvester.S.mapToTexture(gradientMap);

		xSteps = heightMap.GetLength(0);
		zSteps = heightMap.GetLength(1);
	}


	void saveGrid (SquareGrid grid, AStarSearch astar) {
//		var sa = File.CreateText("Assets/pathfinding/arrow.txt");
//		var sv = File.CreateText("Assets/pathfinding/value.txt");
//
//		// Print out the cameFrom array
//		for (var y = 0; y < xSteps; y++)
//		{
//			string sa1="";
//			string sv1="";
//			for (var x = 0; x < zSteps; x++)
//			{
//				float cost;
//				Location id = new Location(x, y);
//				Location ptr = id;
//				if (!astar.cameFrom.TryGetValue(id, out ptr))
//				{
//					ptr = id;
//				}
//				if (grid.walls.Contains(id)) { sa1+="##"; sv1+="##"; }
//				else if (id.x == startLoc.x && id.y == startLoc.y) {sa1+="A ";sv1+="A ";}
//				else if (id.x == goalLoc.x && id.y == goalLoc.y) {sa1+="Z "; sv1+="Z ";}
//				else if (ptr.x == x+1) { sa1+="\u2192 "; }
//				else if (ptr.x == x-1) { sa1+="\u2190 "; }
//				else if (ptr.y == y+1) { sa1+="\u2193 "; }
//				else if (ptr.y == y-1) { sa1+="\u2191 "; }
//				else { sa1+="* ";  sv1+="* ";}
//				if (astar.costSoFar.TryGetValue(id, out cost)) {
//					sv1+=cost.ToString()+" ";
//				}
//			}
//			sa.WriteLine(sa1);
//			sv.WriteLine(sv1);
//		}
//		sa.Close();
//		sv.Close();
	}
}
