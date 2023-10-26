using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform _orientation;
    [SerializeField] private Transform _combatLookAt;
    [SerializeField] private Transform _explorationCamera;
    [SerializeField] private Transform _combatCamera;
    [SerializeField] private Transform _playerObj;
    private Rigidbody _rb;

    public LookType _lookType;
    public enum LookType
    {
        Exploration,
        Combat
    }
    
    // General Variables
    private Vector3 _moveDirection;
    
    // Rotation variables
    [SerializeField] float _rotationSpeed;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    private void Awake() {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Start() {
        _rb.freezeRotation = true;
        
        // Locks cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        UpdateInputVector();
        
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
        UpdateRotation();
    }

    private void UpdateRotation() {
        switch (_lookType) {
            case LookType.Exploration: {
                var playerPosition = transform.position;
                var cameraPosition = _explorationCamera.position;
                Vector3 viewDir = playerPosition - new Vector3(cameraPosition.x, playerPosition.y, cameraPosition.z);
                _orientation.forward = viewDir.normalized;

                if (_moveDirection != Vector3.zero)
                    _playerObj.forward = Vector3.Slerp(_playerObj.forward, _moveDirection.normalized,
                        Time.deltaTime * _rotationSpeed);
                break;
            }

            case LookType.Combat: {
                var combatPosition = _combatLookAt.position;
                var cameraPosition = _combatCamera.position;
                Vector3 dirToCombatLookAt = combatPosition - new Vector3(cameraPosition.x, combatPosition.y, cameraPosition.z);
                _orientation.forward = dirToCombatLookAt.normalized;

                _playerObj.forward = dirToCombatLookAt.normalized;
                break;
            }
        }
    }

    private void UpdateInputVector() {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
    }
}
