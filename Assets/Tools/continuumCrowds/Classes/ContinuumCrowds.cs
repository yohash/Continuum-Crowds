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
	public Vector2[,] dh;
	public float[,] g;

	public CC_Map_Package (Vector2[,] _dh, float[,] _h, float[,] _dhdx, float[,] _dhdy, float[,] _g)
	{
		this.dh = _dh;
		this.h = _h;
		this.dhdx = _dhdx;
		this.dhdy = _dhdy;
		this.g = _g;
	}
}

public class ContinuumCrowds
{
	public float[,] rho;				// density field
	public Vector2[,] vAve;				// average velocity field
	public float[,] gP;					// predictive discomfort
	public Vector4[,] f;				// speed field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)

	float gP_predictiveSeconds = 4f;

	float f_slopeMax = 0.5f;			// correlates roughly to 30-degree incline
	float f_slopeMin = 0f;				// for a min slope, nothing else makes sense...

	float f_rhoMax = 0.6f;
	float f_rhoMin = 0.2f;

	float f_speedMax = 100f;			// I just picked something big here
	float f_speedMin = 0f;				// minimum would be 0

	public Vector4[,] C;				// cost field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
	public float[,] Phi;				// potential field
	public Vector2[,] dPhi;				// potential field gradient
	public Vector2[,] v;				// final velocity

	CC_Map_Package theMap;
	List<CC_Unit_Goal_Group> theUnitGoalGroups;

	// this array of Vect2's correlates to our data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
	Vector2[] dir = new Vector2[] {Vector2.right, Vector2.up, Vector2.left, Vector2.down };

	int N, M;

	public ContinuumCrowds (CC_Map_Package map, List<CC_Unit_Goal_Group> unitGoals)
	{
		theMap = map;
		theUnitGoalGroups = unitGoals;

		N = map.h.GetLength (0);
		M = map.h.GetLength (1);

		rho = new float[N, M];
		vAve = new Vector2[N, M];
		gP = new float[N, M];
		f = new Vector4[N, M];

		// these next fields must be computed for each unit in the entire list:
		// 		populate density field
		// 		populate average velocity field
		// 		populate predictive discomfort field
		// 		populate speed field
		foreach (CC_Unit_Goal_Group cc_ugg in theUnitGoalGroups) {
			foreach (CC_Unit cc_u in cc_ugg.units) {
				// (1) density field and velocity
				computeDensityField (cc_u); 		
				// (2) predictive discomfort field
				applyPredictiveDiscomfort (gP_predictiveSeconds, cc_u);	
			}
		}
		// (3) 	now that the velocity field and density fields are implemented,
		// 		divide the velocity by densty to get average velocity field
		// **** FOR NOW, LETS CALCLUATE THE WHOLE THING. LATER, WE SHOULD SAVE COMPUTATION
		// **** BY ONLY CALCULATING THE NECESSARY POINTS USING THE COST FUNCTION
		computeAverageVelocityField ();	

		// (4)	now that the average velocity field is computed, and the density
		// 		field is in place, we calculate the speed field
		computeSpeedField ();

		// these next fields must be computed for each goal in the entire list:
		foreach (CC_Unit_Goal_Group cc_ugg in unitGoals) {

			// first we need to compute new dynamic boundaries for small unit groups

			C = new Vector4[N, M];
			Phi = new float[N, M];
			dPhi = new Vector2[N, M];
			v = new Vector2[N, M];

			// determine the Rect to bound the current units and their goal
			// calculate cost field
			// calculate potential field (Eikonal solver)
			// calculate velocity field
			// assign new velocities to each unit in List<CC_Unit> units
		}
	}

	void computeDensityField (CC_Unit cc_u)
	{
		Vector2 cc_u_pos = cc_u.getLocalPosition ();

		linear1stOrderSplat (cc_u_pos.x, cc_u_pos.y, rho, 1);

		int xInd = (int)Mathf.Floor (cc_u_pos.x);
		int yInd = (int)Mathf.Floor (cc_u_pos.y);

		computeVelocityFieldPoint (xInd, yInd, cc_u.getVelocity ());
		computeVelocityFieldPoint (xInd + 1, yInd, cc_u.getVelocity ());
		computeVelocityFieldPoint (xInd, yInd + 1, cc_u.getVelocity ());
		computeVelocityFieldPoint (xInd + 1, yInd + 1, cc_u.getVelocity ());
	}

	void computeVelocityFieldPoint (int x, int y, Vector2 v)
	{
		if (isPointValid (x, y)) {
			vAve [x, y] += v * rho [x, y];
		}
	}

	void computeAverageVelocityField ()
	{
		for (int n = 0; n < N; n++) {
			for (int m = 0; m < M; m++) {
				if (rho [n, m] != 0) {
					vAve [n, m] /= rho [n, m];
				}
			}
		}
	}

