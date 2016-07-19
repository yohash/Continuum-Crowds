using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	void Awake ()
	{
		My_CC_units.AddRange (GetComponentsInChildren<CC_Unit> ());
		My_CC_unit_goal_groups = new List<CC_Unit_Goal_Group> ();

		Rect r;
		r = new Rect(new Vector2(1,1), new Vector2(1,1));
		temp_cc_units = new List<CC_Unit>(); 
		temp_cc_units.Add(My_CC_units[0]);
		temp_unit_goal_group = new CC_Unit_Goal_Group(r, temp_cc_units);

		My_CC_unit_goal_groups.Add(temp_unit_goal_group);

		r = new Rect(new Vector2(1,1), new Vector2(1,1));
		temp_cc_units = new List<CC_Unit>(); 
		temp_cc_units.Add(My_CC_units[1]);
		temp_unit_goal_group = new CC_Unit_Goal_Group(r, temp_cc_units);

		My_CC_unit_goal_groups.Add(temp_unit_goal_group);
	}

	void Start () {
		Invoke("start_CC",0.5f);
	}

	void Update() {
	}

	void start_CC() {

		My_CC_map_package = new CC_Map_Package (
			mapAnalyzer.S.get_dh (),
			mapAnalyzer.S.get_h (), 
			mapAnalyzer.S.get_dhdx (),
			mapAnalyzer.S.get_dhdy (),
			mapAnalyzer.S.get_g ()
		);

		CC = new ContinuumCrowds(My_CC_map_package, My_CC_unit_goal_groups);

		float max = 0f;
		float[,] phic = CC.Phi;
		Vector2[,] dphic = CC.dPhi;

		for (int n=0; n<CC.Phi.GetLength(0); n++) {
			for (int m=0; m<CC.Phi.GetLength(1); m++) {
				if (phic[n,m] > max && phic[n,m]!=Mathf.Infinity) {max = phic[n,m];}
			}
		}
		for (int n=0; n<CC.Phi.GetLength(0); n++) {
			for (int m=0; m<CC.Phi.GetLength(1); m++) {
				phic[n,m] /= max/1.5f ;
				dphic[n,m].x *= .5f;
				dphic[n,m].y *= .5f;
			}
		}

		tileAndColorSystem.S.setTileColor(dphic, Color.red);

		int dimone = 11;

//		print(CC.f[dimone,0]);
//		print(CC.f[dimone,1]);
//		print(CC.f[dimone,2]);
//		print(CC.f[dimone,3]);
//		print(CC.f[dimone,4]);
//		print(CC.f[dimone,5]);
//		print(CC.f[dimone,6]);
//		print(CC.f[dimone,7]);
//		print(CC.f[dimone,8]);
//		print(CC.f[dimone,9]);

//		print(dphic[dimone,0]);
//		print(dphic[dimone,1]);
//		print(dphic[dimone,2]);
//		print(dphic[dimone,3]);
//		print(dphic[dimone,4]);
//		print(dphic[dimone,5]);
//		print(dphic[dimone,6]);
//		print(dphic[dimone,7]);
//		print(dphic[dimone,8]);
//		print(dphic[dimone,9]);
//		print(dphic[dimone,10]);
//		print(dphic[dimone,11]);
//		print(dphic[dimone,12]);
//		print(dphic[dimone,13]);
//		print(dphic[dimone,14]);
//		print(dphic[dimone,15]);
//		print(dphic[dimone,16]);
//		print(dphic[dimone,17]);
//		print(dphic[dimone,18]);
//		print(dphic[dimone,19]);

//		tileAndColorSystem.S.setTileColor(dimone,0,Color.white);
//		tileAndColorSystem.S.setTileColor(dimone,19,Color.blue);
	}

}