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
	}

	void Start () {
	}

	void Update() {
			testCC();
	}

	void testCC ()
	{
		My_CC_map_package = new CC_Map_Package (
			mapAnalyzer.S.get_dh (),
			mapAnalyzer.S.get_h (), 
			mapAnalyzer.S.get_dhdx (),
			mapAnalyzer.S.get_dhdy (),
			mapAnalyzer.S.get_g ()
		);

		Rect r;

		My_CC_unit_goal_groups = new List<CC_Unit_Goal_Group> ();

		r = new Rect(new Vector2(30,10), new Vector2(10,10));
		temp_cc_units = new List<CC_Unit>(); 
		temp_cc_units.Add(My_CC_units[0]);
		temp_unit_goal_group = new CC_Unit_Goal_Group(r, temp_cc_units);

		My_CC_unit_goal_groups.Add(temp_unit_goal_group);

		r = new Rect(new Vector2(5,15), new Vector2(10,10));
		temp_cc_units = new List<CC_Unit>(); 
		temp_cc_units.Add(My_CC_units[1]);
		temp_unit_goal_group = new CC_Unit_Goal_Group(r, temp_cc_units);

		My_CC_unit_goal_groups.Add(temp_unit_goal_group);

		CC = new ContinuumCrowds(My_CC_map_package, My_CC_unit_goal_groups);


		tileAndColorSystem.S.setTileColor(CC.gP,Color.red);

	}
}