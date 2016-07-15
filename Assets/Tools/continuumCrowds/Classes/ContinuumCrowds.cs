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
	//public Vector2[,] dh;
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
	public float[,] rho;
	public Vector2[,] vAve;
	public float[,] gPred;
	public Vector2[,] f;

	public float[,] C;
	public Vector2[,] dC;
	public float[,] Phi;
	public Vector2[,] dPhi;
	public Vector2[,] v;

	public ContinuumCrowds (CC_Map_Package map, List<CC_Unit_Goal_Group> unitGoals)
	{
		int N, M;
		N = map.h.GetLength (0);
		M = map.h.GetLength (1);

		rho = new float[N,M];
		vAve = new Vector2[N,M];
		gPred = new float[N,M];
		f = new Vector2[N,M];


		// these next fields must be computed for each unit in the entire list:
		// 		populate density field
		// 		populate average velocity field
		// 		populate predictive discomfort field
		// 		populate speed field
		foreach (CC_Unit_Goal_Group cc_ugg in unitGoals) {
			foreach(CC_Unit cc_u in cc_ugg.units) {
				densityFieldComputation(cc_u); // density field
				// average velocity field
				// predictive discomfort field
				// speed field
			}
		}

		// these next fields must be computed for each goal in the entire list:
		foreach (CC_Unit_Goal_Group cc_ugg in unitGoals) {
			// determine the Rect to bound the current units and their goal
			// calculate cost field
			// calculate potential field (Eikonal solver)
			// calculate velocity field
			// assign new velocities to each unit in List<CC_Unit> units
		}
	}

	void densityFieldComputation(CC_Unit cc_u) {
		Vector2 cc_u_pos = cc_u.getLocalPosition();

		int xInd = (int)Mathf.Floor(cc_u_pos.x);
		int yInd = (int)Mathf.Floor(cc_u_pos.y);

		float delx = cc_u_pos.x - xInd;
		float dely = cc_u_pos.y - yInd;

		rho[xInd,yInd] = Mathf.Min(1-delx, 1-dely);
		rho[xInd+1,yInd] = Mathf.Min(delx, 1-dely);
		rho[xInd,yInd+1] = Mathf.Min(1-delx, dely);
		rho[xInd+1,yInd+1] = Mathf.Min(delx, dely);
	}
}
