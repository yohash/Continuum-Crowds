using System.Threading.Tasks;
using UnityEngine;

public class HeightMapGenerator : MonoBehaviour
{
	private const float TEMP_RHO_MAX = 0.6f;

	private const string ROOT_PATH = "/_Data/";

	[Header("Assign these variables")]
	public float TerrainHeightMax;
	public float TerrainHeightMin;

	public Vector2 MapSize;
	public Vector2 Center;

	public string Filename;

	public float StepSize = 1;

	[Header("Texture Map")]
	public Texture2D HeightMap;
	public Texture2D AbsGradientMap;
	public Texture2D GradientMap;
	public Texture2D DiscomfortMap;

	// private local vars
	private float[,] h;
	private float[,] absGradient;

	private float[,] dhdx;
	private float[,] dhdy;

	private float[,] g;

	private Vector2[,] dh;

	// ***************************************************************************
	//  HEIGHT MAP GENERATION
	// ***************************************************************************
	/// <summary>
	/// Generate the Height Map based on the provided Map Size
	/// </summary>
	public void GenerateHeightMap()
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

		HeightMap = TextureGenerator.TextureFromMatrix(h);
	}

	/// <summary>
	/// Generate the gradient maps from the previously generated height maps
	/// </summary>
	public void GenerateGradientMaps()
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
					// right edge
					writeGradientMapData(i, k, i - 1, i + 1, k - 1, k);
				}
			}
		}

		AbsGradientMap = TextureGenerator.TextureFromMatrix(absGradient);
		GradientMap = TextureGenerator.TextureFromMatrix(dh);
	}

	/// <summary>
	/// Generate the discomfort maps based on the gradients computed previously
	/// </summary>
	public void GenerateDiscomfortMap()
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

		DiscomfortMap = TextureGenerator.TextureFromMatrix(g);
	}

	public void SaveTextures()
	{
		// Application.persistentDataPath = "C:/Users/<name>/AppData/LocalLow/<company>/<project>"
		// Application.dataPath = "<Project Path>/Assets"
		string path = Application.dataPath + ROOT_PATH;

		FileUtility.SaveTextureAsPNG(path, Filename + "_H", HeightMap);
		FileUtility.SaveTextureAsPNG(path, Filename + "_abs_dH", AbsGradientMap);
		FileUtility.SaveTextureAsPNG(path, Filename + "_dH", GradientMap);
		FileUtility.SaveTextureAsPNG(path, Filename + "_g", DiscomfortMap);
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
		absGradient[x, y] = Mathf.Max(Mathf.Abs(dhdx[x, y]), Mathf.Abs(dhdy[x, y]));
	}

	/// <summary>
	/// Raycast into the scene and store data
	/// </summary>
	private Vector3[] getHeightAndNormalDataForPoint(float x, float z)
	{
		Vector3 rayPoint = new Vector3(x, TerrainHeightMax, z);
		Vector3 rayDir = new Vector3(0, -1, 0);

		Ray ray = new Ray(rayPoint, rayDir);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, (TerrainHeightMax - TerrainHeightMin) * 1.5f)) {
			return new Vector3[2] { hit.point, hit.normal };
		}
		return new Vector3[2] { Vector3.zero, Vector3.zero };
	}
}
