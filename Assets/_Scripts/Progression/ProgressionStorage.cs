using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProgressionStorage : MonoBehaviour
{

  public struct ProgressionData
  {
    public int cores;
    public int usedcores;
    public bool[] ownedBlueprints;
    public bool[] unlockedBlueprints;

  }

  public ProgressionData _progression = new ProgressionData();

  private static ProgressionStorage _instance;
  public static ProgressionStorage Instance { get { return _instance; } }

  private void Awake() {
    if (_instance == null) {
      _instance = this;
    } else {
      Destroy(this);
    }
  }

  public void SaveProgressionData()
  {
    StorageInterface.SaveData("progression.dat", _progression);
  }

  public ProgressionData LoadProgressionData()
  {
    if (StorageInterface.LoadData("progression.dat") == null) {
      _progression.cores = 0;
      _progression.usedcores = 0;
      _progression.ownedBlueprints = new bool[5] {true, true, false, false, true};
      _progression.unlockedBlueprints = new bool[5] {true, true, false, false, true};
      return _progression;
    }
    ProgressionData data = (ProgressionData)StorageInterface.LoadData("progression.dat");
    _progression = data;
    return data;
  }
}