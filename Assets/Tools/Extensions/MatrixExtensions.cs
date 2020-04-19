using UnityEngine;

public static class MatrixExtensions
{
	public static float[,] SubMatrix(this float[,] matrix, int startX, int startY, int sizeX, int sizeY)
	{
		float[,] newMat = new float[sizeX, sizeY];

		for (int x = 0; x < sizeX; x++) {
			for (int y = 0; y < sizeY; y++) {
				newMat[x, y] = matrix[startX + x, startY + y];
			}
		}

		return newMat;
	}
	public static Vector2[,] SubMatrix(this Vector2[,] matrix, int startX, int startY, int sizeX, int sizeY)
	{
		Vector2[,] newMat = new Vector2[sizeX, sizeY];

		for (int x = 0; x < sizeX; x++) {
			for (int y = 0; y < sizeY; y++) {
				newMat[x, y] = matrix[startX + x, startY + y];
			}
		}

		return newMat;
	}
}
