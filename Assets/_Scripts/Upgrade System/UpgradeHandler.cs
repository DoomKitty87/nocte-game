using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeHandler : MonoBehaviour
{
  public int _upgradeLevels;

  private void OnEnable() {
    _upgradeLevels = PlayerMetaProgression.Instance.AvailableCores;
  }
  
}
