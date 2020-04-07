﻿using UnityEngine;
using System.Collections.Generic;

public class HeightMapGenerator : MonoBehaviour
{
	private const float TEMP_RHO_MAX = 0.6f;

	[Header("Assign these variables")]
	public float TerrainHeightMax;
	public float TerrainHeightMin;

	public Vector2 MapSize;
	public Vector2 Center;

	public float StepSize = 1;

	[Header("Texture Map")]
	public Texture2D HeightMap;
	public Texture2D GradientMap;

	// private local vars
	private float[,] h;
	private float[,] absGradient;

	private float[,] dhdx;
	private float[,] dhdy;

	private float[,] g;

	private Vector2[,] dh;

	// ***************************************************************************
	//  PUBLIC METHODS
	// ***************************************************************************
	public Texture2D MapToTexture()
	{
		float[,] tex = normalizeMap(h);
		return TextureGenerator.TextureFromMap(tex);
	}

	public Vector3[] GetHeightAndNormalDataForPoint(Location l)
	{
		return getHeightAndNormalDataForPoint(l.x, l.y);
	}


	// ***************************************************************************
	//  HEIGHT MAP GENERATION
	// ***************************************************************************
	public float[,] GenerateHeightMap()
	{
		int xSteps = (int)(MapSize.x / StepSize);
		int zSteps = (int)(MapSize.y / StepSize);

		h = new float[xSteps, zSteps];

		float xOffset = StepSize / 2f + (Center.x - MapSize.x / 2f);
		float zOffset = StepSize / 2f + (Center.y - MapSize.y / 2f);

		for (int i = 0; i < xSteps; i++) {
			for (int k = 0; k < zSteps; k++) {
				h[i, k] = getHeightAndNormalDataForPoint(StepSize * i + xOffset, StepSize * k + zOffset)[0].y;
			}
		}
		return h;
	}

	public float[,] GenerateGradientMaps()
	{
		int xSteps = (int)(MapSize.x / StepSize);
		int zSteps = (int)(MapSize.y / StepSize);

		absGradient = new float[xSteps, zSteps];
		dhdx = new float[xSteps, zSteps];
		dhdy = new float[xSteps, zSteps];
		dh = new Vector2[xSteps, zSteps];

		for (int i = 0; i < xSteps; i++) {
			for (int k = 0; k < zSteps; k++) {
				if ((i != 0) && (i != xSteps - 1) && (k != 0) && (k != zSteps - 1)) {
					// generic spot
					writeGradientMapData(i, k, i - 1, i + 1, k - 1, k + 1);
				} else if ((i == 0) && (k == zSteps - 1)) {
					// upper left corner
					writeGradientMapData(i, k, i, i + 1, k - 1, k);
				} else if ((i == xSteps - 1) && (k == 0)) {
					// bottom left corner
					writeGradientMapData(i, k, i - 1, i, k, k + 1);
				} else if ((i == 0) && (k == 0)) {
					// upper left corner
					writeGradientMapData(i, k, i, i + 1, k, k + 1);
				} else if ((i == xSteps - 1) && (k == zSteps - 1)) {
					// bottom right corner
					writeGradientMapData(i, k, i - 1, i, k - 1, k);
				} else if (i == 0) {
					// top edge
					writeGradientMapData(i, k, i, i + 1, k - 1, k + 1);
				} else if (i == xSteps - 1) {
					// bot edge
					writeGradientMapData(i, k, i - 1, i, k - 1, k + 1);
				} else if (k == 0) {
					// left edge
					writeGradientMapData(i, k, i - 1, i + 1, k, k + 1);
				} else if (k == zSteps - 1) {
				} // right edge
				writeGradientMapData(i, k, i - 1, i + 1, k - 1, k);
			}
		}
		return absGradient;
	}

	private void GenerateDiscomfortMap()
	{
		int xSteps = (int)(MapSize.x / StepSize);
		int zSteps = (int)(MapSize.y / StepSize);

		g = new float[xSteps, zSteps];

		for (int i = 0; i < xSteps; i++) {
			for (int k = 0; k < zSteps; k++) {
				if (Mathf.Max(Mathf.Abs(dhdx[i, k]), Mathf.Abs(dhdy[i, k])) > TEMP_RHO_MAX) {
					g[i, k] = 1f;
				}
			}
		}
	}

	// ***************************************************************************
	//  PRIVATE METHODS
	// ***************************************************************************
	/// <summary>
	/// Perform a center point gradient
	/// </summary>
	private void writeGradientMapData(int x, int y, int xMin, int xMax, int yMin, int yMax)
	{
		dhdx[x, y] = (h[xMax, y] - h[xMin, y]) / (xMax - xMin);
		dhdy[x, y] = (h[x, yMax] - h[x, yMin]) / (yMax - yMin);
		dh[x, y] = new Vector2(dhdx[x, y], dhdy[x, y]);
		absGradient[x, y] = Mathf.Max(dhdx[x, y], dhdy[x, y]);
	}

	/// <summary>
	/// Raycast into the scene and store data
	/// </summary>
	private Vector3[] getHeightAndNormalDataForPoint(float x, float z)
	{
		Vector3 rayPoint = new Vector3(x, TerrainHeightMax * 1.1f, z);
		Vector3 rayDir = new Vector3(0, -TerrainHeightMin, 0);

		Ray ray = new Ray(rayPoint, rayDir);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, TerrainHeightMin * 1.5f)) {
			return new Vector3[2] { hit.point, hit.normal };
		}
		return new Vector3[2] { Vector3.zero, Vector3.zero };
	}

	private float[,] normalizeMap(float[,] map)
	{
		float maxHeight = 0f;
		for (int i = 0; i < map.GetLength(0); i++) {
			for (int k = 0; k < map.GetLength(1); k++) {
				if (map[i, k] > maxHeight) { maxHeight = h[i, k]; }
			}
		}
		if (maxHeight > 0) {
			for (int i = 0; i < map.GetLength(0); i++) {
				for (int k = 0; k < map.GetLength(1); k++) {
					map[i, k] /= maxHeight;
				}
			}
		}
		return map;
	}
}
