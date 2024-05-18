using UnityEngine;
using UnityEngine.Events;

public static class UpgradeInfo
{

	public static int xpLevel = 0;

	public static UnityEvent OnLevelChange = new UnityEvent();

	public struct Upgrade
	{
		public string name;
		public bool isLocked;
		public int value;
	}

  // All values should default to 1 as they are multipliers, but -1 if the upgrade is locked.
  // All speed multipliers are already taken as inverses, just treat them as speed increases instead of time decreases (1+).
  public static Upgrade[] Upgrades;

  public static Upgrade GetUpgrade(string name) {
	  Upgrade upgrade;
	  if (Upgrades == null) {
			Debug.LogWarning("Upgrade is null");
		  return new Upgrade { name = "Error", isLocked = true, value = -1 };
	  }
	  
		try {
		  upgrade = System.Array.Find(Upgrades, upgrade => upgrade.name == name);
	  }
	  catch {
			upgrade = new Upgrade { name = "Error", isLocked = true, value = -1 };
			Debug.LogError("Upgrade not found: " + name);
	  }

		return upgrade;
	}

  public static void AddXPLevel() {
		Debug.Log("Increased Level");
	  xpLevel++;
	  OnLevelChange?.Invoke();
  }

  public static void RemoveXPLevel() {
	  xpLevel--;
	  OnLevelChange?.Invoke();
	}

	public static void AddLevel(string name) {
	  for (int i = 0; i < 12; i++) {
		  if (Upgrades[i].name == name) {
				Upgrades[i].value++;
		  }
	  }

  }

  public static void Initialize() {
		PlayerMetaProgression instance = PlayerMetaProgression.Instance;

		Upgrades = new Upgrade[] {
			new Upgrade { name = "Jump Height", isLocked = (instance.CheckUpgrade(0) != 2), value = 0 },
			new Upgrade { name = "Sprint Speed", isLocked = (instance.CheckUpgrade(1) != 2), value = 0 },
			new Upgrade { name = "Max Health", isLocked = (instance.CheckUpgrade(2) != 2), value = 0 },
			new Upgrade { name = "Health Regen", isLocked = (instance.CheckUpgrade(3) != 2), value = 0 },
			new Upgrade { name = "Damage", isLocked = (instance.CheckUpgrade(4) != 2), value = 0 },
			new Upgrade { name = "Reload Speed", isLocked = (instance.CheckUpgrade(5) != 2), value = 0 },
			new Upgrade { name = "Mag Size", isLocked = (instance.CheckUpgrade(6) != 2), value = 0 },
			new Upgrade { name = "Crit Chance", isLocked = (instance.CheckUpgrade(7) != 2), value = 0 },
			new Upgrade { name = "Scan Range", isLocked = (instance.CheckUpgrade(8) != 2), value = 0 },
			new Upgrade { name = "Grapple Range", isLocked = (instance.CheckUpgrade(9) != 2), value = 0 },
			new Upgrade { name = "Grapple Strength", isLocked = (instance.CheckUpgrade(10) != 2), value = 0 },
			new Upgrade { name = "Scan Cooldown", isLocked = (instance.CheckUpgrade(11) != 2), value = 0 }
		};
	}
}