using UnityEngine;
using System.Collections;

public class CC_Unit : MonoBehaviour {

	public float CC_Unit_MaxSpeed;
	public Rect CC_Unit_Goal;


	// private variables
	private Vector3 _CC_Unit_velocity;
	private Transform _CC_Unit_transform;
	private Rect _CC_worldSpace_Goal;


	// getters and setters
	public Vector3 getVelocity() {return _CC_Unit_velocity;}
	public Transform getTransform() {return _CC_Unit_transform;}
	public Rect getWorldSpaceGoal() {return _CC_worldSpace_Goal;}

	private void setVelocity(Vector3 v) {_CC_Unit_velocity = v;}
	private void setGoal(Rect r) {_CC_worldSpace_Goal = r;}


	void Awake () {
		_CC_Unit_transform = this.transform;
	}
	void Start () {	}
	void OnEnable () { }

	void Update () {

	}

}