	void applyPredictiveDiscomfort (float numSec, CC_Unit cc_u)
	{
		Vector2 newLoc;
		float sc;

		Vector2 xprime = cc_u.getLocalPosition () + cc_u.getVelocity () * numSec;
		float vfMag = Vector2.Distance (cc_u.getLocalPosition (), xprime);

		for (int i = 1; i < vfMag; i++) {
			newLoc = Vector2.MoveTowards (cc_u.getLocalPosition (), xprime, i);
			sc = (vfMag - i) / vfMag;				// inverse scale
			linear1stOrderSplat (newLoc, gP, sc);
		}
	}

	void computeSpeedField ()
	{
		for (int n = 0; n < N; n++) {
			for (int m = 0; m < M; m++) {
				for (int d = 0; d < 4; d++) {
					f[n,m][d] = computeSpeedFieldValue(n,m,dir[d]);
				}
			}
		} 
	}

	float computeSpeedFieldValue(int x, int y, Vector2 direction) {
		int xInto = x+(int)direction.x;
		int yInto = y+(int)direction.y;

		// if we're looking in an invalid direction, dont store this value
		if (!isPointValid(xInto,yInto)) {return 9999f;}

		// otherwise, run the speed field calculation
		float ff=0, ft=0, fv=0;
		float r = rho[xInto,yInto];				

		// test the density INTO WHICH we move: 
		if (r < f_rhoMin) {	// rho < rho_min calc
			ft = computeTopographicalSpeed(x, y, direction);
			ff = ft;
		} else if (r > f_rhoMax) {	// rho > rho_max calc
			fv = computeFlowSpeed(xInto, yInto, direction);
			ff = fv;
		} else {	// rho in-between calc
			fv = computeFlowSpeed(xInto,yInto,direction);
			ft = computeTopographicalSpeed(x, y, direction) ;
			ff = ft + (r-f_rhoMin) / (f_rhoMax-f_rhoMin) * (fv-ft);
		}

		return ff;
	}


	float computeTopographicalSpeed(int x, int y, Vector2 dir) {
		// first, calculate the gradient in the direction we are looking. By taking the dot with Direction,
		// we extract the direction we're looking and assign it a proper sign
		// i.e. if we look left (x=-1) we want -dhdx(x,y), because the gradient is assigned with a positive x
		// 		therefore:		also, Vector.left = [-1,0]
		//						Vector2.Dot(Vector.left, dh[x,y]) = -dhdx;
		float hGradientInDirection = Vector2.Dot(dir, theMap.dh[x,y]) ;
		// calculate the speed field from the equation
		return (f_speedMax + (hGradientInDirection - f_slopeMin) / (f_slopeMax - f_slopeMin) * (f_speedMin - f_speedMax) );
	}

	float computeFlowSpeed(int xI, int yI, Vector2 dir) {
		// the flow speed is simply the average velocity field of the region INTO WHICH we are looking,
		// dotted with the direction vector
		return Vector2.Dot(vAve[xI,yI],dir);
	}


	// ******************************************************************************
	//			TOOLS AND UTILITIES
	//******************************************************************************
	void linear1stOrderSplat (Vector2 v, float[,] mat, float scalar)
	{
		linear1stOrderSplat (v.x, v.y, mat, scalar);
	}

	void linear1stOrderSplat (float x, float y, float[,] mat, float scalar)
	{
		int xInd = (int)Mathf.Floor (x);
		int yInd = (int)Mathf.Floor (y);

		float delx = x - xInd;
		float dely = y - yInd;

		// use += to stack density field up
		if (isPointValid (xInd, yInd)) 			{mat [xInd, yInd] += Mathf.Min (1 - delx, 1 - dely) * scalar;}
		if (isPointValid (xInd + 1, yInd)) 		{mat [xInd + 1, yInd] += Mathf.Min (delx, 1 - dely) * scalar;}
		if (isPointValid (xInd, yInd + 1)) 		{mat [xInd, yInd + 1] += Mathf.Min (1 - delx, dely) * scalar;}
		if (isPointValid (xInd + 1, yInd + 1)) 	{mat [xInd + 1, yInd + 1] += Mathf.Min (delx, dely) * scalar;}
	}
	
	bool isPointValid (Vector2 v)
	{
		return isPointValid((int)v.x, (int)v.y);
	}

	bool isPointValid (int x, int y)
	{
		// check to make sure the point is not in a place dis-allowed by terrain (slope)
				// TBC
		// check to make sure the point is not outside the grid
		if ((x < 0) || (y < 0) || (x > N - 1) || (y > M - 1)) {
			return false;
		}
		// check to make sure the point is not on a place of absolute discomfort (like inside a building)
				// TBC
		// side note: should absolute discomfort encompass too-steep-terrain slopes?

		return true;
	}
}