using UnityEngine;
public static class CCvals
{
  // scalar for density to splat onto discomfort map
  public static float rho_sc = 1f;

  // **************************************************************************************************
  // ************ deprecated due to visually unappealing 'dodging' ************************************
  //   // how far into future we predict the path
  //	 public static float g_predictiveSeconds = 4f;
  //   // scalar for predictive discomfort to splat onto discomfort map
  //	 public static float g_weight = 0.5f;
  // **************************************************************************************************
  // everything above this must be clamped to 'unpassable' discomfort map
  public static float f_slopeMax = 1f;
  public static float f_slopeMin = -1f;

  public static float f_rhoMax = 2f;
  public static float f_rhoMin = 0.6f;

  // set to some small positive number to clamp flow speed
  public static float f_speedMin = 0.1f;
  // set this to 1 to automatically just receive normalized 'direction'
  // then, we simply scale by the units particular maxSpeed
  public static float f_speedMax = 40f;

  // path length field weight
  public static float C_alpha = 0.5f;
  // time weight (inverse of speed)
  public static float C_beta = 0.3f;
  // discomfort weight
  public static float C_gamma = 0.8f;
  // density weight
  public static float C_delta = 1.5f;

  // this array of Vect2's correlates to our data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
  public static Vector2[] ENSW = new Vector2[] { 
    Vector2.right, 
    Vector2.up,
    Vector2.left, 
    Vector2.down 
  };
  public static Vector2Int[] ENSWint = new Vector2Int[] { 
    Vector2Int.right, 
    Vector2Int.up, 
    Vector2Int.left, 
    Vector2Int.down 
  };
}
