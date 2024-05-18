using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUpgradeNode : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI upgradeName;

	public void SetUpgradeName(string name) {
		upgradeName.text = name;
	}
}
