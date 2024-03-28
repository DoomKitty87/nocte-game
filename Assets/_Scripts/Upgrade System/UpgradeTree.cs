using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradeSystem {
  public class UpgradeTree : MonoBehaviour
  {
    public bool _enabled;
    public int _upgradeTreeIndex;

    [SerializeField] private List<UpgradeNode> Roots;

    [SerializeField] private Button _resetButton;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private string _currentLevelTextPretext = "Current Level: "; // idk what to call this
    
    private int UpgradeLevels { 
      get => transform.parent.GetComponent<UpgradeHandler>()._upgradeLevels;
      set => transform.parent.GetComponent<UpgradeHandler>()._upgradeLevels = value;
    } // Temporary public for testing

    private int _localUpgradeLevels = 0;
    
    private void Start() {
      if (!_enabled) return;
      currentLevelText.text = _currentLevelTextPretext + UpgradeLevels;
      foreach (var Root in Roots) {
        Root.EnableNode();
        Root.AssignAllButtons(this);
      }

      int upgradeIndex = 0;
      foreach (var Root in Roots) {
        Root.LoadAllLevels(_upgradeTreeIndex, ref upgradeIndex);
      }

      _resetButton.onClick.AddListener(ResetButton);
    }

    private void OnEnable() {
      currentLevelText.text = _currentLevelTextPretext + UpgradeLevels;
    }

    public void OnClick(UpgradeNode node) {
      
      if (!node._enabled) return;

      if (UpgradeLevels <= 0) return;
      
      int UpgradeCost = 0;
      // Increases level on node in IncreaseLevel function
      if (!node.IncreaseLevel(UpgradeLevels, ref UpgradeCost)) return;
      UpgradeLevels -= UpgradeCost;
      PlayerMetaProgression.Instance.UseCore(UpgradeCost);

      _localUpgradeLevels += UpgradeCost;

      currentLevelText.text = _currentLevelTextPretext + UpgradeLevels;
    }

    public void ResetButton() {

      foreach (var Root in Roots)
        Root.ResetAllNodes();

      UpgradeLevels += _localUpgradeLevels;
      PlayerMetaProgression.Instance.FreeCore(_localUpgradeLevels);

      _localUpgradeLevels = 0;

      currentLevelText.text = _currentLevelTextPretext + UpgradeLevels;

      foreach (var Root in Roots)
        Root.EnableNode();
    }
  }
}