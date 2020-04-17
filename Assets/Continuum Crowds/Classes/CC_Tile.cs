using UnityEngine;
using System.Collections;

/// <summary>
/// The CC_Tile struct is specficially built to hold
/// the field values for Continuum Crowd that are
/// common amongst all units.
/// </summary>
[System.Serializable]
public class CC_Tile
{
  // tile dimension (all tiles are square)
  private int dim;

  // testing this -- only update tiles with
  // units moving around in them
  public bool UPDATE_TILE;

  // might need to store this
  public Location myLoc;

  // the Continuum Crowds Dynamic Global fields
  // density field
  public float[,] rho;
  // average velocity field
  public Vector2[,] vAve;
  // discomfort
  public float[,] g;
  // speed field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
  public Vector4[,] f;
  private Vector4[,] _fbackup;
  // cost field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
  public Vector4[,] C;
  private Vector4[,] _Cbackup;

  /// <summary>
  /// Basic Constructor takes a integer dimension, and a Location
  /// </summary>
  /// <param name="d"></param>
  /// <param name="l"></param>
  public CC_Tile(int d, Location l)
  {
    dim = d;
    myLoc = l;

    rho = new float[dim, dim];
    g = new float[dim, dim];
    vAve = new Vector2[dim, dim];
    f = new Vector4[dim, dim];
    C = new Vector4[dim, dim];

    _fbackup = new Vector4[dim, dim];
    _Cbackup = new Vector4[dim, dim];

    float f0 = CCvals.f_speedMax + (-CCvals.f_slopeMin) / (CCvals.f_slopeMax - CCvals.f_slopeMin) * (CCvals.f_speedMin - CCvals.f_speedMax);
    // initialize speed and cost fields
    Vector4 f_init = f0 * Vector4.one;
    Vector4 C_init = Vector4.one * (f0 * CCvals.C_alpha + CCvals.C_beta) / f0;

    for (int i = 0; i < dim; i++) {
      for (int k = 0; k < dim; k++) {
        f[i, k] = f_init;
        C[i, k] = C_init;

        _fbackup[i, k] = f_init;
        _Cbackup[i, k] = C_init;
      }
    }
    UPDATE_TILE = false;
  }

  /// <summary>
  /// Set the current Speed field and Cost fields as defaults.
  /// These defaults will be loaded when tiles reset.
  /// </summary>
  public void StoreCurrentSpeedAndCostFields()
  {
    for (int i = 0; i < dim; i++) {
      for (int k = 0; k < dim; k++) {
        _fbackup[i, k] = f[i, k];
        _Cbackup[i, k] = C[i, k];
      }
    }
  }

  /// <summary>
  /// Reset all the tile values to 0.
  /// Speed and Cost fields are reset to the stored defaults.
  /// </summary>
  public void ResetTile()
  {
    for (int i = 0; i < dim; i++) {
      for (int k = 0; k < dim; k++) {
        rho[i, k] = 0;
        g[i, k] = 0;
        vAve[i, k] = Vector2.zero;
        f[i, k] = _fbackup[i, k];
        C[i, k] = _Cbackup[i, k];
      }
    }
    UPDATE_TILE = false;
  }

  // **************************************************************
  //  		WRITE data
  // (I tried making a generic function that would read/write to the
  // proper matrix based on the input Type, but it was hard, and I
  // wasnt seeing success. Thus, this 'brute force' approach)
  // **************************************************************
  public void writeData_rho(int xTile, int yTile, float f)
  {
    rho[xTile, yTile] = f;
    UPDATE_TILE = true;
  }
  public void writeData_g(int xTile, int yTile, float f)
  {
    g[xTile, yTile] = f;
    UPDATE_TILE = true;
  }
  public void writeData_vAve(int xTile, int yTile, Vector2 f)
  {
    vAve[xTile, yTile] = f;
  }
  public void writeData_f(int xTile, int yTile, Vector4 v)
  {
    f[xTile, yTile] = v;
  }
  public void writeData_C(int xTile, int yTile, Vector4 v)
  {
    C[xTile, yTile] = v;
  }

  // **************************************************************
  //  		READ data
  // **************************************************************
  public float readData_rho(int xTile, int yTile)
  {
    return rho[xTile, yTile];
  }
  public float readData_g(int xTile, int yTile)
  {
    return g[xTile, yTile];
  }
  public Vector2 readData_vAve(int xTile, int yTile)
  {
    return vAve[xTile, yTile];
  }
  public Vector4 readData_f(int xTile, int yTile)
  {
    return f[xTile, yTile];
  }
  public Vector4 readData_C(int xTile, int yTile)
  {
    return C[xTile, yTile];
  }
}
