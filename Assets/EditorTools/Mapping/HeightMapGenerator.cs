using UnityEngine;

public class HeightMapGenerator : MonoBehaviour
{
  private const float TEMP_RHO_MAX = 0.6f;

  [Header("Assign these variables")]
  public float TerrainHeightMax;
  public float TerrainHeightMin;

  public Vector2 MapSize;
  public Vector2 Center;

  public string Filename = "Test";

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

  public void SetFilename(string filename) { Filename = filename; }

  // ***************************************************************************
  //  HEIGHT MAP GENERATION
  // ***************************************************************************
  #region Height Map Generation
  /// <summary>
  /// Generate the Height Map based on the provided Map Size
  /// </summary>
  public void GenerateHeightMap()
  {
    int xSteps = (int)(MapSize.x / StepSize);
    int zSteps = (int)(MapSize.y / StepSize);

    h = new float[xSteps, zSteps];

    //float xOffset = StepSize / 2f + (Center.x - MapSize.x / 2f);
    //float zOffset = StepSize / 2f + (Center.y - MapSize.y / 2f);
    float xOffset = Center.x - MapSize.x / 2f;
    float zOffset = Center.y - MapSize.y / 2f;

    for (int i = 0; i < xSteps; i++) {
      for (int k = 0; k < zSteps; k++) {
        h[i, k] = getHeightAndNormalDataForPoint(StepSize * i + xOffset, StepSize * k + zOffset)[0].y;
      }
    }

    Debug.Log($"{this}.GenerateHeightMap() - Height map generated for map {MapSize}");

    HeightMap = TextureGenerator.TextureFromMatrix(h);
  }

  /// <summary>
  /// Generate the gradient maps from the previously generated height maps
  /// </summary>
  public void GenerateGradientMaps()
  {
    // compute center-point gradient
    dh = h.Gradient();
    absGradient = dh.AbsoluteValue();
    dhdx = dh.x();
    dhdy = dh.y();

    AbsGradientMap = TextureGenerator.TextureFromMatrix(absGradient);
    GradientMap = TextureGenerator.TextureFromMatrix(dh);
  }

  /// <summary>
  /// Generate the discomfort maps based on the gradients computed previously
  /// </summary>
  public void GenerateDiscomfortMap()
  {
    int id = h.GetLength(0);
    int kd = h.GetLength(1);

    g = new float[id, kd];

    for (int i = 0; i < id; i++) {
      for (int k = 0; k < kd; k++) {
        if (absGradient[i, k] > TEMP_RHO_MAX) {
          g[i, k] = 1f;
        }
      }
    }

    DiscomfortMap = TextureGenerator.TextureFromMatrix(g);
  }
  #endregion
  // ***************************************************************************
  //  FILE IO
  // ***************************************************************************
  public void SaveTextureImages()
  {
    string path = $"{FileUtility.PATH}/{FileUtility.DATA_FOLDER}/{Filename}/{FileUtility.IMAGE_FOLDER}/";

    FileUtility.SaveTextureAsPNG(path, Filename + "_H", HeightMap);
    FileUtility.SaveTextureAsPNG(path, Filename + "_abs_dH", AbsGradientMap);
    FileUtility.SaveTextureAsPNG(path, Filename + "_dH", GradientMap);
    FileUtility.SaveTextureAsPNG(path, Filename + "_g", DiscomfortMap);
  }

  public void SaveMapsAsCsv()
  {
    string path = $"{FileUtility.PATH}/{FileUtility.DATA_FOLDER}/{Filename}/{FileUtility.CSV_FOLDER}/";

    FileUtility.SaveMatrixAsCsv(path, Filename + "_H", h);
    FileUtility.SaveMatrixAsCsv(path, Filename + "_dHdx", dhdx);
    FileUtility.SaveMatrixAsCsv(path, Filename + "_dHdy", dhdy);
    FileUtility.SaveMatrixAsCsv(path, Filename + "_g", g);
  }

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

  // ***************************************************************************
  //  PRIVATE METHODS
  // ***************************************************************************
  /// <summary>
  /// Raycast into the scene and store data
  /// </summary>
  private Vector3[] getHeightAndNormalDataForPoint(float x, float z)
  {
    var rayPoint = new Vector3(x, TerrainHeightMax, z);
    var rayDir = new Vector3(0, -1, 0);

    var ray = new Ray(rayPoint, rayDir);

    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, (TerrainHeightMax - TerrainHeightMin) * 1.5f)) {
      return new Vector3[2] { hit.point, hit.normal };
    }
    // return an impossibly low value
    return new Vector3[2] { Vector3.one * float.MinValue, Vector3.one * float.MinValue };
  }
}
