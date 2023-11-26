using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProgressionStorage : MonoBehaviour
{

  public struct ProgressionData
  {
    public int level;
    public int experience;
  }

  public ProgressionData _progression = new ProgressionData();

  public void SaveProgressionData(ProgressionData data = _progression)
  {
    StorageInterface.SaveData("progression.dat", data);
  }

  public ProgressionData LoadProgressionData()
  {
    ProgressionData data = (ProgressionData)StorageInterface.LoadData("progression.dat");
    _progression = data;
    return data;
  }
}