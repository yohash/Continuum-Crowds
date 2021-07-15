using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CcTile
{
  public List<CcUnit> AffectingUnits;

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
  public CcTile(float[,] g, float[,] h, Vector2[,] dh)
  {
    this.g = g;
    this.h = h;
    this.dh = dh;

    int x = g.GetLength(0);
    int y = g.GetLength(1);

    rho = new float[x, y];
    vAve = new Vector2[x, y];
    f = new Vector4[x, y];
    C = new Vector4[x, y];

    _fbackup = new Vector4[x, y];
    _Cbackup = new Vector4[x, y];

    float f0 = CcValues.S.FlatSpeed;

    // initialize speed and cost fields
    Vector4 f_init = f0 * Vector4.one;
    Vector4 C_init = Vector4.one * (f0 * CcValues.S.C_alpha + CcValues.S.C_beta) / f0;

    for (int i = 0; i < x; i++) {
      for (int k = 0; k < y; k++) {
        f[i, k] = f_init;
        C[i, k] = C_init;

        _fbackup[i, k] = f_init;
        _Cbackup[i, k] = C_init;
      }
    }
  }
}
