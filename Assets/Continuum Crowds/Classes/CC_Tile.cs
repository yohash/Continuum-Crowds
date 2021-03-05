using UnityEngine;

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
  public Location Corner;

  // the Continuum Crowds Dynamic Global fields
  // density field
  public float[,] rho;
  // average velocity field
  public Vector2[,] vAve;
  // height map
  public float[,] h;
  // height map gradient
  public Vector2[,] dh;
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
  /// <param name="dimension"></param>
  /// <param name="corner"></param>
  public CC_Tile(MapTile tile)
  {
    dim = tile.TileSize;
    Corner = tile.Corner;

    rho = new float[dim, dim];
    g = tile.g;
    vAve = new Vector2[dim, dim];
    f = new Vector4[dim, dim];
    C = new Vector4[dim, dim];
    h = tile.h;
    dh = tile.dh;

    _fbackup = new Vector4[dim, dim];
    _Cbackup = new Vector4[dim, dim];

    float f0 = CCValues.S.f_speedMax
      + (-CCValues.S.f_slopeMin) / (CCValues.S.f_slopeMax - CCValues.S.f_slopeMin)
      * (CCValues.S.f_speedMin - CCValues.S.f_speedMax);

    // initialize speed and cost fields
    Vector4 f_init = f0 * Vector4.one;
    Vector4 C_init = Vector4.one * (f0 * CCValues.S.C_alpha + CCValues.S.C_beta) / f0;

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
  // **************************************************************
  public void writeData_rho(Location l, float f)
  {
    rho[l.x, l.y] = f;
    UPDATE_TILE = true;
  }
  public void writeData_g(Location l, float f)
  {
    g[l.x, l.y] = f;
    UPDATE_TILE = true;
  }
  public void writeData_vAve(Location l, Vector2 f)
  {
    vAve[l.x, l.y] = f;
  }
  public void writeData_f(Location l, Vector4 v)
  {
    f[l.x, l.y] = v;
  }
  public void writeData_C(Location l, Vector4 v)
  {
    C[l.x, l.y] = v;
  }
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
  //  		READ data - discrete
  // **************************************************************
  public float readData_rho(Location l)
  {
    return rho[l.x, l.y];
  }
  public float readData_g(Location l)
  {
    return g[l.x, l.y];
  }
  public float readData_h(Location l)
  {
    return h[l.x, l.y];
  }
  public Vector2 readData_dh(Location l)
  {
    return dh[l.x, l.y];
  }
  public Vector2 readData_vAve(Location l)
  {
    return vAve[l.x, l.y];
  }
  public Vector4 readData_f(Location l)
  {
    return f[l.x, l.y];
  }
  public Vector4 readData_C(Location l)
  {
    return C[l.x, l.y];
  }
  public float readData_rho(int xTile, int yTile)
  {
    return rho[xTile, yTile];
  }
  public float readData_g(int xTile, int yTile)
  {
    return g[xTile, yTile];
  }
  public float readData_h(int xTile, int yTile)
  {
    return h[xTile, yTile];
  }
  public Vector2 readData_dh(int xTile, int yTile)
  {
    return dh[xTile, yTile];
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

  // **************************************************************
  //  		READ data - fuzzy
  // **************************************************************
  public float readData_h(float xTile, float yTile)
  {
    return h.Interpolate(xTile, yTile);
  }
  public Vector2 readData_dh(float xTile, float yTile)
  {
    return dh.Interpolate(xTile, yTile);
  }
  public float readData_rho(float xTile, float yTile)
  {
    return rho.Interpolate(xTile, yTile);
  }
  public float readData_g(float xTile, float yTile)
  {
    return g.Interpolate(xTile, yTile);
  }
  public Vector2 readData_vAve(float xTile, float yTile)
  {
    return vAve.Interpolate(xTile, yTile);
  }
  public Vector4 readData_f(float xTile, float yTile)
  {
    return f.Interpolate(xTile, yTile);
  }
  public Vector4 readData_C(float xTile, float yTile)
  {
    return C.Interpolate(xTile, yTile);
  }
}
