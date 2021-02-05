using System;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

public static class FileUtility
{
  public static string PATH {
    get {
      // Application.persistentDataPath = "C:/Users/<name>/AppData/LocalLow/<company>/<project>"
      //return Application.persistentDataPath;
      // Application.dataPath = "<Project Path>/Assets"
      return Application.dataPath;
    }
  }
  public static string DATA_FOLDER = "_Data";
  public static string IMAGE_FOLDER = "Image Maps";
  public static string CSV_FOLDER = "CSV";

  public static async Task SaveTextureAsPNG(string path, string filename, Texture2D texture)
  {
    createDirectory(path);
    string completePath = path + filename + ".png";

    Debug.Log("FileUtility.SaveTextureAsPNG - saving: " + completePath);
    using (var save = new FileStream(completePath, FileMode.Create)) {
      // save to the filestream
      byte[] data = ImageConversion.EncodeToPNG(texture);
      await save.WriteAsync(data, 0, data.Length);
    }
    Debug.Log("\tcompleted saving: " + completePath);
  }

  public static void SaveMatrixAsCsv(string path, string filename, float[,] matrix)
  {
    createDirectory(path);
    string completePath = path + filename + ".txt";

    int width = matrix.GetLength(0);
    int height = matrix.GetLength(1);

    // assemble string
    string file = "";
    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        file = string.Concat(
          file,
          matrix[x, y],
          x == width - 1 ? "" : ", "
        );
      }
      file = string.Concat(file, "\n");
    }

    Debug.Log("FileUtility.SaveMatrixAsCsv - saving: " + completePath);
    saveString(completePath, file);
    Debug.Log("\tcompleted saving: " + completePath);
  }

  public static void SaveMatrixAsCsv(string path, string filename, Vector2[,] matrix)
  {
    createDirectory(path);
    string completePath = path + filename + ".txt";

    int width = matrix.GetLength(0);
    int height = matrix.GetLength(1);

    // assemble string
    string file = "";
    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        file = string.Concat(
          file,
          $"({matrix[x, y].x},{matrix[x, y].y})",
          x == width - 1 ? "" : ", "
        );
      }
      file = string.Concat(file, "\n");
    }

    Debug.Log("FileUtility.SaveMatrixAsCsv - saving: " + completePath);
    saveString(completePath, file);
    Debug.Log("\tcompleted saving: " + completePath);
  }

  public static float[,] LoadCsvIntoFloatMatrix(string fullPath)
  {
    if (!File.Exists(fullPath)) {
      Debug.LogError($"FileUtility.LoadCsvIntoMatrix path does not exist! Returning empty\n\t{fullPath}");
      return new float[0, 0];
    }

    var readfile = File.ReadAllLines(fullPath);
    if (readfile.Length == 0) {
      Debug.LogError($"FileUtility.LoadCsvIntoMatrix path is mepty! Returning empty\n\t{fullPath}");
      return new float[0, 0];
    }

    int rows = readfile.Length;
    int columns = readfile[0].Count(ch => ch == ',') + 1;

    var data = new float[rows, columns];
    for (int y = 0; y < rows; y++) {
      string[] lineData = readfile[y].Split(',');
      for (int x = 0; x < columns; x++) {
        data[x, y] = float.Parse(lineData[x]);
      }
    }

    return data;
  }

  private static void saveString(string path, string file)
  {
    using (var save = new FileStream(path, FileMode.Create)) {
      using (var writer = new StreamWriter(save)) {
        writer.Write(file);
      }
    }
  }

  private static void createDirectory(string path)
  {
    if (!Directory.Exists(path)) {
      try {
        Directory.CreateDirectory(path);
      } catch (Exception e) {
        Debug.LogError("Save Texture: Could not create path: " + path + "\n" + e);
      }
    }
  }
}
