using UnityEngine;

public static class TextureGenerator
{
	public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
	{
		Texture2D texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}

	public static Texture2D TextureFromMatrix(float[,] matrix)
	{
		float[,] normalized = normalize(matrix);

		int width = matrix.GetLength(0);
		int height = matrix.GetLength(1);

		// Create a map with all the pixels colors predefined (faster than applying each pixel one-by-one)
		Color[] colorMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				// Assign the pixel a color based on its value
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, normalized[x, y]);
			}
		}
		return TextureFromColorMap(colorMap, width, height);
	}

	public static Texture2D TextureFromMatrix(Vector2[,] matrix)
	{
		Vector2[,] normalized = normalize(matrix);

		int width = matrix.GetLength(0);
		int height = matrix.GetLength(1);

		// Create a map with all the pixels colors predefined (faster than applying each pixel one-by-one)
		Color[] colorMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				// Assign the pixel a color based on its value
				float red = normalized[x, y].x;
				float blu = normalized[x, y].y;
				colorMap[y * width + x] = new Color(red, 0, blu);
			}
		}
		return TextureFromColorMap(colorMap, width, height);
	}

	private static float[,] normalize(float[,] matrix)
	{
		float max = 0f;
		for (int i = 0; i < matrix.GetLength(0); i++) {
			for (int k = 0; k < matrix.GetLength(1); k++) {
				if (matrix[i, k] > max) { max = matrix[i, k]; }
			}
		}
		if (max > 0) {
			for (int i = 0; i < matrix.GetLength(0); i++) {
				for (int k = 0; k < matrix.GetLength(1); k++) {
					matrix[i, k] /= max;
				}
			}
		}
		return matrix;
	}

	private static Vector2[,] normalize(Vector2[,] matrix)
	{
		float xMax = 0f;
		float yMax = 0f;
		for (int i = 0; i < matrix.GetLength(0); i++) {
			for (int k = 0; k < matrix.GetLength(1); k++) {
				if (matrix[i, k].x > xMax) { xMax = matrix[i, k].x; }
				if (matrix[i, k].y > yMax) { yMax = matrix[i, k].y; }
			}
		}
		if (xMax > 0) {
			for (int i = 0; i < matrix.GetLength(0); i++) {
				for (int k = 0; k < matrix.GetLength(1); k++) {
					matrix[i, k].x /= xMax;
					matrix[i, k].y /= yMax;
				}
			}
		}
		return matrix;
	}
}
