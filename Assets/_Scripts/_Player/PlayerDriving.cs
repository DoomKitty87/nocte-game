using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDriving : MonoBehaviour
{

  [SerializeField] private MonoBehaviour[] _toDisable; 
  
  private Collider _playerCollider;
  private Rigidbody _rb;
  private PlayerController _playerController;
  private PlayerCameraController _playerCameraController;
  
  public KeyCode _vehicleKey;
  private bool _hasVehicle;
  private List<GameObject> _availableVehicles;
  private bool _inVehicle;
  private GameObject _currentVehicle;

  private void Awake() {
    _playerCollider = GetComponent<Collider>();
    _rb = GetComponent<Rigidbody>();
    _playerController = GetComponent<PlayerController>();
    _playerCameraController = GetComponent<PlayerCameraController>();
  }

  private void Update() {
    if (Input.GetKeyDown(_vehicleKey) && _hasVehicle && !_inVehicle) EnterVehicle(_availableVehicles[0]);
    else if (Input.GetKeyDown(_vehicleKey) && _inVehicle) ExitVehicle();
  }

  private void OnTriggerEnter(Collider other) {
    if (other.gameObject.CompareTag("Vehicle")) {
      // Prevents re-adding vehicle to list when exiting vehicle
      if (!_availableVehicles.Contains(other.gameObject)) {
        _hasVehicle = true;
        _availableVehicles.Add(other.gameObject);
      }
    }
  }

  private void OnTriggerExit(Collider other) {
    if (_availableVehicles.Contains(other.gameObject)) {
      _availableVehicles.Remove(other.gameObject);
    }
    if (_availableVehicles.Count == 0) _hasVehicle = false;
  }

  private void EnterVehicle(GameObject toEnter) {
    for (int i = 0; i < _toDisable.Length; i++) {
      _toDisable[i].enabled = false;
    }

    _playerController.State = PlayerController.PlayerStates.Driving;
    
    _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    _rb.isKinematic = true;
    _playerCollider.enabled = false;
    _inVehicle = true;
    _currentVehicle = toEnter;
    _playerController.SetParent(toEnter.GetComponent<VehicleControl>()._playerSeat.transform);
    _playerCameraController.SetParent(toEnter.GetComponent<VehicleControl>()._playerSeat.transform);
    _playerCameraController.ResetRotation();
    _playerCameraController.UseClamp(90);
    toEnter.GetComponent<VehicleControl>().EnterVehicle();
  }

  private void ExitVehicle() {
    for (int i = 0; i < _toDisable.Length; i++) {
      _toDisable[i].enabled = true;
    }
  
    _rb.isKinematic = false;
    _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    _playerCollider.enabled = true;
    _inVehicle = false;
    _currentVehicle.GetComponent<VehicleControl>().ExitVehicle();
    _playerCameraController.ResetParent();
    _playerCameraController.ResetRotation();
    _playerCameraController.ResetClamp();

    _playerController.State = PlayerController.PlayerStates.Idle;
  }
}