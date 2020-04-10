using System;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public static class FileUtility
{
  public static async Task SaveTextureAsPNG(string path, string filename, Texture2D texture)
  {
    if (!Directory.Exists(path)) {
      try {
        Directory.CreateDirectory(path);
      }
      catch (Exception e) {
        Debug.LogError("Save Texture: Could not create path: " + path + "\n" + e);
      }
    }

    string completePath = path + filename + ".png";

    Debug.Log("FileUtility.SaveTextureAsPNG - saving: " + completePath);
    using (var save = new FileStream(completePath, FileMode.Create)) {
      // save to the filestream
      //byte[] data = texture.GetRawTextureData();
      byte[] data = ImageConversion.EncodeToPNG(texture);
      await save.WriteAsync(data, 0, data.Length);
    }

    Debug.Log("\tcompleted saving: " + completePath);
  }

  public static void SaveMatrixAsCsv(string path, float[,] matrix)
  {

  }
}
