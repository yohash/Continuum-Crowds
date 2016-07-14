using UnityEngine;
using System.Collections;

public class tile : MonoBehaviour {

	public Color myColor;
	Color col;

	public Renderer render;
	public Material mat;


	void Awake () {
		render = GetComponent<Renderer>();
		mat = render.material;

		col = Color.white;
		col.a = .1f;
		setColor(col);
	}

	public void setColor(Color c) {
		c.a = 0.5f;
		myColor = c;
		mat.color = myColor;
	}
}
