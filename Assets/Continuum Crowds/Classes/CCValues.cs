using UnityEngine;

[CreateAssetMenu(menuName = "New Continuum Crowds Constants", fileName = "CCConstants")]
public class CcValues : ScriptableObject
{
  static private CcValues instance;
  static public CcValues S {
    get {
      return instance ??
        (instance = Resources.Load("CCConstants") as CcValues);
    }
  }

  // how far into future we predict the path due to unit with velocity
  [Header("Number of seconds to extrapolate unit's velocity")]
  public float v_predictiveSeconds = 4f;

  // everything above this must be clamped to 'unpassable' discomfort map
  [Header("Max and Min slopes to scale topographical speed")]
  public float f_slopeMax = 1f;
  public float f_slopeMin = -1f;

  [Header("Max and min densities to scale flow speed")]
  public float f_rhoMax = 2f;
  public float f_rhoMin = 0.6f;

  [Header("Max and min speed field")]
  // set to some small positive number to clamp flow speed
  public float f_speedMin = 0f;
  // set this to 1 to automatically just receive normalized 'direction'
  // then, we simply scale by the units particular maxSpeed
  public float f_speedMax = 40f;

  // path length field weight
  [Header("Weights: Path Length")]
  public float C_alpha = 1f;
  // time weight (inverse of speed)
  [Header("Weights: Time (speed field inverse)")]
  public float C_beta = 1f;
  // discomfort weight
  [Header("Weights: Discomfort")]
  public float C_gamma = 1f;

  [Header("Weighted average for Eikonal solutions")]
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
