using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Threading;
using Foundation.Tasks;


public class CC_UnitManager : MonoBehaviour
{
	// a CC_UnitManager holds lists of its units, and packages them
	// for submission into the ContinuumCrowds class

	public List<CC_Unit> My_CC_units;

	private CC_Map_Package My_CC_map_package;
	private List<CC_Unit_Goal_Group> My_CC_unit_goal_groups;

	CC_Unit_Goal_Group temp_unit_goal_group;
	List<CC_Unit> temp_cc_units;

	ContinuumCrowds CC;

	bool CC_1stIter_done = false;

	void Awake ()
	{
		My_CC_units.AddRange (GetComponentsInChildren<CC_Unit> ());
		My_CC_unit_goal_groups = new List<CC_Unit_Goal_Group> ();

		Rect r;
		r = new Rect(new Vector2(30,20), new Vector2(1,1));
		temp_cc_units = new List<CC_Unit>(); 
		temp_cc_units.Add(My_CC_units[0]);
		temp_unit_goal_group = new CC_Unit_Goal_Group(r, temp_cc_units);

		My_CC_unit_goal_groups.Add(temp_unit_goal_group);

		r = new Rect(new Vector2(30,20), new Vector2(1,1));
		temp_cc_units = new List<CC_Unit>(); 
		temp_cc_units.Add(My_CC_units[1]);
		temp_unit_goal_group = new CC_Unit_Goal_Group(r, temp_cc_units);

		My_CC_unit_goal_groups.Add(temp_unit_goal_group);
	}

	void Start () {
		Invoke("start_CC",0.5f);
	}
	void start_CC() {
		StartCoroutine("start_CC_task");

	}

	void Update() {
		if (CC_1stIter_done) {
			// distribute velocities to each unit in each unit-goal-group
			float vx, vy;
			int xf, yf;
			Vector2 newV;
			foreach(CC_Unit_Goal_Group ccugg in My_CC_unit_goal_groups) {
				foreach(CC_Unit ccu in ccugg.units) {
					newV = interpolateBetweenValues(ccu.getLocalPosition().x,ccu.getLocalPosition().y,CC.v);


					ccu.setVelocity(newV);
				}
			}
		}
	}

	IEnumerator start_CC_task() {
		while (true) {
			My_CC_map_package = new CC_Map_Package (
				mapAnalyzer.S.get_dh (),
				mapAnalyzer.S.get_h (), 
				mapAnalyzer.S.get_g ()
			);

//			float TIMEST = Time.realtimeSinceStartup;

			CC = new ContinuumCrowds(My_CC_map_package, My_CC_unit_goal_groups);

			CC_1stIter_done = true;

//			float dT = Time.realtimeSinceStartup;
//			Debug.Log("TOTAL time: "+(dT-TIMEST));

			float[,] phic = CC.Phi;
			Vector2[,] dphic = CC.dPhi;
			float max = 0f;

			for(int m=0; m<phic.GetLength(0); m++) {
				for(int n=0; n<phic.GetLength(1); n++) {
					if (phic[m,n]>max && !float.IsInfinity(phic[m,n])) {max = phic[m,n];}
				}
			}

			int d = 21;
//			Debug.Log(CC.f[d,40]);
//			Debug.Log(CC.f[d,39]);
//			Debug.Log(CC.f[d,38]);
//			Debug.Log(CC.f[d,37]);
//			Debug.Log(CC.f[d,36]);
//			Debug.Log(CC.f[d,35]);
//			Debug.Log(CC.f[d,34]);
//			Debug.Log(CC.f[d,33]);
//			Debug.Log(CC.f[d,32]);
//			Debug.Log(CC.f[d,31]);
//			Debug.Log(CC.f[d,30]);
			//
//			Debug.Log(CC.C[d,40]);
//			Debug.Log(CC.C[d,39]);
//			Debug.Log(CC.C[d,38]);
//			Debug.Log(CC.C[d,37]);
//			Debug.Log(CC.C[d,36]);
//			Debug.Log(CC.C[d,35]);
//			Debug.Log(CC.C[d,34]);
//			Debug.Log(CC.C[d,33]);
//			Debug.Log(CC.C[d,32]);
//			Debug.Log(CC.C[d,31]);
//			Debug.Log(CC.C[d,30]);

//			print("VAL: phiR-"+CC.Phi[22,33]+",phiC-"+CC.Phi[21,33]+", phiL-"+CC.Phi[20,33]+", costInto-"+CC.C[21,33]);
//			print("the two var (r,l): "+(CC.C[21,33][0] + CC.Phi[22,33])+", "+(CC.C[21,33][2] + CC.Phi[20,33]));

//			Debug.Log(CC.dPhi[d,40]);
//			Debug.Log(CC.dPhi[d,39]);
//			Debug.Log(CC.dPhi[d,38]);
//			Debug.Log(CC.dPhi[d,37]);
//			Debug.Log(CC.dPhi[d,36]);
//			Debug.Log(CC.dPhi[d,35]);
//			Debug.Log(CC.dPhi[d,34]);
//			Debug.Log(CC.dPhi[d,33]);
//			Debug.Log(CC.dPhi[d,32]);
//			Debug.Log(CC.dPhi[d,31]);
//			Debug.Log(CC.dPhi[d,30]);

			tileAndColorSystem.S.setTileColor(CC.v, Color.red);
//
			tileAndColorSystem.S.setTileColor(d,30,Color.white);
			tileAndColorSystem.S.setTileColor(d,40,Color.white);


			yield return new WaitForSeconds(3f);
		}
	}

