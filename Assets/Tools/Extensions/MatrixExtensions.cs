using UnityEngine;
using System;

public static class MatrixExtensions
{
  public static float[,] SubMatrix(this float[,] matrix, int startX, int startY, int sizeX, int sizeY)
  {
    var sub = new float[sizeX, sizeY];

    for (int x = 0; x < sizeX; x++) {
      for (int y = 0; y < sizeY; y++) {
        sub[x, y] = matrix[startX + x, startY + y];
      }
    }

    return sub;
  }

  public static Vector2[,] SubMatrix(this Vector2[,] matrix, int startX, int startY, int sizeX, int sizeY)
  {
    var sub = new Vector2[sizeX, sizeY];

    for (int x = 0; x < sizeX; x++) {
      for (int y = 0; y < sizeY; y++) {
        sub[x, y] = matrix[startX + x, startY + y];
      }
    }

    return sub;
  }

  public static Vector2[,] Normalize(this Vector2[,] matrix)
  {
    float max = 0;
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        if (matrix[i, k].sqrMagnitude > max) { max = matrix[i, k].sqrMagnitude; }
      }
    }

    if (max == 0) { return matrix; }
    max = Mathf.Sqrt(max);

    var normalized = new Vector2[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        normalized[i, k] = matrix[i, k] / max;
      }
    }

    return normalized;
  }

  /// <summary>
  /// Normalize a matrix of floats to 1 by the largest absolute value
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] Normalize(this float[,] matrix)
  {
    float max = 0f;
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        if (Mathf.Abs(matrix[i, k]) > max) { max = Mathf.Abs(matrix[i, k]); }
      }
    }

    var normalized = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        normalized[i, k] = Mathf.Abs(matrix[i, k]) / max;
      }
    }

    return normalized;
  }

  /// <summary>
  /// Compute the matrix of absolute values from a vector matrix
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] AbsoluteValue(this Vector2[,] matrix)
  {
    var abs = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        abs[i, k] = Mathf.Max(Mathf.Abs(matrix[i, k].x), Mathf.Abs(matrix[i, k].y));
      }
    }
    return abs;
  }

  /// <summary>
  /// Perform a center-point gradient
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static Vector2[,] Gradient(this float[,] matrix)
  {
    int id = matrix.GetLength(0);
    int kd = matrix.GetLength(1);

    var gradient = new Vector2[id, kd];

    void computeGradient(int x, int y, int xMin, int xMax, int yMin, int yMax)
    {
      var xGrad = (matrix[xMax, y] - matrix[xMin, y]) / (xMax - xMin);
      var yGrad = (matrix[x, yMax] - matrix[x, yMin]) / (yMax - yMin);
      gradient[x, y] = new Vector2(xGrad, yGrad);
    }

    for (int i = 0; i < id; i++) {
      for (int k = 0; k < kd; k++) {
        if ((i != 0) && (i != id - 1) && (k != 0) && (k != kd - 1)) {
          // generic spot
          computeGradient(i, k, i - 1, i + 1, k - 1, k + 1);
        } else if ((i == 0) && (k == kd - 1)) {
          // upper left corner
          computeGradient(i, k, i, i + 1, k - 1, k);
        } else if ((i == id - 1) && (k == 0)) {
          // bottom left corner
          computeGradient(i, k, i - 1, i, k, k + 1);
        } else if ((i == 0) && (k == 0)) {
          // upper left corner
          computeGradient(i, k, i, i + 1, k, k + 1);
        } else if ((i == id - 1) && (k == kd - 1)) {
          // bottom right corner
          computeGradient(i, k, i - 1, i, k - 1, k);
        } else if (i == 0) {
          // top edge
          computeGradient(i, k, i, i + 1, k - 1, k + 1);
        } else if (i == id - 1) {
          // bot edge
          computeGradient(i, k, i - 1, i, k - 1, k + 1);
        } else if (k == 0) {
          // left edge
          computeGradient(i, k, i - 1, i + 1, k, k + 1);
        } else if (k == kd - 1) {
          // right edge
          computeGradient(i, k, i - 1, i + 1, k - 1, k);
        }
      }
    }
    return gradient;
  }

  /// <summary>
  /// Strip the x-dimention from a matrix of vectors
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] x(this Vector2[,] matrix)
  {
    var x = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        x[i, k] = matrix[i, k].x;
      }
    }
    return x;
  }

  /// <summary>
  /// Strip the y-dimension from a matrix of vectors
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] y(this Vector2[,] matrix)
  {
    var y = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        y[i, k] = matrix[i, k].y;
      }
    }
    return y;
  }

  public static string ToString<T>(this T[,] matrix, string format = "")
  {
    string s = "";
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        s += matrix[i, k].ToString() + ",\t";
      }
      s += "\n";
    }
    return s;
  }

  /// <summary>
  /// Rotates a matrix counter-clockwise by given degrees
  /// </summary>
  /// <param name="matrix"></param>
  /// <param name="degrees"></param>
  public static float[,] Rotate(this float[,] matrix, float degrees)
  {
    float radians = degrees * Mathf.Deg2Rad;
    // store constants
    var n = matrix.GetLength(0);
    var m = matrix.GetLength(1);
    // determine the components that sum the side dimensions of our new matrix
    var na = Mathf.Abs(n * Mathf.Cos(radians));
    var nb = Mathf.Abs(m * Mathf.Sin(radians));
    var ma = Mathf.Abs(m * Mathf.Cos(radians));
    var mb = Mathf.Abs(n * Mathf.Sin(radians));

    // compute new dimensions of the matrix required to hold the rotated matrix
    var N = Mathf.Ceil(na + nb);
    var M = Mathf.Ceil(ma + mb);

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(float x, float y) { return x < 0 || y < 0 || x > n - 1 || y > m - 1; }

    // build a helper function to transform a matrix point
    float invTransform(float[,] mtx, int u, int v)
    {
      // (1) translate to midpoint
      var up = u - (N - 1) / 2f;
      var vp = v - (M - 1) / 2f;
      // (2) apply rotation matrix
      var ur = up * Mathf.Cos(radians) + vp * Mathf.Sin(radians);
      var vr = -up * Mathf.Sin(radians) + vp * Mathf.Cos(radians);
      // (3) re-scale to original frame of reference
      var x = ur + (n - 1) / 2f;
      var y = vr + (m - 1) / 2f;
      // (4) round our rescaled parameter to round integers
      var xf = Mathf.FloorToInt(x);
      var xc = Mathf.CeilToInt(x);
      var yf = Mathf.FloorToInt(y);
      var yc = Mathf.CeilToInt(y);
      // (5) build a 2x2 matrix of these 4 corners, using 0 if we're outisde the original matrix
      mtx[0, 0] = outside(xf, yf) ? 0 : matrix[xf, yf];
      mtx[0, 1] = outside(xf, yc) ? 0 : matrix[xf, yc];
      mtx[1, 0] = outside(xc, yf) ? 0 : matrix[xc, yf];
      mtx[1, 1] = outside(xc, yc) ? 0 : matrix[xc, yc];
      // (5) interpolate
      return mtx.Interpolate(x.Modulus(1), y.Modulus(1));
    }

    // declare new rotated matrix with previously determined dimensions
    var rotated = new float[(int)N, (int)M];
    // cache
    float[,] c = new float[2, 2];

    // build the rotated matrix
    for (int x = 0; x < N; x++) {
      for (int y = 0; y < M; y++) {
        rotated[x,y] = invTransform(c, x, y);
      }
    }

    return rotated;
  }
}
