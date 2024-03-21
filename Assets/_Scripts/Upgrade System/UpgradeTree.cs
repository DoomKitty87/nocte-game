using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradeSystem {
  public class UpgradeTree : MonoBehaviour
  {
    public UpgradeNode Root;

    public TextMeshProUGUI currentLevelText;
    public string currentLevelTextPretext = "Current Level: "; // idk what to call this
    
    public int _upgradeLevels;
    private int _localUpgradeLevels;
    
    private void Start() {
      currentLevelText.text = currentLevelTextPretext + _upgradeLevels;
      Root.EnableNode();
      _localUpgradeLevels = _upgradeLevels;
    }

    public void OnClick(UpgradeNode node) {
      
      if (!node._enabled) return;

      if (_upgradeLevels <= 0) return;
      
      // Increases level on node in IncreaseLevel function
      if (!node.IncreaseLevel()) {
        return;
      }

      _upgradeLevels--;
      currentLevelText.text = currentLevelTextPretext + _upgradeLevels;
    }

    public void ResetButton() {
      Root.ResetAllNodes();
      _upgradeLevels = _localUpgradeLevels;
      currentLevelText.text = currentLevelTextPretext + _upgradeLevels;
      
      Root.EnableNode();
    }
  }
}