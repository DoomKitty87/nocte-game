using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradeSystem {
  public class UpgradeTree : MonoBehaviour
  {
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
      currentLevelText.text = _currentLevelTextPretext + UpgradeLevels;
      foreach (var Root in Roots) {
        Root.EnableNode();
        Root.AssignAllButtons(this);
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
      _localUpgradeLevels += UpgradeCost;

      currentLevelText.text = _currentLevelTextPretext + UpgradeLevels;
    }

    public void ResetButton() {

      foreach (var Root in Roots)
        Root.ResetAllNodes();

      UpgradeLevels += _localUpgradeLevels;
      _localUpgradeLevels = 0;

      currentLevelText.text = _currentLevelTextPretext + UpgradeLevels;

      foreach (var Root in Roots)
        Root.EnableNode();
    }
  }
}