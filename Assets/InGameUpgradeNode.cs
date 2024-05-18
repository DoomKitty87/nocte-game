using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUpgradeNode : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI upgradeName;
	[SerializeField] private TextMeshProUGUI upgradeLevel;

	public void SetUpgradeName(string name) {
		upgradeName.text = name;
	}

	public void UpgradeLevel() {
		Debug.Log("Level up");
		UpgradeInfo.AddLevel(upgradeName.text);
		Debug.Log(UpgradeInfo.GetUpgrade(upgradeName.text).value);
		upgradeLevel.text = UpgradeInfo.GetUpgrade(upgradeName.text).value.ToString();
	}
}
