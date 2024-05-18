using UnityEngine;

public class InGameUpgradeHandler : MonoBehaviour
{

	private string[] names = new string[] { "Jump Height", "Sprint Speed", "Max Health", "Health Regen", "Damage", "Reload Speed", "Mag Size", "Crit Chance", "Scan Range", "Grapple Range", "Grapple Strength", "Scan Cooldown"};

	[SerializeField] private Transform prefabParent;
	[SerializeField] private GameObject upgradePrefab;

	private GameObject[] upgradeObjects = new GameObject[12];

	private void Start() {
		Invoke(nameof(FillUpgradeList), 0.5f);
	}

	private void FillUpgradeList() {
		for (int i = 0; i < 12; i++) {
			UpgradeInfo.Upgrade upgrade = UpgradeInfo.GetUpgrade(names[i]);
			if (!upgrade.isLocked) {
				GameObject upgradeObject = Instantiate(upgradePrefab, prefabParent);
				upgradeObjects[i] = upgradeObject;
				upgradeObject.GetComponent<InGameUpgradeNode>().SetUpgradeName(upgrade.name);
				// Assign stuff here
			}
		}
	}
}
