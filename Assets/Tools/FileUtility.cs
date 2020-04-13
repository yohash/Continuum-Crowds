using System;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

public static class FileUtility
{
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
    using (var save = new FileStream(completePath, FileMode.Create)) {
      using (var writer = new StreamWriter(save)) {
        writer.Write(file);
      }
    }
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
    using (var save = new FileStream(completePath, FileMode.Create)) {
      using (var writer = new StreamWriter(save)) {
        writer.Write(file);
      }
    }
    Debug.Log("\tcompleted saving: " + completePath);
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
