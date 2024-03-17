using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthInterface))]
public class PlayerCombatCore : MonoBehaviour
{
  private InputHandler _input;

  [Header("Dependencies")]
  public Camera _mainCamera;
  [SerializeField] private GameObject _weaponContainer;
  [SerializeField] private WeaponItem _currentWeaponItem;
  [Header("Info - Dont change in editor")]
  [SerializeField] private GameObject _weaponInstance;
  [SerializeField] private WeaponScript _instanceScript;
  [SerializeField] private WeaponUI _weaponUI;
  
  private bool _fire1LastFrame;
  private bool _fire2LastFrame;


  public void SetWeaponItem(WeaponItem weaponItem) {
    _currentWeaponItem = weaponItem;
    if (_weaponInstance != null) {
      Destroy(_weaponInstance);
    }
    InstanceWeaponItem();
  }
  
  private void InstanceWeaponItem() {
    if (_weaponContainer == null) {
      Debug.LogWarning("The variable _weaponContainer of PlayerCombatCore has not been assigned. PlayerCombatCore has been disabled.");
      enabled = false;
      return;
    }
    GameObject instance = Instantiate(_currentWeaponItem._weaponPrefab, _weaponContainer.transform);
    _weaponInstance = instance;
    _instanceScript = instance.GetComponent<WeaponScript>();
    if (_instanceScript == null) {
      Debug.LogWarning($"{gameObject.name} CombatCore: Could not find WeaponScript or subclass on weaponInstance!");
    }
    _instanceScript._instancingPlayerCombatCoreScript = this;
  }
  
  private void UpdateControls() {
    if (_input.Shoot) {
      if (_fire1LastFrame == false) {
        _instanceScript.FireDown();
        _weaponUI.UpdateAmmoCount(_instanceScript.GetAmmo);
      }
      else {
        _instanceScript.FireHold();
        _weaponUI.UpdateAmmoCount(_instanceScript.GetAmmo);
      }
      _fire1LastFrame = true;
    }
    else {
      if (_fire1LastFrame) {
        _instanceScript.FireUp();
        _weaponUI.UpdateAmmoCount(_instanceScript.GetAmmo);
      }
      _fire1LastFrame = false;
    }
    if (_input.Grapple) { // Super confused what this is supposed to do, need to impliment new input system here but grapple is already right click?
      Debug.LogWarning("Unclear what's happening here, see comment in PlayerCombatCore.cs line 65.");
      return;
      if (_fire2LastFrame == false) {
        _instanceScript.Fire2Down();
        _weaponUI.UpdateAmmoCount(_instanceScript.GetAmmo);
      }
      else {
        _instanceScript.Fire2Hold();
        _weaponUI.UpdateAmmoCount(_instanceScript.GetAmmo);
      }
      _fire2LastFrame = true;
    }
    else {
      if (_fire2LastFrame) {
        _instanceScript.Fire2Up();
        _weaponUI.UpdateAmmoCount(_instanceScript.GetAmmo);
      }
      _fire2LastFrame = false;
    }
  }
  
  // Start is called before the first frame update
  private void Start() {
    _input = InputHandler.Instance;

    _fire1LastFrame = false;
    _fire2LastFrame = false;
    if (_currentWeaponItem != null) InstanceWeaponItem();
  }
  
  // Update is called once per frame
  private void Update() {
    if (_instanceScript != null) {
      UpdateControls();
    }
  }
}