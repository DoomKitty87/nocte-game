using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMetaProgression : MonoBehaviour
{
  private static PlayerMetaProgression _instance;
  public static PlayerMetaProgression Instance { get { return _instance; } }


  // Blueprints: 0 = Dexterity, 1 = Lethality, 2 = Perception (Scanner), 3 = Utility (Grappling Hook), 4 = Vitality

  [System.Serializable]
  public struct Blueprint {
    public bool owned;
    public bool unlocked;
    public int[] upgradeLevels;
  }

  [System.Serializable]
  public struct ProgressionData
  {
    public int cores;
    public int usedcores;
    public Blueprint[] blueprints;
  }

  private int _playerCores;

  private int _usedCoresValue;
  private int _usedCores { 
    get { return _usedCoresValue; } 
    set { 
      _usedCoresValue = value; 
      SaveData(); 
    } 
  }

  public int AvailableCores { get { return _playerCores - _usedCores; } }

  public ProgressionData _progression = new ProgressionData();

  private void OnEnable() {
    if (_instance == null) {
      _instance = this;
    } else {
      Destroy(this);
    }

    LoadProgressionData();

    Debug.Log(_progression.blueprints[0].upgradeLevels[0]);
    // Load player cores from save file
  }

  public void LoadProgressionData()
  {
    if (StorageInterface.LoadData("progression.dat") == null) {
      _progression.cores = 0;
      _progression.usedcores = 0;
        _progression.blueprints = new Blueprint[5] {
          new Blueprint { owned = true, unlocked = true, upgradeLevels = new int[5] {0, 0, 0, 0, 0} },
          new Blueprint { owned = true, unlocked = true, upgradeLevels = new int[1] {0} },
          new Blueprint { owned = false, unlocked = false, upgradeLevels = new int[3] {0, 0, 0} },
          new Blueprint { owned = false, unlocked = false, upgradeLevels = new int[3] {0, 0, 0} },
          new Blueprint { owned = true, unlocked = true, upgradeLevels = new int[2] {0, 0} }
        };

      return;
    }
    ProgressionData data = (ProgressionData)StorageInterface.LoadData("progression.dat");
    _progression = data;
  }

  public void SaveData() {
    SaveProgressionData();
  }

    public void SaveProgressionData()
  {
    StorageInterface.SaveData("progression.dat", _progression);
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
    return _progression.blueprints[index].owned;
  }

  public bool CheckBlueprintUnlocked(int index) {
    return _progression.blueprints[index].unlocked;
  }

  public void ObtainBlueprint(int index) {
    _progression.blueprints[index].owned = true;
  }

  public bool[] GetAvailableBlueprints() {
    bool[] unlockedBlueprints = new bool[5];
    for (int i = 0; i < _progression.blueprints.Length; i++) {
      unlockedBlueprints[i] = _progression.blueprints[i].unlocked;
    }
    return unlockedBlueprints;
  }

  public int GetUpgradeLevel(int blueprintIndex, int upgradeIndex) {
    return _progression.blueprints[blueprintIndex].upgradeLevels[upgradeIndex];
  }

  public void SetUpgradeLevel(int blueprintIndex, int upgradeIndex, int level) {
    _progression.blueprints[blueprintIndex].upgradeLevels[upgradeIndex] = level;
  }

  public void UnlockBlueprints() {
    for (int i = 0; i < _progression.blueprints.Length; i++) {
      if (!_progression.blueprints[i].unlocked) return;
      _progression.blueprints[i].unlocked = true;
    }
  }
}
