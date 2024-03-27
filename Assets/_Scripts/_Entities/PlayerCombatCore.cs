using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(HealthInterface))]
public class PlayerCombatCore : MonoBehaviour
{
  private PlayerInput _input;

  [Header("Dependencies")]
  public Camera _mainCamera;
  [SerializeField] private GameObject _weaponContainer;
  [SerializeField] private WeaponItem _currentWeaponItem;
  [Header("Info - Dont change in editor")]
  [SerializeField] private GameObject _weaponInstance;
  [SerializeField] private WeaponScript _instanceScript;
  
  // See line 48
  // [SerializeField] private WeaponUI _weaponUI;
  
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
    
  // Start is called before the first frame update
  private void Start() {
    _input = InputReader.Instance.PlayerInput;

    _input.Player.Shoot.started += _ => _instanceScript.FireDown();
    _input.Player.Shoot.performed += _ => _instanceScript.FireHold();
    _input.Player.Shoot.canceled += _ => _instanceScript.FireUp();

    _input.Player.ADS.started += _ => _instanceScript.Fire2Down();
    _input.Player.ADS.performed += _ => _instanceScript.Fire2Hold();
    _input.Player.ADS.canceled += _ => _instanceScript.Fire2Up();

    if (_currentWeaponItem != null) InstanceWeaponItem();
  }

  void OnDisable()
  {
    _input.Player.Shoot.started -= _ => _instanceScript.FireDown();
    _input.Player.Shoot.performed -= _ => _instanceScript.FireHold();
    _input.Player.Shoot.canceled -= _ => _instanceScript.FireUp();

    _input.Player.ADS.started -= _ => _instanceScript.Fire2Down();
    _input.Player.ADS.performed -= _ => _instanceScript.Fire2Hold();
    _input.Player.ADS.canceled -= _ => _instanceScript.Fire2Up();
  }

}