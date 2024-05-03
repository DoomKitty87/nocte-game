using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipUpgradeAnimationHandler : MonoBehaviour {
    [SerializeField] private GameObject _upgradeScreenObject;
    [SerializeField] private GameObject _upgradeScreenCamera;
    [SerializeField] private GameObject _homeScreenObject;
    
    public void ToggleUpgradeScreen(bool enable) {
        _homeScreenObject.SetActive(!enable);
        _upgradeScreenObject.SetActive(enable);
        _upgradeScreenCamera.SetActive(enable);
    }
    
    
}
