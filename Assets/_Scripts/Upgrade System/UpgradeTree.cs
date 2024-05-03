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
    [HideInInspector] public UpgradeHandler _upgradeHandler;

    [SerializeField] private List<UpgradeNode> Roots;

    [SerializeField] private Button _resetButton;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    
    private int UpgradeLevels { 
      get => _upgradeHandler.UpgradeLevels;
      set => _upgradeHandler.UpgradeLevels = value;
    }

    private int _localUpgradeLevels = 0;
    
    private void Start() {
      if (!_enabled) return;
      foreach (var Root in Roots) {
        Root.EnableNode();
        Root.AssignAllButtons(this);
      }

      int upgradeIndex = 0;
      int totalUpgradeLevels = 0;
      foreach (var Root in Roots) {
        Root.LoadAllLevels(_upgradeTreeIndex, ref upgradeIndex, ref totalUpgradeLevels);
      }
      _localUpgradeLevels += totalUpgradeLevels;

      _resetButton.onClick.AddListener(ResetButton);
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
    }

    public void ResetButton() {

      foreach (var Root in Roots)
        Root.ResetAllNodes();

      UpgradeLevels += _localUpgradeLevels;
      PlayerMetaProgression.Instance.FreeCore(_localUpgradeLevels);

      _localUpgradeLevels = 0;

      foreach (var Root in Roots)
        Root.EnableNode();
    }
  }
}