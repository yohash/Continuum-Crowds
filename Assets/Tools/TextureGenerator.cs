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
		return TextureFromMatrix(matrix, Color.black, Color.white);
	}

	public static Texture2D TextureFromMatrix(float[,] matrix, Color min, Color max)
	{
		var normalized = matrix.Normalize(1);

		int width = matrix.GetLength(0);
		int height = matrix.GetLength(1);

		// Create a map with all the pixels colors predefined (faster than applying each pixel one-by-one)
		var colorMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				// Assign the pixel a color based on its value
				colorMap[y * width + x] = Color.Lerp(min, max, normalized[x, y]);
			}
		}
		return TextureFromColorMap(colorMap, width, height);
	}

	public static Texture2D TextureFromMatrix(Vector2[,] matrix)
	{
		var normalized = matrix.Normalize();

		int width = matrix.GetLength(0);
		int height = matrix.GetLength(1);

		// Create a map with all the pixels colors predefined (faster than applying each pixel one-by-one)
		var colorMap = new Color[width * height];
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
}
