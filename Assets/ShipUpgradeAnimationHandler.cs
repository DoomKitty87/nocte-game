using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ShipUpgradeAnimationHandler : MonoBehaviour {
    [SerializeField] private GameObject _upgradeScreenObject;
    [SerializeField] private GameObject _upgradeScreenCamera;
    [SerializeField] private GameObject _homeScreenObject;

    [SerializeField] private VisualEffect _holoTableVFX;
    
    public void ToggleUpgradeScreen(bool enable) {
        _homeScreenObject.SetActive(!enable);
        _upgradeScreenObject.SetActive(enable);
        _upgradeScreenCamera.SetActive(enable);
        
        _holoTableVFX.SetBool("ZoomedIn", enable);
    }
    
    
}
