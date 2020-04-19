using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
	public string Filename = "Test";
	public void SetFilename(string filename) { Filename = filename; }

	public int TileSize = 25;

	public List<MapTile> Tiles;

	// terrain fields
	private float[,] h;
	private float[,] g;
	private Vector2[,] dh;

	// ***************************************************************************
	//  TILE GENERATION
	// ***************************************************************************
	public void GenerateCCTiles()
	{
		int sizeX = h.GetLength(0);
		int sizeY = h.GetLength(1);

		if (h.GetLength(0) != g.GetLength(0) || g.GetLength(0) != dh.GetLength(0) ||
				h.GetLength(1) != g.GetLength(1) || g.GetLength(1) != dh.GetLength(1)) {
			Debug.LogError("Cannot generate Tiles, dimensions disagree. Re-generate heightmaps.");
			return;
		}

		int numTilesX = Mathf.CeilToInt((float)sizeX / (float)TileSize);
		int numTilesY = Mathf.CeilToInt((float)sizeY / (float)TileSize);

		Tiles = new List<MapTile>();
		for (int x = 0; x < numTilesX; x++) {
			for (int y = 0; y < numTilesY; y++) {
				Vector2 corner = new Vector2(x * TileSize, y * TileSize);
				Tiles.Add(new MapTile(
							corner,
							h.SubMatrix(x * TileSize, y * TileSize, TileSize, TileSize),
							g.SubMatrix(x * TileSize, y * TileSize, TileSize, TileSize),
							dh.SubMatrix(x * TileSize, y * TileSize, TileSize, TileSize)
				));
			}
		}
	}

	// ***************************************************************************
	//  FILE IO
	// ***************************************************************************
	public void LoadCsvFiles()
	{
		string path = $"{FileUtility.PATH}/{FileUtility.DATA_FOLDER}/{Filename}/{FileUtility.CSV_FOLDER}/";

		h = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_H.txt");
		g = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_g.txt");
		
		float[,] dhdx = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_dHdx.txt");
		float[,] dhdy = FileUtility.LoadCsvIntoFloatMatrix(path + Filename + "_dHdy.txt");

		// populate matrix dh
		dh = new Vector2[dhdx.GetLength(0), dhdx.GetLength(1)];
		for (int i = 0; i < dhdx.GetLength(0); i++) {
			for (int k = 0; k < dhdx.GetLength(1); k++) {
				dh[i, k] = new Vector2(dhdx[i, k], dhdy[i, k]);
			}
		}
	}

	public void SerializeAndSaveTiles()
	{
		/// TBD
	}
}