	Vector2 interpolateBetweenValues(float x, float y, Vector2[,] array)
	{
		float xcomp,ycomp;

		int topLeftX = (int)Mathf.Floor(x);
		int topLeftY = (int)Mathf.Floor(y);

		float xAmountRight = x - topLeftX;
		float xAmountLeft = 1.0f - xAmountRight;
		float yAmountBottom = y - topLeftY;
		float yAmountTop = 1.0f - yAmountBottom;

		Vector4 valuesX = Vector4.zero;
		valuesX[0] = array[topLeftX, topLeftY].x;
		valuesX[1] = array[topLeftX + 1, topLeftY].x;
		valuesX[2] = array[topLeftX, topLeftY + 1].x;
		valuesX[3] = array[topLeftX + 1, topLeftY + 1].x;
		for (int n=0; n<4; n++) {
			if (valuesX[n] == float.NaN) {valuesX[n] = 0f;}
		}

		float averagedXTop = valuesX[0] * xAmountLeft + valuesX[1] * xAmountRight;
		float averagedXBottom = valuesX[2] * xAmountLeft + valuesX[3] * xAmountRight;

		xcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

		Vector4 valuesY = Vector4.zero;
		valuesY[0] = array[topLeftX, topLeftY].y;
		valuesY[1] = array[topLeftX + 1, topLeftY].y;
		valuesY[2] = array[topLeftX, topLeftY + 1].y;
		valuesY[3] = array[topLeftX + 1, topLeftY + 1].y;
		for (int n=0; n<4; n++) {
			if (valuesY[n] == float.NaN) {valuesY[n] = 0f;}
		}

		averagedXTop = valuesY[0] * xAmountLeft + valuesY[1] * xAmountRight;
		averagedXBottom = valuesY[2] * xAmountLeft + valuesY[3] * xAmountRight;

		ycomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

//		print("4 vals from ("+x+","+y+"): "+new Vector2(valuesX[0], valuesY[0])+", "+new Vector2(valuesX[1], valuesY[1])+". "+new Vector2(valuesX[2], valuesY[2])+", "+new Vector2(valuesX[3], valuesY[3]));
//		print("new INTERP val: "+new Vector2(xcomp,ycomp));

		if (float.IsNaN(xcomp)) {xcomp = 0f;}
		if (float.IsNaN(ycomp)) {ycomp = 0f;}

		return new Vector2(xcomp,ycomp);
	}

}