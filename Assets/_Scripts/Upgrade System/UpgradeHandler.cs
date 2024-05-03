using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UpgradeSystem;

public class UpgradeHandler : MonoBehaviour {
  [SerializeField] private bool _isMainMenu;
  
  [SerializeField] private UpgradeTree[] _upgradeTrees; 

  [SerializeField] private Button _addButton;
  [SerializeField] private Button _removeButton;
  [SerializeField] private TextMeshProUGUI currentLevelText;
  [SerializeField] private string _currentLevelTextPretext = "Current Level: "; // idk what to call this

  private int _upgradeLevels;
  [HideInInspector] public int UpgradeLevels { 
    get {
      return _upgradeLevels;
    }
    set {
      _upgradeLevels = value;
      SetUpgradeLevelText(value);
      PlayerMetaProgression.Instance.SaveData();
    }
  }

  private void OnEnable() {
    _addButton.onClick.AddListener(Add); 
    _removeButton.onClick.AddListener(Remove);

    SetValue(PlayerMetaProgression.Instance.AvailableCores);

    if (_isMainMenu) {
      PlayerMetaProgression.Instance.ConvertOwnedBlueprintsToUnlocked();
    }
    
    bool[] unlockedBlueprints = PlayerMetaProgression.Instance.GetAvailableBlueprints();

    for (int i = 0; i < _upgradeTrees.Length; i++) {
      var currentTree = _upgradeTrees[i];
      currentTree._upgradeHandler = this;
      currentTree._upgradeTreeIndex = i;
      currentTree._enabled = unlockedBlueprints[i];
    }
  }

  private void OnDisable() {
    _addButton.onClick.RemoveListener(Add);
    _removeButton.onClick.RemoveListener(Remove);
  }

  private void Add() {
    PlayerMetaProgression.Instance.AddCore(1);
    ChangeValue(1);
    PlayerMetaProgression.Instance.SaveData();
  }

  private void Remove() {
    if (UpgradeLevels <= 0) return;
    PlayerMetaProgression.Instance.AddCore(-1);
    ChangeValue(-1);
    PlayerMetaProgression.Instance.SaveData();
  
  }

  private void ChangeValue(int value) {
    UpgradeLevels += value;
  }

  private void SetValue(int value) {
    UpgradeLevels = value;
  }

  private void SetUpgradeLevelText(int value) {
    currentLevelText.text = _currentLevelTextPretext + _upgradeLevels;
  }
}
