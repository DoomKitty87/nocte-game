using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InGameUpgradeNode : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI upgradeName;
	[SerializeField] private TextMeshProUGUI upgradeLevel;

	private void Start() {
		UpgradeInfo.OnLevelChange.AddListener(TestLevelCount);
	}

	public void SetUpgradeName(string name) {
		upgradeName.text = name;
	}

	public void UpgradeLevel() {
		UpgradeInfo.AddLevel(upgradeName.text);
		upgradeLevel.text = UpgradeInfo.GetUpgrade(upgradeName.text).value.ToString();

		UpgradeInfo.RemoveXPLevel();
	}

	private void TestLevelCount() {
		if (UpgradeInfo.xpLevel == 0) {
			GetComponent<Animator>().SetBool("Have Levels", false);
		}
		else {
			GetComponent<Animator>().SetBool("Have Levels", true);
		}
	}
}
