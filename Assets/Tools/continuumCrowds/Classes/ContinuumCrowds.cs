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
	public float[,] rho;			// density field
	public Vector2[,] vAve;			// average velocity field
	public float[,] gP;				// predictive discomfort
	public Vector2[,] f;			// speed field

	float seconds_predictiveDiscomfort=4f;

	public float[,] C;				// cost field
	public Vector2[,] dC;			// cost field gradient
	public float[,] Phi;			// potential field
	public Vector2[,] dPhi;			// potential field gradient
	public Vector2[,] v;			// final velocity

	int N, M;

	public ContinuumCrowds (CC_Map_Package map, List<CC_Unit_Goal_Group> unitGoals)
	{
		N = map.h.GetLength (0);
		M = map.h.GetLength (1);

		rho = new float[N, M];
		vAve = new Vector2[N, M];
		gP = new float[N, M];
		f = new Vector2[N, M];

		// these next fields must be computed for each unit in the entire list:
		// 		populate density field
		// 		populate average velocity field
		// 		populate predictive discomfort field
		// 		populate speed field
		foreach (CC_Unit_Goal_Group cc_ugg in unitGoals) {
			foreach (CC_Unit cc_u in cc_ugg.units) {
				densityFieldComputation (cc_u); 		// density field
				applyPredictiveDiscomfort (seconds_predictiveDiscomfort, cc_u);		
														// predictive discomfort field
				// speed field
			}
		}
		// now that the velocity field and density fields are implemented,
		// divide the velocity by densty to get average velocity field
		averageVelFieldComptutation ();	

		// these next fields must be computed for each goal in the entire list:
		foreach (CC_Unit_Goal_Group cc_ugg in unitGoals) {
			// determine the Rect to bound the current units and their goal
			// calculate cost field
			// calculate potential field (Eikonal solver)
			// calculate velocity field
			// assign new velocities to each unit in List<CC_Unit> units
		}
	}

	void densityFieldComputation (CC_Unit cc_u)
	{
		Vector2 cc_u_pos = cc_u.getLocalPosition ();

		linear1stOrderSplat (cc_u_pos.x, cc_u_pos.y, rho, 1);

		int xInd = (int)Mathf.Floor (cc_u_pos.x);
		int yInd = (int)Mathf.Floor (cc_u_pos.y);

		velocityFieldPointAddition (xInd, yInd, cc_u.getVelocity ());
		velocityFieldPointAddition (xInd + 1, yInd, cc_u.getVelocity ());
		velocityFieldPointAddition (xInd, yInd + 1, cc_u.getVelocity ());
		velocityFieldPointAddition (xInd + 1, yInd + 1, cc_u.getVelocity ());
	}

	void velocityFieldPointAddition (int x, int y, Vector2 v)
	{
		vAve [x, y] += v * rho [x, y];
	}

	void averageVelFieldComptutation ()
	{
		for (int n = 0; n < N; n++) {
			for (int m = 0; m < M; m++) {
				if (rho [n, m] != 0) {
					vAve [n, m] /= rho [n, m];
				}
			}
		}
	}

	void applyPredictiveDiscomfort (int numSec, CC_Unit cc_u)
	{
		Vector2 newLoc;
		float sc;

		Vector2 xprime = cc_u.getLocalPosition () + cc_u.getVelocity * numSec;
		float vfMag = Vector2.Distance (cc_u.getLocalPosition (), xprime);

		for (int i = 1; i < vfMag; i++) {
			newLoc = Vector2.MoveTowards (cc_u.getLocalPosition, xprime, i);
			sc = (vfMag - i) / vfMag;				// inverse scale
			linear1stOrderSplat (newLoc, gP, sc);
		}
	}


	void linear1stOrderSplat (Vector2 v, float[,] mat, float scalar) {
		linear1stOrderSplat (v.x, v.y, mat, scalar);
	}
	void linear1stOrderSplat (float x, float y, float[,] mat, float scalar)
	{
		int xInd = (int)Mathf.Floor (x);
		int yInd = (int)Mathf.Floor (y);

		float delx = x - xInd;
		float dely = y - yInd;

		// use += to stack density field up
		mat [xInd, yInd] += Mathf.Min (1 - delx, 1 - dely) * scalar;			// ADD CHECKS TO MAKE SURE
		mat [xInd + 1, yInd] += Mathf.Min (delx, 1 - dely) * scalar;			// YOU ARENT REFERENCNG NON
		mat [xInd, yInd + 1] += Mathf.Min (1 - delx, dely) * scalar;			// INDEX-ABLE LOCATIONS!!!
		mat [xInd + 1, yInd + 1] += Mathf.Min (delx, dely) * scalar;
	}

	void testPointForValidity (int x, int y)
	{
		// TBC TO BE CODED
		// check to make sure the point is not in a place dis-allowed by terrain (slope)
		// check to make sure the point is not outside the grid
		// check to make sure the point is not on a place of absolute discomfort (like inside a building)
		// side note: should absolute discomfort encompass too-steep-terrain slopes?
	}
}