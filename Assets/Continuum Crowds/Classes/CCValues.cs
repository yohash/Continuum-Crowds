using UnityEngine;

[CreateAssetMenu(menuName = "New Continuum Crowds Constants", fileName = "CCConstants")]
public class CCValues : ScriptableObject
{
  static private CCValues instance;
  static public CCValues Instance {
    get {
      return instance ??
        (instance = Resources.Load("CCConstants") as CCValues);
    }
  }
  // scalar for density to splat onto discomfort map
  public float rho_sc = 1f;

  // **************************************************************************************************
  // ************ deprecated due to visually unappealing 'dodging' ************************************
  //   // how far into future we predict the path
  //	 public static float g_predictiveSeconds = 4f;
  //   // scalar for predictive discomfort to splat onto discomfort map
  //	 public static float g_weight = 0.5f;
  // **************************************************************************************************
  // everything above this must be clamped to 'unpassable' discomfort map
  public float f_slopeMax = 1f;
  public float f_slopeMin = -1f;

  public float f_rhoMax = 2f;
  public float f_rhoMin = 0.6f;

  // set to some small positive number to clamp flow speed
  public float f_speedMin = 0f;
  // set this to 1 to automatically just receive normalized 'direction'
  // then, we simply scale by the units particular maxSpeed
  public float f_speedMax = 40f;

  // path length field weight
  public float C_alpha = 1f;
  // time weight (inverse of speed)
  public float C_beta = 1f;
  // discomfort weight
  public float C_gamma = 1f;
  // density weight
  public float C_delta = 1f;

  // Eikonal solver weighted average, max weight
  public float maxWeight = 2.5f;
  // Eikonal solver weighted average, min weight
  public float minWeight = 1f;


  // this array of Vect2's correlates to our data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
  public Vector2[] ENSW = new Vector2[] {
    Vector2.right,
    Vector2.up,
    Vector2.left,
    Vector2.down
  };
  public Vector2Int[] ENSWint = new Vector2Int[] {
    Vector2Int.right,
    Vector2Int.up,
    Vector2Int.left,
    Vector2Int.down
  };
}
