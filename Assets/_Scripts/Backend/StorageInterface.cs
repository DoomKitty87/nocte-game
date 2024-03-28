using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public static class StorageInterface
{

  // Important note: This is meant for meta progression data, not for settings. Use PlayerPrefs for settings.

  public static void SaveData(string fileName, object data)
  {
    BinaryFormatter bf = new BinaryFormatter();
    if (!Directory.Exists(Application.persistentDataPath + "/savedata/"))
    {
      Directory.CreateDirectory(Application.persistentDataPath + "/savedata/");
    }
    FileStream file = File.Create(Application.persistentDataPath + "/savedata/" + fileName);
    bf.Serialize(file, data);
    file.Close();
  }

  public static object LoadData(string fileName)
  {
    if (File.Exists(Application.persistentDataPath + "/savedata/" + fileName))
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Open(Application.persistentDataPath + "/savedata/" + fileName, FileMode.Open);
      object data = bf.Deserialize(file);
      file.Close();
      return data;
    }
    else
    {
      return null;
    }
  }

  public static void ClearData() {
    DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/savedata/");
    foreach (FileInfo file in di.GetFiles())
    {
      file.Delete();
    }
  }

}