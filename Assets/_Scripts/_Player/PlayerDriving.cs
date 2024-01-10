using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDriving : MonoBehaviour
{

  [SerializeField] private MonoBehaviour[] _toDisable;
  [SerializeField] private Rigidbody _playerRigidbody;
  [SerializeField] private Collider _playerCollider;
  [SerializeField] private Transform _player;
  [SerializeField] private Transform _playerParent;

  public KeyCode _vehicleKey;
  private bool _hasVehicle;
  private List<GameObject> _availableVehicles = new List<GameObject>();
  private bool _inVehicle = false;
  private GameObject _currentVehicle;

  private void Update() {
    if (Input.GetKeyDown(_vehicleKey) && _hasVehicle && !_inVehicle) EnterVehicle(_availableVehicles[0]);
    else if (Input.GetKeyDown(_vehicleKey) && _inVehicle) ExitVehicle();
  }

  private void OnTriggerEnter(Collider other) {
    if (other.gameObject.CompareTag("Vehicle")) {
      _hasVehicle = true;
      _availableVehicles.Add(other.gameObject);
    }
  }

  private void OnTriggerExit(Collider other) {
    if (_availableVehicles.Contains(other.gameObject)) _availableVehicles.Remove(other.gameObject);
    if (_availableVehicles.Count == 0) _hasVehicle = false;
  }

  private void EnterVehicle(GameObject toEnter) {
    for (int i = 0; i < _toDisable.Length; i++) {
      _toDisable[i].enabled = false;
    }
    _playerRigidbody.isKinematic = true;
    _playerCollider.enabled = false;
    _inVehicle = true;
    _currentVehicle = toEnter;
    _player.parent = toEnter.GetComponent<VehicleControl>()._playerSeat;
    _player.localPosition = Vector3.zero;
    toEnter.GetComponent<VehicleControl>().EnterVehicle();
  }

  private void ExitVehicle() {
    for (int i = 0; i < _toDisable.Length; i++) {
      _toDisable[i].enabled = true;
    }

    _playerRigidbody.isKinematic = false;
    _playerCollider.enabled = true;
    _inVehicle = false;
    _player.parent = _playerParent;
    _currentVehicle.GetComponent<VehicleControl>().ExitVehicle();
  }
}