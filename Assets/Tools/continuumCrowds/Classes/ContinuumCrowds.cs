using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CC_Unit_Goal_Group
{
	public Rect goal;
	public List<CC_Unit> units;

	public CC_Unit_Goal_Group (Rect r, List<CC_Unit> u)
	{
		this.goal = r;
		this.units = u;
	}
}

public struct CC_Map_Package
{
	public float[,] h;
	public float[,] dhdx;
	public float[,] dhdy;
	public float[,] g;

	public CC_Map_Package (float[,] _h, float[,] _dhdx, float[,] _dhdy, float[,] _g)
	{
		this.h = _h;
		this.dhdx = _dhdx;
		this.dhdy = _dhdy;
		this.g = _g;
	}
}

public class ContinuumCrowds
{
	private float[,] rho;
	private Vector2[,] vAve;
	private float[,] gPred;
	private Vector2[,] f;

	private float[,] C;
	private Vector2[,] dC;
	private float[,] Phi;
	private Vector2[,] dPhi;
	private Vector2[,] v;

	public ContinuumCrowds (CC_Map_Package map, List<CC_Unit_Goal_Group> unitGoals)
	{
		int N, M;
		N = map.h.GetLength (0);
		M = map.h.GetLength (1);

		rho = new float[N,M];
		vAve = new Vector2[N,M];
		gPred = new float[N,M];
		f = new Vector2[N,M];

		// populate density field
		// populate average velocity field
		// populate predictive discomfort field
		// populate speed field
		foreach (CC_Unit_Goal_Group cc_ugg in unitGoals) {
			// determine the Rect to bound the current units and their goal
			// calculate cost field
			// calculate potential field (Eikonal solver)
			// calculate velocity field
			// assign new velocities to each unit in List<CC_Unit> units
		}
	}

	void basicDensitySplat() {

	}
}
