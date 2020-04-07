using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeightMapDataHarvester : MonoBehaviour
{
	public static HeightMapDataHarvester S;

	[Header("Assign these variables")]
	public float TerrainMaxWorldHeight;
	public float TerrainMaxHeightDifferential;

	public float MapWidth;
	public float MapHeight;
	public Vector2 Center;

	public float StepSize = 1;

	public Texture2D HeightMap;
	public Texture2D GradientMap;

	// private local vars
	private int xSteps;
	private int zSteps;
	private float[,] heightMap;
	private float[,] absGradientMap;
	private float[,] xGradMap;
	private float[,] yGradMap;

	private Vector3 rayPoint;
	private Vector3 rayDir;


	// *****************************************************************************************************************
	// 		MONOBEHAVIOURS
	// *****************************************************************************************************************
	private void Awake()
	{
		S = this;
	}

	private void Start()
	{
		xSteps = (int)(MapWidth / StepSize);
		zSteps = (int)(MapHeight / StepSize);
	}

	// *****************************************************************************************************************
	// 		PUBLIC ACCESSORS
	// *****************************************************************************************************************
	public Texture2D MapToTexture(float[,] tex)
	{
		tex = normalizeMap(tex);
		return TextureGenerator.TextureFromMap(tex);
	}

	public Vector3[] GetHeightAndNormalDataForPoint(Location l)
	{
		return getHeightAndNormalDataForPoint(l.x, l.y);
	}

	public float[,] GenerateHeightMap()
	{
		heightMap = new float[xSteps, zSteps];
		float xoffset = StepSize / 2f + (Center.x - MapWidth / 2f);
		float zoffset = StepSize / 2f + (Center.y - MapHeight / 2f);

		for (int i = 0; i < xSteps; i++) {
			for (int k = 0; k < zSteps; k++) {
				heightMap[i, k] = getHeightAndNormalDataForPoint(StepSize * i + xoffset, StepSize * k + zoffset)[0].y;
			}
		}
		return heightMap;
	}

	public float[,] GenerateGradientMaps()
	{
		absGradientMap = new float[xSteps, zSteps];
		xGradMap = new float[xSteps, zSteps];
		yGradMap = new float[xSteps, zSteps];

		for (int i = 0; i < xSteps; i++) {
			for (int k = 0; k < zSteps; k++) {
				if ((i != 0) && (i != xSteps - 1) && (k != 0) && (k != zSteps - 1)) {             // generic spot
																																													// xGradMap[i,k] = Mathf.Abs(heightMap[i-1,k] - heightMap[i+1,k]);
																																													// yGradMap[i,k] = Mathf.Abs(heightMap[i,k-1] - heightMap[i,k+1]);
																																													// absGradientMap[i,k] = Mathf.Max(xGradMap[i,k],yGradMap[i,k]);
					writeGradientMapData(i, k, i - 1, i + 1, k - 1, k + 1);
				} else if ((i == 0) && (k == zSteps - 1)) { writeGradientMapData(i, k, i, i + 1, k - 1, k); }   // upper left corner
					else if ((i == xSteps - 1) && (k == 0)) { writeGradientMapData(i, k, i - 1, i, k, k + 1); } // bottom left corner
					else if ((i == 0) && (k == 0)) { writeGradientMapData(i, k, i, i + 1, k, k + 1); }  // upper left corner
					else if ((i == xSteps - 1) && (k == zSteps - 1)) { writeGradientMapData(i, k, i - 1, i, k - 1, k); } // bottom right corner
					else if (i == 0) { writeGradientMapData(i, k, i, i + 1, k - 1, k + 1); }  // top edge
					else if (i == xSteps - 1) { writeGradientMapData(i, k, i - 1, i, k - 1, k + 1); } // bot edge
					else if (k == 0) { writeGradientMapData(i, k, i - 1, i + 1, k, k + 1); }  // left edge
					else if (k == zSteps - 1) { writeGradientMapData(i, k, i - 1, i + 1, k - 1, k); } // right edge
			}
		}
		return absGradientMap;
	}

	public List<Location> GetUnpassableTerrain(float threshhold)
	{
		List<Location> theList = new List<Location>();
		for (int i = 0; i < xSteps; i++) {
			for (int k = 0; k < zSteps; k++) {
				if (absGradientMap[i, k] > threshhold) { theList.Add(new Location(i, k)); }
			}
		}
		return theList;
	}

	public float GetHeightData(Location l)
	{
		return heightMap[l.x, l.y];
	}

	// *****************************************************************************************************************
	// 		HELPer functions
	// *****************************************************************************************************************
	private void writeGradientMapData(int x, int y, int xMin, int xMax, int yMin, int yMax)
	{
		xGradMap[x, y] = Mathf.Abs(heightMap[xMin, y] - heightMap[xMax, y]);
		yGradMap[x, y] = Mathf.Abs(heightMap[x, yMin] - heightMap[x, yMax]);
		absGradientMap[x, y] = Mathf.Max(xGradMap[x, y], yGradMap[x, y]);
	}

	private Vector3[] getHeightAndNormalDataForPoint(float x, float z)
	{
		rayPoint = new Vector3(x, TerrainMaxWorldHeight * 1.01f, z);
		rayDir = new Vector3(0, -TerrainMaxHeightDifferential, 0);

		Ray ray = new Ray(rayPoint, rayDir);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, TerrainMaxHeightDifferential * 1.05f)) {
			return new Vector3[2] { hit.point, hit.normal };
		}
		return new Vector3[2] { Vector3.zero, Vector3.zero };
	}

	private float[,] normalizeMap(float[,] unNormMap)
	{
		float maxHeight = 0f;
		for (int i = 0; i < xSteps; i++) {
			for (int k = 0; k < zSteps; k++) {
				if (unNormMap[i, k] > maxHeight) { maxHeight = heightMap[i, k]; }
			}
		}
		if (maxHeight > 0) {
			for (int i = 0; i < xSteps; i++) {
				for (int k = 0; k < zSteps; k++) {
					unNormMap[i, k] /= maxHeight;
				}
			}
		}
		return unNormMap;
	}
}
