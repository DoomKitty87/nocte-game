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

  private bool[] _ownedBlueprints = new bool[5] {true, true, false, false, true};
  // Blueprints: 0 = Dexterity, 1 = Lethality, 2 = Perception (Scanner), 3 = Utility (Grappling Hook), 4 = Vitality

  private bool[] _unlockedBlueprints = new bool[5] {true, true, false, false, true};

  private void OnEnable() {
    if (_instance == null) {
      _instance = this;
    } else {
      Destroy(this);
    }

    ProgressionStorage.Instance.LoadProgressionData();
    _playerCores = ProgressionStorage.Instance._progression.cores;
    _usedCores = ProgressionStorage.Instance._progression.usedcores;
    _ownedBlueprints = ProgressionStorage.Instance._progression.ownedBlueprints;
    _unlockedBlueprints = ProgressionStorage.Instance._progression.unlockedBlueprints;
    // Load player cores from save file
  }

  public void SaveData() {
    ProgressionStorage.Instance._progression.cores = _playerCores;
    ProgressionStorage.Instance._progression.usedcores = _usedCores;
    ProgressionStorage.Instance._progression.ownedBlueprints = _ownedBlueprints;
    ProgressionStorage.Instance._progression.unlockedBlueprints = _unlockedBlueprints;
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

  public bool CheckBlueprintOwned(int index) {
    return _ownedBlueprints[index];
  }

  public bool CheckBlueprintUnlocked(int index) {
    return _unlockedBlueprints[index];
  }

  public void ObtainBlueprint(int index) {
    _ownedBlueprints[index] = true;
  }

  public bool[] GetAvailableBlueprints() {
    return _unlockedBlueprints;
  }

  public void UnlockBlueprints() {
    for (int i = 0; i < _unlockedBlueprints.Length; i++) {
      if (!_ownedBlueprints[i]) return;
      _unlockedBlueprints[i] = true;
    }
  }

}
