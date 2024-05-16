using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UpgradeSystem
{
  [CreateAssetMenu(fileName = "Upgrade Name", menuName = "ScriptableObjects/UpgradeObject")]
  public class UpgradeScriptable : ScriptableObject
  {
    public string _upgradeName;
    public string _upgradeDescription;
  }
}