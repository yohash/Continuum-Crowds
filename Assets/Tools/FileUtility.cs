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

    // assemble string
    string file = "";
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        file = string.Concat(
          file,
          matrix[i, k],
          k == matrix.GetLength(1) - 1 ? "" : ", "
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

    // assemble string
    string file = "";
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        file = string.Concat(
          file,
          $"({matrix[i, k].x},{matrix[i, k].y})",
          k == matrix.GetLength(1) - 1 ? "" : ", "
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
      Debug.LogError($"FileUtility.LoadCsvIntoMatrix - path\n\t{fullPath} \ndoes not exist! Returning empty.");
      return new float[0, 0];
    }

    var readfile = File.ReadAllLines(fullPath);
    if (readfile.Length == 0) {
      Debug.LogError($"FileUtility.LoadCsvIntoMatrix - path\n\t{fullPath} \nis mepty! Returning empty.");
      return new float[0, 0];
    }

    int rows = readfile.Length;
    int columns = readfile[0].Count(ch => ch == ',') + 1;

    var data = new float[columns, rows];
    for (int i = 0; i < rows; i++) {
      string[] lineData = readfile[i].Split(',');
      for (int k = 0; k < columns; k++) {
        data[i, k] = float.Parse(lineData[k]);
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
      }
      catch (Exception e) {
        Debug.LogError("Save Texture: Could not create path: " + path + "\n" + e);
      }
    }
  }
}
