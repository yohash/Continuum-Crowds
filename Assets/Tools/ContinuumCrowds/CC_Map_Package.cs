using UnityEngine;
using System.Collections;

/// <summary>
/// The Continuum Crowds Map Package is a container struct for the
/// discomfort (g), speed (F), and cost (C) fields in a given
/// Continuum Crowds solution.
/// </summary>
[System.Serializable]
public struct CC_Map_Package
{
  public float[,] g;
  public Vector4[,] f;
  public Vector4[,] C;

  public CC_Map_Package (float[,] _g, Vector4[,] _f, Vector4[,] _C)
  {
    this.g = _g;
    this.f = _f;
    this.C = _C;
  }
}