using UnityEngine;
using System;

public static class Interpolations
{
  /// <summary>
  /// Interpolate the float value at non-discrete (x,y) inside the float[,] grid
  /// </summary>
  /// <param name="x">The x point to interpolate</param>
  /// <param name="y">The y point to interpolate</param>
  /// <param name="array">The array from which the interpolated value is
  /// calculated</param>
	public static float Interpolate(this float[,] array, float x, float y)
  {
    float xcomp;

    int xl = array.GetLength(0);
    int yl = array.GetLength(1);

    int topLeftX = (int)Mathf.Floor(x);
    int topLeftY = (int)Mathf.Floor(y);

    float xAmountRight = x - topLeftX;
    float xAmountLeft = 1.0f - xAmountRight;
    float yAmountBottom = y - topLeftY;
    float yAmountTop = 1.0f - yAmountBottom;

    Vector4 valuesX = Vector4.zero;

    if (isPointInsideArray(topLeftX, topLeftY, xl, yl)) {
      valuesX[0] = array[topLeftX, topLeftY];
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY, xl, yl)) {
      valuesX[1] = array[topLeftX + 1, topLeftY];
    }
    if (isPointInsideArray(topLeftX, topLeftY + 1, xl, yl)) {
      valuesX[2] = array[topLeftX, topLeftY + 1];
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY + 1, xl, yl)) {
      valuesX[3] = array[topLeftX + 1, topLeftY + 1];
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(valuesX[n])) {
        valuesX[n] = 0f;
      }
      if (float.IsInfinity(valuesX[n])) {
        valuesX[n] = 0f;
      }
    }

    float averagedXTop = valuesX[0] * xAmountLeft + valuesX[1] * xAmountRight;
    float averagedXBottom = valuesX[2] * xAmountLeft + valuesX[3] * xAmountRight;

    xcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    return xcomp;
  }

  /// <summary>
  /// Interpolate the Vector2 value at (x,y) inside the Vector2[,] grid
  /// </summary>
  /// <param name="x">The x point to interpolate</param>
  /// <param name="y">The y point to interpolate</param>
  /// <param name="array">The array from which the interpolated value is
  /// calculated</param>
  public static Vector2 Interpolate(this Vector2[,] array, float x, float y)
  {
    float xcomp, ycomp;

    int xl = array.GetLength(0);
    int yl = array.GetLength(1);

    int topLeftX = (int)Mathf.Floor(x);
    int topLeftY = (int)Mathf.Floor(y);

    float xAmountRight = x - topLeftX;
    float xAmountLeft = 1.0f - xAmountRight;
    float yAmountBottom = y - topLeftY;
    float yAmountTop = 1.0f - yAmountBottom;

    var valuesX = Vector4.zero;

    // x component of the vector 2
    if (isPointInsideArray(topLeftX, topLeftY, xl, yl)) {
      valuesX[0] = array[topLeftX, topLeftY].x;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY, xl, yl)) {
      valuesX[1] = array[topLeftX + 1, topLeftY].x;
    }
    if (isPointInsideArray(topLeftX, topLeftY + 1, xl, yl)) {
      valuesX[2] = array[topLeftX, topLeftY + 1].x;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY + 1, xl, yl)) {
      valuesX[3] = array[topLeftX + 1, topLeftY + 1].x;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(valuesX[n])) {
        valuesX[n] = 0f;
      }
      if (float.IsInfinity(valuesX[n])) {
        valuesX[n] = 0f;
      }
    }

    float averagedXTop = valuesX[0] * xAmountLeft + valuesX[1] * xAmountRight;
    float averagedXBottom = valuesX[2] * xAmountLeft + valuesX[3] * xAmountRight;

    xcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // y component of the vector 2
    Vector4 valuesY = Vector4.zero;
    if (isPointInsideArray(topLeftX, topLeftY, xl, yl)) {
      valuesY[0] = array[topLeftX, topLeftY].y;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY, xl, yl)) {
      valuesY[1] = array[topLeftX + 1, topLeftY].y;
    }
    if (isPointInsideArray(topLeftX, topLeftY + 1, xl, yl)) {
      valuesY[2] = array[topLeftX, topLeftY + 1].y;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY + 1, xl, yl)) {
      valuesY[3] = array[topLeftX + 1, topLeftY + 1].y;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(valuesY[n])) {
        valuesY[n] = 0f;
      }
      if (float.IsInfinity(valuesY[n])) {
        valuesY[n] = 0f;
      }
    }

    averagedXTop = valuesY[0] * xAmountLeft + valuesY[1] * xAmountRight;
    averagedXBottom = valuesY[2] * xAmountLeft + valuesY[3] * xAmountRight;

    ycomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    return (new Vector2(xcomp, ycomp));
  }

  /// <summary>
  /// Interpolate the Vector2 value at (x,y) inside the Vector2[,] grid
  /// </summary>
  /// <param name="x">The x point to interpolate</param>
  /// <param name="y">The y point to interpolate</param>
  /// <param name="array">The array from which the interpolated value is
  /// calculated</param>
  public static Vector4 Interpolate(this Vector4[,] array, float x, float y)
  {
    float wcomp, xcomp, ycomp, zcomp;

    int xl = array.GetLength(0);
    int yl = array.GetLength(1);

    int topLeftX = (int)Mathf.Floor(x);
    int topLeftY = (int)Mathf.Floor(y);

    float xAmountRight = x - topLeftX;
    float xAmountLeft = 1.0f - xAmountRight;
    float yAmountBottom = y - topLeftY;
    float yAmountTop = 1.0f - yAmountBottom;

    // x component of the vector 4
    var values = Vector4.zero;
    if (isPointInsideArray(topLeftX, topLeftY, xl, yl)) {
      values[0] = array[topLeftX, topLeftY].x;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY, xl, yl)) {
      values[1] = array[topLeftX + 1, topLeftY].x;
    }
    if (isPointInsideArray(topLeftX, topLeftY + 1, xl, yl)) {
      values[2] = array[topLeftX, topLeftY + 1].x;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY + 1, xl, yl)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].x;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    float averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    float averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    xcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // y component of the vector 4
    values = Vector4.zero;
    if (isPointInsideArray(topLeftX, topLeftY, xl, yl)) {
      values[0] = array[topLeftX, topLeftY].y;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY, xl, yl)) {
      values[1] = array[topLeftX + 1, topLeftY].y;
    }
    if (isPointInsideArray(topLeftX, topLeftY + 1, xl, yl)) {
      values[2] = array[topLeftX, topLeftY + 1].y;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY + 1, xl, yl)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].y;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    ycomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // z component of the vector 4
    values = Vector4.zero;
    if (isPointInsideArray(topLeftX, topLeftY, xl, yl)) {
      values[0] = array[topLeftX, topLeftY].y;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY, xl, yl)) {
      values[1] = array[topLeftX + 1, topLeftY].y;
    }
    if (isPointInsideArray(topLeftX, topLeftY + 1, xl, yl)) {
      values[2] = array[topLeftX, topLeftY + 1].y;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY + 1, xl, yl)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].y;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    zcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // w component of the vector 4
    values = Vector4.zero;
    if (isPointInsideArray(topLeftX, topLeftY, xl, yl)) {
      values[0] = array[topLeftX, topLeftY].x;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY, xl, yl)) {
      values[1] = array[topLeftX + 1, topLeftY].x;
    }
    if (isPointInsideArray(topLeftX, topLeftY + 1, xl, yl)) {
      values[2] = array[topLeftX, topLeftY + 1].x;
    }
    if (isPointInsideArray(topLeftX + 1, topLeftY + 1, xl, yl)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].x;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    wcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    return new Vector4(xcomp, ycomp, zcomp, wcomp);
  }


  /// <summary>
  /// "Splat" a value onto a 2x2 matrix at a point (x,y) where
  /// (0,0) < (x,y) < (1,1). Fractionally breaks the single value
  /// onto each of the 4 grid points based on the (x,y) location
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="scalar"></param>
  public static float[,] Linear1stOrderSplat(float x, float y, float scalar)
  {
    float[,] mat = new float[2, 2];

    mat[0, 0] = 0;
    mat[0, 1] = 0;
    mat[1, 0] = 0;
    mat[1, 1] = 0;

    int xInd = (int)Math.Floor((double)x);
    int yInd = (int)Math.Floor((double)y);

    float delx = x - xInd;
    float dely = y - yInd;

    // use += to stack density field up
    mat[0, 0] += Math.Min(1 - delx, 1 - dely);
    mat[0, 0] *= scalar;

    mat[1, 0] += Math.Min(delx, 1 - dely);
    mat[1, 0] *= scalar;

    mat[0, 1] += Math.Min(1 - delx, dely);
    mat[0, 1] *= scalar;

    mat[1, 1] += Math.Min(delx, dely);
    mat[1, 1] *= scalar;

    return mat;
  }

  public static float[,] BilinearInterpolation(this float[,] grid, Vector2 offset)
  {
    return grid.BilinearInterpolation(offset.x, offset.y);
  }
  /// <summary>
  /// Uses bilinear interpolation to shift a grid of points by [offset]
  /// </summary>
  /// <param name="grid"></param>
  /// <param name="offset"></param>
  public static float[,] BilinearInterpolation(this float[,] grid, float xOffset, float yOffset)
  {
    int xDimension = grid.GetLength(0);
    int yDimension = grid.GetLength(1);

    // create the return grid, with a +1 buffer
    float[,] interpolatedGrid = new float[xDimension + 1, yDimension + 1];

    // precompute some quantities
    // (these quantities are inverse to the actual equations because we are translating
    // from a transformed coordinate plane Back Into the original)
    float dx2 = modulus(xOffset, 1f);
    float dy2 = modulus(yOffset, 1f);
    float dx1 = 1f - dx2;
    float dy1 = 1f - dy2;

    // cache our variables
    float Q11, Q12, Q21, Q22;

    // iterate the new larger grid, interpolating at each point
    for (int x = 0; x < xDimension + 1; x++) {
      for (int y = 0; y < yDimension + 1; y++) {
        // ******** TEST EACH POINT INDIVIDUALLY
        // ******** middle (most common)
        if ((x != 0) && (y != 0) && (x != xDimension) && (y != yDimension)) {
          Q11 = grid[x - 1, y - 1];
          Q21 = grid[x, y - 1];
          Q12 = grid[x - 1, y];
          Q22 = grid[x, y];
        }
        // ******** left-edge
        else if ((x == 0) && (y != 0) && (y != yDimension)) {
          Q11 = 0;
          Q21 = grid[x, y - 1];
          Q12 = 0;
          Q22 = grid[x, y];
        }
        // ******** right-edge
        else if ((x == xDimension) && (y != 0) && (y != yDimension)) {
          Q11 = grid[x - 1, y - 1];
          Q21 = 0;
          Q12 = grid[x - 1, y];
          Q22 = 0;
        }
        // ******** bottom-edge
        else if ((y == 0) && (x != 0) && (x != xDimension)) {
          Q11 = 0;
          Q21 = 0;
          Q12 = grid[x - 1, y];
          Q22 = grid[x, y];
        }
        // ******** top-edge
        else if ((y == yDimension) && (x != 0) && (x != xDimension)) {
          Q11 = grid[x - 1, y - 1];
          Q21 = grid[x, y - 1];
          Q12 = 0;
          Q22 = 0;
        }
        // ******** lower-left corner
        else if ((x == 0) && (y == 0)) {
          Q11 = 0;
          Q21 = 0;
          Q12 = 0;
          Q22 = grid[x, y];
        }
        // ******** upper-left corner
        else if ((x == 0) && (y == yDimension)) {
          Q11 = 0;
          Q21 = grid[x, y - 1];
          Q12 = 0;
          Q22 = 0;
        }
        // ******** lower-right corner
        else if ((y == 0) && (x == xDimension)) {
          Q11 = 0;
          Q21 = 0;
          Q12 = grid[x - 1, y];
          Q22 = 0;
        }
        // ******** upper-right corner (last one)
        else {
          Q11 = grid[x - 1, y - 1];
          Q21 = 0;
          Q12 = 0;
          Q22 = 0;
        }

        // compute the interpolated value
        interpolatedGrid[x, y] = dy2 * (dx2 * Q11 + dx1 * Q21) + dy1 * (dx2 * Q12 + dx1 * Q22);
      }
    }

    return interpolatedGrid;
  }

  // ******************************************************************************
  //   TOOLS
  //******************************************************************************
  private static bool isPointInsideArray(int x, int y, int xl, int yl)
  {
    return x < 0 && x > xl - 1 && y < 0 && y > yl - 1;
  }

  private static float modulus(float x, float m)
  {
    return (x % m + m) % m;
  }
}