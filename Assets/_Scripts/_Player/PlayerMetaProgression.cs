using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerMetaProgression : MonoBehaviour
{
	private static PlayerMetaProgression _instance;

  public static PlayerMetaProgression Instance {
	  get { return _instance; }
  }

  // 0 = Locked, 1 = Unlocked but not purchased, 2 = Purchased
  public int[] DefaultUpgrades = new int[] {
    // Movement
    1, // Jump height     * 0
    1, // Sprint speed    * 1
    1, //                 * 2
    1, //                 * 3
    // Combat             
    1, // Damage          * 4
    1, // Reload speed    * 5
    1, // Bullet capacity * 6
    1, //                 * 7
    // Utility
    1, // Scan range      * 8
    1, // Grapple range   * 9
    1, //                 * 10
    1  //                 * 11
    };

  [System.Serializable]
  public struct ProgressionData
  {
    public int cores;
    public int usedcores;
    public int[] upgrades;
  }

  public int AvailableCores => _progression.cores - _progression.usedcores;

  public ProgressionData _progression = new ProgressionData();

  private void Awake()
  {
		if (Instance != null && Instance != this)
    {
			Destroy(this);
		}
    else {
			_instance = this;
		}

    LoadProgressionData();
    // Load player cores from save file
  }

  public void LoadProgressionData()
  {
    if (StorageInterface.LoadData("progression.dat") == null)
    {
      _progression.cores = 0;
      _progression.usedcores = 0;
      _progression.upgrades = DefaultUpgrades;

      Debug.Log("Created file 'progression.dat'.");
      SaveData();
      return;
    }
    ProgressionData data = (ProgressionData)StorageInterface.LoadData("progression.dat");
    _progression = data;
    SaveData();
    Debug.Log("Loaded file 'progression.dat'.");
  }

  public void SaveData()
  {
    SaveProgressionData();
  }

  private void SaveProgressionData()
  {
    StorageInterface.SaveData("progression.dat", _progression);

    Debug.Log($"Saved file 'progression.dat' at {Application.persistentDataPath}.");
	}

  public void AddCore()
  {
    _progression.cores++;
		SaveData();
  }

  public void AddCore(int cores)
  {
    _progression.cores += cores;
  }

  public void UseCore()
  {
    _progression.usedcores++;
  }

  public void UseCore(int cores)
  {
    _progression.usedcores += cores;
  }

  public void FreeCore()
  {
    _progression.usedcores--;
  }

  public void FreeCore(int cores)
  {
    _progression.usedcores -= cores;
  }

  public void Unlock(int index)
  {
    if (_progression.upgrades[index] != 0)
      Debug.LogWarning($"Upgrade index {index} is in the wrong state {_progression.upgrades[index]}. Fix this.");
    _progression.upgrades[index] = 1;
  }

  public void Buy(int index)
  {
    if (_progression.upgrades[index] != 1)
      Debug.LogWarning($"Upgrade index {index} is in the wrong state {_progression.upgrades[index]}. Fix this.");
    _progression.upgrades[index] = 2;
  }

  public void Lock(int index)
  {
    _progression.upgrades[index] = 0;
  }

  public int CheckUpgrade(int index)
  {
    return _progression.upgrades[index];
  }
}
