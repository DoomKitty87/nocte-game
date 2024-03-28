using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UpgradeSystem;

public class UpgradeHandler : MonoBehaviour
{
  [SerializeField] private UpgradeTree[] _upgradeTrees; 

  [SerializeField] private Button _addButton;
  [SerializeField] private TextMeshProUGUI currentLevelText;
  [SerializeField] private string _currentLevelTextPretext = "Current Level: "; // idk what to call this

  [HideInInspector] public int _upgradeLevels;

  private void OnEnable() {
    _addButton.onClick.AddListener(() => Add()); 

    _upgradeLevels = PlayerMetaProgression.Instance.AvailableCores;

    bool[] unlockedBlueprints = PlayerMetaProgression.Instance.GetAvailableBlueprints();

    for (int i = 0; i < _upgradeTrees.Length; i++) {
      _upgradeTrees[i]._upgradeTreeIndex = i;
      _upgradeTrees[i]._enabled = unlockedBlueprints[i];
    }
  }

  private void Add() {
    Debug.Log("Adding core");
    PlayerMetaProgression.Instance.AddCore(1);
    PlayerMetaProgression.Instance.SaveData();
    ChangeValue(1);
  }

  public void ChangeValue(int value) {
    _upgradeLevels += value;
    currentLevelText.text = _currentLevelTextPretext + _upgradeLevels;
  }

  public void SetValue(int value) {
    _upgradeLevels = value;
    currentLevelText.text = _currentLevelTextPretext + _upgradeLevels;
  }
}
