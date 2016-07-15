using UnityEngine;
using System.Collections;

public class CC_Unit : MonoBehaviour {
	// a CC_Unit is ALWAYS localized to a portion of the larger map
	// and has this localization instantiated on a continuum crowds call

	// private variables
	public Vector2 _CC_Unit_velocity;
	public Vector2 _CC_Unit_localPosition;		// hold position vs. transform as only local space is relevant
	public Rect _CC_Unit_localGoal;
	Vector2 tmp2;
	Vector3 tmp3;

	public Vector2 _CC_worldspace_anchor;
	public Rect _CC_worldspace_goal;

	// getters and setters
	public Vector2 getVelocity() {return _CC_Unit_velocity;}
	public Vector2 getLocalPosition() {return _CC_Unit_localPosition;}
	public Rect getLocalGoal() {return _CC_Unit_localGoal;}

	private void setVelocity(Vector3 v) {_CC_Unit_velocity = v;}
	private void setGoal(Rect r) {_CC_Unit_localGoal = r;}


	void Awake () {	}
	void Start () {	}
	void OnEnable () { }

	void Update () {
		// SUPER temporary code
		tmp3 = transform.position;
		_CC_Unit_localPosition = new Vector2(tmp3.x,tmp3.z);
	}

	public void packageForCCSubmission(Vector2 worldSpace_anchor, Rect worldSpace_goal) {
		setGoal(new Rect(worldSpace_goal.min - worldSpace_anchor, worldSpace_goal.size));
	}
}
