using UnityEngine;

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
    float xMax = 0f;
    float yMax = 0f;
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        if (Mathf.Abs(matrix[i, k].x) > xMax) { xMax = Mathf.Abs(matrix[i, k].x); }
        if (Mathf.Abs(matrix[i, k].y) > yMax) { yMax = Mathf.Abs(matrix[i, k].y); }
      }
    }

    var normalized = new Vector2[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        normalized[i, k].x = Mathf.Abs(matrix[i, k].x) / xMax;
        normalized[i, k].y = Mathf.Abs(matrix[i, k].y) / yMax;
      }
    }

    return normalized;
  }

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
}
