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
				if (Mathf.Abs(matrix[i, k]) > max) { max = Mathf.Abs(matrix[i, k]); }
			}
		}

		float[,] normalized = new float[matrix.GetLength(0), matrix.GetLength(1)];
		for (int i = 0; i < matrix.GetLength(0); i++) {
			for (int k = 0; k < matrix.GetLength(1); k++) {
				normalized[i, k] = Mathf.Abs(matrix[i, k]) / max;
			}
		}

		return normalized;
	}

	private static Vector2[,] normalize(Vector2[,] matrix)
	{
		float xMax = 0f;
		float yMax = 0f;
		for (int i = 0; i < matrix.GetLength(0); i++) {
			for (int k = 0; k < matrix.GetLength(1); k++) {
				if (Mathf.Abs(matrix[i, k].x) > xMax) { xMax = Mathf.Abs(matrix[i, k].x); }
				if (Mathf.Abs(matrix[i, k].y) > yMax) { yMax = Mathf.Abs(matrix[i, k].y); }
			}
		}

		Vector2[,] normalized = new Vector2[matrix.GetLength(0), matrix.GetLength(1)];
		for (int i = 0; i < matrix.GetLength(0); i++) {
			for (int k = 0; k < matrix.GetLength(1); k++) {
				normalized[i, k].x = Mathf.Abs(matrix[i, k].x) / xMax;
				normalized[i, k].y = Mathf.Abs(matrix[i, k].y) / yMax;
			}
		}

		return normalized;
	}
}
