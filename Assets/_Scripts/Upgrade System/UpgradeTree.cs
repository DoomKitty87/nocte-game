using UnityEngine;

namespace UpgradeSystem {
  public class UpgradeTree : MonoBehaviour
  {
    public UpgradeNode Root;

    public void OnClick(UpgradeNode node) {
      if (!node._enabled) return;
      
      // Increases level on node in IncreaseLevel function
      if (node.IncreaseLevel()) {
        foreach (UpgradeNode childNode in node._children) childNode._enabled = true;
      }
      else {
        return;
      }
    }
  }
}