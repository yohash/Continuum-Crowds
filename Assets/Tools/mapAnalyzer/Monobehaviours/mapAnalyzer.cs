using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mapAnalyzer : MonoBehaviour {
	
	public static mapAnalyzer S;

	public bool ___MapInputs___;
	public float terrainMaxWorldHeight;
	public float terrainMaxHeightDifferential;

	public float mapWidthX, mapHeightZ;
	public float centerX, centerZ;
	public float stepSize;

	public bool ___TextureMaps___;
	public Texture2D heightMap;

	// *****************************************
	int xSteps, zSteps;
	float[,] h, dhdx, dhdy, g;


	void Awake () {
		S = this;		
	}

	void Start () {
		xSteps = (int) (mapWidthX/stepSize);
		zSteps = (int) (mapHeightZ/stepSize);

		generateHeightMap();
		generateGradientMaps();
		generateDiscomfortMap();
	}


	// *****************************************************************************************************************
	// 				HEIGHT MAP GENERATION
	// *****************************************************************************************************************
	void generateHeightMap() {
		h = new float[xSteps,zSteps];
		float xoffset = stepSize/2f + (centerX-mapWidthX/2f);
		float zoffset = stepSize/2f + (centerZ-mapHeightZ/2f);

		for (int i=0; i<xSteps; i++) {
			for (int k=0; k<zSteps; k++) {
				h[i,k] = getHeightAndNormalDataForPoint(stepSize*i + xoffset, stepSize*k + zoffset)[0].y;
			}
		}
	}

	void generateGradientMaps() {
		dhdx = new float[xSteps,zSteps];
		dhdy = new float[xSteps,zSteps];

		for (int i=0; i<xSteps; i++) {
			for (int k=0; k<zSteps; k++) {
				if ((i!=0) && (i!=xSteps-1) && (k!=0) && (k!=zSteps-1)) 
													{writeGradientMapData(i,k,i-1,i+1,k-1,k+1);} // generic spot
				else if ((i==0) && (k==zSteps-1)) 	{writeGradientMapData(i,k,i,i+1,k-1,k);} 	// upper left corner
				else if ((i==xSteps-1) && (k==0)) 	{writeGradientMapData(i,k,i-1,i,k,k+1);}	// bottom left corner
				else if ((i==0) && (k==0)) 			{writeGradientMapData(i,k,i,i+1,k,k+1);}	// upper left corner
				else if ((i==xSteps-1) && (k==zSteps-1)) 
													{writeGradientMapData(i,k,i-1,i,k-1,k);} 	// bottom right corner
				else if (i==0) 						{writeGradientMapData(i,k,i,i+1,k-1,k+1);}	// top edge
				else if (i==xSteps-1) 				{writeGradientMapData(i,k,i-1,i,k-1,k+1);}	// bot edge
				else if (k==0) 						{writeGradientMapData(i,k,i-1,i+1,k,k+1);}	// left edge
				else if (k==zSteps-1) 				{writeGradientMapData(i,k,i-1,i+1,k-1,k);}	// right edge								
			}
		}
	}

	void generateDiscomfortMap() {
	//		void generateDiscomfortMap(List<Rect> bldgs) {
		//		foreach (Rect r in bldgs) {		}
		g = new float[xSteps,zSteps];
	}

	// this performs a center-gradient for interior points, 
	// and is how MATLAB calculates gradients of matrices
	void writeGradientMapData(int x, int y, int xMin, int xMax, int yMin, int yMax) {
		dhdx[x,y] = (h[xMax,y] - h[xMin,y]) / (xMax - xMin);
		dhdy[x,y] = (h[x,yMax] - h[x,yMin]) / (yMax - yMin);
	}

	public List<Location> getUnpassableTerrain(float threshhold) {
		List<Location> theList = new List<Location>();
		for (int i=0; i<xSteps; i++) {
			for (int k=0; k<zSteps; k++) {
				if (g[i,k] > threshhold) {theList.Add(new Location(i,k));}
			}
		}
		return theList;
	}


	// *****************************************************************************************************************
	// 		HELPer functions
	// *****************************************************************************************************************
	Vector3 rayPoint, rayDir;
	Vector3[] getHeightAndNormalDataForPoint(float x, float z) {

		rayPoint = new Vector3(x,terrainMaxWorldHeight*1.1f,z);
		rayDir = new Vector3(0,-terrainMaxHeightDifferential,0);

		Ray ray = new Ray(rayPoint , rayDir);

		RaycastHit hit;
		int mask = 1 << 8;
		if (Physics.Raycast(ray, out hit, terrainMaxHeightDifferential*1.5f, mask)) {
			return new Vector3[2] {hit.point,hit.normal};
		}
		return new Vector3[2] {Vector3.zero,Vector3.zero};
	}

	float[,] normalizeMap(float[,] unNormMap) {
		float maxHeight = 0f;
		for (int i=0; i<xSteps; i++) {
			for (int k=0; k<zSteps; k++) {
				if (unNormMap[i,k] > maxHeight) {maxHeight = h[i,k];}
			}
		}
		if (maxHeight > 0) {
			for (int i=0; i<xSteps; i++) {
				for (int k=0; k<zSteps; k++) {
					unNormMap[i,k] /= maxHeight;
				}
			}
		}
		return unNormMap;
	}

	void printOutMatrix(float[,] f) {
		string s;
		print("NEW MATRIX");
		for (int n=0; n<f.GetLength(0); n++) {
			s = "";
			for (int m=0; m<f.GetLength(1); m++) {
				s += f[n,m].ToString() + " "; 
			}
			print(s);
		}
	}
}
