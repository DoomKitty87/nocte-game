using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UpgradeSystem
{
    [CreateAssetMenu(fileName = "Upgrade Name", menuName = "ScriptableObjects/UpgradeObject")]
    public class UpgradeScriptable : ScriptableObject
    {
        public UpgradeType _upgradeType;
        public string _upgradeName;
        public string _upgradeDescription;
        public int _upgradeCost;
        public int _upgradeMaxLevel;
    }

    public enum UpgradeType
    {
        Dexterity,  // Movement
        Utility,    // Grapple
        Perception, // Scan
        Lethality,  // Damage
        Vitality    // Health
    }
}