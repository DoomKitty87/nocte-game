using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class NewMovement : MonoBehaviour
{
    [FormerlySerializedAs("orientation")]
    [Header("References")] 
    [SerializeField] private Transform _orientation;
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _playerObj;
    private Rigidbody _rb;

    // General Variables
    private Vector3 _moveDirection;
    
    // Rotation variables
    [SerializeField] float _rotationSpeed;

    // Movement variables
    [SerializeField] private float _moveSpeed;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    private void Awake() {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Start() {
        _rb.freezeRotation = true;
    }

    private void FixedUpdate() {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
        
        UpdateInputVector();
        UpdateRotation();
        MovePlayer();
    }

    private void UpdateRotation() {
        var playerPosition = _player.position;
        var cameraPosition = transform.position;
        Vector3 viewDir = cameraPosition - new Vector3(cameraPosition.x, playerPosition.y, cameraPosition.z);
        _orientation.forward = viewDir.normalized;
        Debug.Log(viewDir.normalized);
        

        if (_moveDirection != Vector3.zero)
            _playerObj.forward = Vector3.Slerp(_playerObj.forward, _moveDirection.normalized, Time.deltaTime * _rotationSpeed);
    }

    private void UpdateInputVector() {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer() {
        _rb.AddForce(_moveSpeed * 10f * _moveDirection.normalized, ForceMode.Force);
    }
}
