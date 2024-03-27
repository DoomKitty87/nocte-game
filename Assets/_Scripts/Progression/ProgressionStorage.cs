using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProgressionStorage : MonoBehaviour
{

  public struct ProgressionData
  {
    public int cores;
    public int usedcores;
  }

  public ProgressionData _progression = new ProgressionData();

  private static ProgressionStorage _instance;
  public static ProgressionStorage Instance { get { return _instance; } }

  public void SaveProgressionData()
  {
    StorageInterface.SaveData("progression.dat", _progression);
  }

  public ProgressionData LoadProgressionData()
  {
    ProgressionData data = (ProgressionData)StorageInterface.LoadData("progression.dat");
    _progression = data;
    return data;
  }
}