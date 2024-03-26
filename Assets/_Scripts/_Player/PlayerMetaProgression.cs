using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMetaProgression : MonoBehaviour
{

  private static PlayerMetaProgression _instance;
  public static PlayerMetaProgression Instance { get { return _instance; } }

  private int _playerCores;

  private int _usedCores;

  public int AvailableCores { get { return _playerCores - _usedCores; } }

  private void OnEnable() {
    if (_instance == null) {
      _instance = this;
    } else {
      Destroy(this);
    }

    ProgressionStorage.Instance.LoadProgressionData();
    _playerCores = ProgressionStorage.Instance._progression.cores;
    _usedCores = ProgressionStorage.Instance._progression.usedcores;
    // Load player cores from save file
  }

  public void SaveData() {
    ProgressionStorage.Instance._progression.cores = _playerCores;
    ProgressionStorage.Instance._progression.usedcores = _usedCores;
    ProgressionStorage.Instance.SaveProgressionData();
  }

  public void AddCore() {
    _playerCores++;
  }

  public void AddCore(int cores) {
    _playerCores += cores;
  }

  public void UseCore() {
    _usedCores++;
  }

  public void UseCore(int cores) {
    _usedCores += cores;
  }

  public void FreeCore() {
    _usedCores--;
  }

  public void FreeCore(int cores) {
    _usedCores -= cores;
  }

}
