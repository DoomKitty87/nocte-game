using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerDriving : MonoBehaviour
{
  private PlayerInput _input;

  [SerializeField] private MonoBehaviour[] _toDisable; 
  
  // private PlayerCameraController _playerCameraController;
  
  private bool _hasVehicle;
  private List<GameObject> _availableVehicles = new List<GameObject>();
  private bool _inVehicle;
  private GameObject _currentVehicle;
  private bool _isFlying;

  private void Awake() {
    // _playerCameraController = GetComponent<PlayerCameraController>();
  }

  void Start()
  {
    _input = InputReader.Instance.PlayerInput;

    _input.Player.Interact.performed += TryEnterVehicle;
    _input.Driving.Leave.performed += TryExitVehicle;
    _input.Flying.Leave.performed += TryExitVehicle;

  }

  void OnDisable()
  {
    _input.Player.Interact.performed -= TryEnterVehicle;
    _input.Driving.Leave.performed -= TryExitVehicle;
  }

  private void TryEnterVehicle(InputAction.CallbackContext context) {
    // Debug.Log("Try to enter");
    if (_hasVehicle && !_inVehicle) {
      Debug.Log("Enter Vehicle");
      if (_availableVehicles[0].TryGetComponent<PlaneController>(out var planeController)) {
        InputReader.Instance.EnableFlying();
        _isFlying = true;
        EnterVehicle(_availableVehicles[0]);
      }
      else if (_availableVehicles[0].TryGetComponent<VehicleControl>(out var vehicleControl)) {
        InputReader.Instance.EnableDriving();
        _isFlying = false;
        EnterVehicle(_availableVehicles[0]);
      }
    }
  }

  private void TryExitVehicle(InputAction.CallbackContext context) {
    if (_inVehicle) {
      InputReader.Instance.EnablePlayer();
      ExitVehicle();
    }
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
      Debug.Log(_toDisable.Length); //
      _toDisable[i].enabled = false;
    }

    PlayerController.Instance.State = PlayerController.PlayerStates.Driving;
    
    _inVehicle = true;
    _currentVehicle = toEnter;
    if (_isFlying) {
      PlayerController.Instance.SetParent(toEnter.GetComponent<PlaneController>()._playerSeat.transform);
      toEnter.GetComponent<PlaneController>().EnterVehicle();
    }
    else {
      PlayerController.Instance.SetParent(toEnter.GetComponent<VehicleControl>()._playerSeat.transform);
      toEnter.GetComponent<VehicleControl>().EnterVehicle();
    }
    // _playerCameraController.SetParent(toEnter.GetComponent<VehicleControl>()._playerSeat.transform);
    // _playerCameraController.ResetRotation();
    // _playerCameraController.UseClamp(90);
  }

  private void ExitVehicle() {
    for (int i = 0; i < _toDisable.Length; i++) {
      _toDisable[i].enabled = true;
    }
    
    _inVehicle = false;

    if (_isFlying) {
      _currentVehicle.GetComponent<PlaneController>().ExitVehicle();
    }
    else {
      _currentVehicle.GetComponent<VehicleControl>().ExitVehicle();
    }
    // _playerCameraController.ResetParent();
    // _playerCameraController.ResetRotation();
    // _playerCameraController.ResetClamp();

    PlayerController.Instance.State = PlayerController.PlayerStates.Idle;
  }
}