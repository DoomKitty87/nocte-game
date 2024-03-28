using UnityEngine;
using UpgradeSystem;

public class UpgradeHandler : MonoBehaviour
{
  [SerializeField] UpgradeTree[] _upgradeTrees; 

  public int _upgradeLevels;

  private void OnEnable() {
    _upgradeLevels = PlayerMetaProgression.Instance.AvailableCores;

    bool[] unlockedBlueprints = PlayerMetaProgression.Instance.GetAvailableBlueprints();

    for (int i = 0; i < _upgradeTrees.Length; i++) {
      _upgradeTrees[i]._enabled = unlockedBlueprints[i];
      Debug.Log($"Blueprint {i} is enabled: {unlockedBlueprints[i]}");
    }
  }
}
