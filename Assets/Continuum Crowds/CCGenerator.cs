using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCGenerator : MonoBehaviour
{
	public string Filename;
	public void SetFilename(string filename) { Filename = filename; }

	// terrain fields
	private float[,] h;
	private float[,] absGradient;

	private float[,] dhdx;
	private float[,] dhdy;

	private float[,] g;

	private Vector2[,] dh;

	public void GenerateCCTiles()
	{
		Debug.Log("Generate tiles");
	}

	// ***************************************************************************
	//  FILE IO
	// ***************************************************************************
	public void LoadCsvFiles()
	{
		string path = $"{FileUtility.PATH}/{FileUtility.DATA_FOLDER}/{Filename}/{FileUtility.CSV_FOLDER}/";

		h = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_H.txt");
		g = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_g.txt");
		dhdx = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_dHdx.txt");
		dhdy = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_dHdy.txt");
		// manually populate matrix dh
		dh = new Vector2[dhdx.GetLength(0), dhdx.GetLength(1)];
		for (int i = 0; i < dhdx.GetLength(0); i++) {
			for (int k = 0; k < dhdx.GetLength(1); k++) {
				dh[i, k] = new Vector2(dhdx[i, k], dhdy[i, k]);
			}
		}
	}
}
