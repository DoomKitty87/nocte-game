using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

[RequireComponent(typeof(HealthInterface))]
public class CombatCore : MonoBehaviour
{
  [Header("Dependencies")]
  [SerializeField] private GameObject _weaponContainer;
  [Header("Info")]
  [SerializeField] private WeaponItem _currentWeaponItem;
  [SerializeField] private GameObject _weaponInstance;
  [SerializeField] private WeaponScript _instanceScript;
  
  private bool _fire1LastFrame;
  private bool _fire2LastFrame;

  public Vector3 GetCenterScreenWorldPosition() {
    if (Camera.main != null)
      return Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2));
    else {
      return Vector3.zero;
    }
  }
  
  private void InstanceWeaponItem() {
    GameObject instance = Instantiate(_currentWeaponItem._weaponPrefab, _weaponContainer.transform);
    _weaponInstance = instance;
    _instanceScript = instance.GetComponent<WeaponScript>();
    if (_instanceScript == null) {
      Debug.LogWarning($"{gameObject.name} CombatCore: Could not find WeaponScript or subclass on weaponInstance!");
    }
    _instanceScript._instancingCombatCoreScript = this;
  }
  
  private void UpdateControls() {
    if (Input.GetAxisRaw("Fire1") > 0) {
      if (_fire1LastFrame == false) {
        _instanceScript.FireDown();
      }
      else {
        _instanceScript.FireHold();
      }
      _fire1LastFrame = true;
    }
    else {
      if (_fire1LastFrame) {
        _instanceScript.FireUp();
      }
      _fire1LastFrame = false;
    }
    if (Input.GetAxisRaw("Fire2") > 0) {
      if (_fire2LastFrame == false) {
        _instanceScript.Fire2Down();
      }
      else {
        _instanceScript.Fire2Hold();
      }
      _fire2LastFrame = true;
    }
    else {
      if (_fire2LastFrame) {
        _instanceScript.Fire2Up();
      }
      _fire2LastFrame = false;
    }
  }
  
  // Start is called before the first frame update
  private void Start() {
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