using UnityEngine;
using System.Collections;

public class tileAndColorSystem : MonoBehaviour {

	public static tileAndColorSystem S;

	public GameObject theTile;
	tile[,] tiles;

	public int xNum, zNum;
	public float tileDim;


	void Awake() {
		S = this;
	}

	public void instantiateTiles(int x, int z, float dim) {
		xNum = x;
		zNum = z;
		tileDim = dim;

		tiles = new tile[x,z];
		GameObject tempTile;

		float xLoc, zLoc;

		for (int n=0; n<xNum; n++) {
			for (int m=0; m<zNum; m++) {
				tempTile = Instantiate(theTile) as GameObject;
				xLoc = n*tileDim; // + tileDim/2f;
				zLoc = m*tileDim; // + tileDim/2f;
				tempTile.transform.position = new Vector3(xLoc,0f,zLoc);
				tempTile.transform.SetParent(this.transform);
				tiles[n,m] = tempTile.GetComponent<tile>();
			}
		}
	}

	public void instantiateTiles(int x, int z, float dim, float[,] heightMap) {
		xNum = x;
		zNum = z;
		tileDim = dim;

		tiles = new tile[x,z];
		GameObject tempTile;

		float xLoc, zLoc;

		for (int n=0; n<xNum; n++) {
			for (int m=0; m<zNum; m++) {
				tempTile = Instantiate(theTile) as GameObject;
				xLoc = n*tileDim; //+ tileDim/2f;
				zLoc = m*tileDim; // + tileDim/2f;
				tempTile.transform.position = new Vector3(xLoc,heightMap[n,m]+0.1f,zLoc);
				tempTile.transform.SetParent(this.transform);
				tiles[n,m] = tempTile.GetComponent<tile>();
			}
		}
	}

	public void setTileColor(int xLoc, int zLoc, Color col) {
		tiles[(int)(xLoc/tileDim), (int)(zLoc/tileDim)].setColor(col);
	}
	public void addTileColor(int xLoc, int zLoc, Color col) {
		tiles[(int)(xLoc/tileDim), (int)(zLoc/tileDim)].addColor(col);
	}
	public void setTileColor(Vector2[,] colMap, Color col) {
		int N = colMap.GetLength(0);
		int M = colMap.GetLength(1);

		if (N!= xNum || M!= zNum) {
			for (int n=0; n<xNum; n++) {
				for (int m=0; m<zNum; m++) {
					tiles[n,m].setColor(Color.white);
				}
			}
		} else {	
			for (int n=0; n<xNum; n++) {
				for (int m=0; m<zNum; m++) {
					Color newC;
					newC = col * colMap[n,m].x;
					setTileColor(n,m,newC);

					float r,g,b;
					r = col.g;
					g = col.b;
					b = col.r;
					newC = (new Color(r,g,b)) * colMap[n,m].y;
					addTileColor(n,m,newC);
				}
			}
		}
	}
	public void setTileColor(float[,] colMap, Color col) {

		int N = colMap.GetLength(0);
		int M = colMap.GetLength(1);

		if (N!= xNum || M!= zNum) {
			for (int n=0; n<xNum; n++) {
				for (int m=0; m<zNum; m++) {
					tiles[n,m].setColor(Color.white);
				}
			}
		} else {	
			for (int n=0; n<xNum; n++) {
				for (int m=0; m<zNum; m++) {
					Color newC;

					if (colMap[n,m]>0) {
						newC = col * colMap[n,m];
					} else {
						float r,g,b;
						r = 1f-col.r;
						g = 1f-col.g;
						b = 1f-col.b;

						newC = new Color(r,g,b) * Mathf.Abs(colMap[n,m]);
					}
					tiles[n,m].setColor(newC);
				}
			}
		}
	}

	public void addTileColor(float[,] colMap, Color col) {

		int N = colMap.GetLength(0);
		int M = colMap.GetLength(1);

		if (N!= xNum || M!= zNum) {
			for (int n=0; n<xNum; n++) {
				for (int m=0; m<zNum; m++) {
					tiles[n,m].setColor(Color.white);
				}
			}
		} else {	
			for (int n=0; n<xNum; n++) {
				for (int m=0; m<zNum; m++) {
					Color newC;

					if (colMap[n,m]>0) {
						newC = col * colMap[n,m];
					} else {
						float r,g,b;
						r = 1f-col.r;
						g = 1f-col.g;
						b = 1f-col.b;

						newC = new Color(r,g,b) * Mathf.Abs(colMap[n,m]);
					}
					tiles[n,m].addColor(newC);
				}
			}
		}
	}
}
