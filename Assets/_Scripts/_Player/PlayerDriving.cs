using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerDriving : MonoBehaviour
{
  private InputHandler _input;

  [SerializeField] private MonoBehaviour[] _toDisable; 
  
  private PlayerCameraController _playerCameraController;
  
  private bool _hasVehicle;
  private List<GameObject> _availableVehicles = new List<GameObject>();
  private bool _inVehicle;
  private GameObject _currentVehicle;

  private void Awake() {
    _playerCameraController = GetComponent<PlayerCameraController>();
  }

  void Start()
  {
    _input = InputHandler.Instance;

    _input.PLAYER_interactAction.performed += TryEnterVehicle;
    //_input.DRIVING_leaveAction.performed += TryExitVehicle;

  }

  void OnDisable()
  {
    _input.PLAYER_interactAction.performed -= TryEnterVehicle;
    _input.DRIVING_leaveAction.performed -= TryExitVehicle;
  }

  private void TryEnterVehicle(InputAction.CallbackContext context) {
    if (_hasVehicle && !_inVehicle) {
      Debug.Log(context);
      Debug.Log("Driving");
      _input.SwitchActiveInputMap("Driving");
      EnterVehicle(_availableVehicles[0]);
    }
  }

  private void TryExitVehicle(InputAction.CallbackContext context) {
    if (_inVehicle) {
      Debug.Log("Player");
      _input.SwitchActiveInputMap("Player");
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
      Debug.Log(_toDisable.Length);
      // _toDisable[i].enabled = false;
    }

    PlayerController.Instance.State = PlayerController.PlayerStates.Driving;
    
    _inVehicle = true;
    _currentVehicle = toEnter;
    PlayerController.Instance.SetParent(toEnter.GetComponent<VehicleControl>()._playerSeat.transform);
    // _playerCameraController.SetParent(toEnter.GetComponent<VehicleControl>()._playerSeat.transform);
    // _playerCameraController.ResetRotation();
    // _playerCameraController.UseClamp(90);
    toEnter.GetComponent<VehicleControl>().EnterVehicle();
  }

  private void ExitVehicle() {
    for (int i = 0; i < _toDisable.Length; i++) {
      // _toDisable[i].enabled = true;
    }
    
    _inVehicle = false;
    _currentVehicle.GetComponent<VehicleControl>().ExitVehicle();
    _playerCameraController.ResetParent();
    _playerCameraController.ResetRotation();
    // _playerCameraController.ResetClamp();

    PlayerController.Instance.State = PlayerController.PlayerStates.Idle;
  }
}