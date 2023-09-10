using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Rigidbody _rigidbody;
  [SerializeField] private PhysicMaterial _colliderMaterial;
  [SerializeField] private Transform _xMouseMovementTransform;
  [SerializeField] private Transform _yMouseMovementTransform;
  [SerializeField] private string _groundTag = "Ground";
  [Header("Settings")]
  [FormerlySerializedAs("_speed")] 
  [SerializeField] private float _moveSpeed = 5;
  [SerializeField] private float _moveFriction = 0.6f;
  [SerializeField] private float _slideSpeed = 8;
  [SerializeField] private float _slideFriction = 0.1f;
  [SerializeField] private float _slideForceExitSpeed = 2;
  [SerializeField] private float _jumpForce = 2;
  [SerializeField] private float _groundDrag = 5;
  [SerializeField] private float _slideDrag = 0.5f;
  [SerializeField] private float _airDrag = 0;
  [Header("Controls")]
  [SerializeField] private float _mouseSensitivity = 30;
  
  [SerializeField] private bool _grounded;
  [SerializeField] private bool _sliding;
  private bool _firstCrouchDown;
  
  private void OnValidate() {
    _rigidbody = gameObject.GetComponent<Rigidbody>();
  }
  private void Start() {
    Cursor.lockState = CursorLockMode.Locked;
    _grounded = true;
    _sliding = false;
    _firstCrouchDown = true;
  }

  private void SlideCheck() {
    if (Input.GetAxisRaw("Crouch") > 0 && _grounded) {
      _sliding = true;
      if (_firstCrouchDown) {
        _rigidbody.velocity = _rigidbody.velocity.normalized * _slideSpeed;
        _firstCrouchDown = false;
      }
    }
    else {
      _sliding = false;
      _firstCrouchDown = true;
    }
  }
  private void Update() {
    _xMouseMovementTransform.transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime);
    _yMouseMovementTransform.transform.Rotate(Vector3.left, Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime);
    if (Input.GetAxisRaw("Jump") > 0 && _grounded) {
      _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
      _grounded = false; 
    }
    SlideCheck();
  }
  private void OnCollisionEnter(Collision other) {
    if (other.gameObject.CompareTag(_groundTag)) {
      _grounded = true;
      SlideCheck();
    }
  }
  private void OnCollisionStay(Collision other) {
    if (other.gameObject.CompareTag(_groundTag)) {
      _grounded = true;
    }
  }
  private Vector3 GetInputDirectionVector() {
    Vector3 direction = Vector3.zero;
    if (Input.GetAxisRaw("Horizontal") != 0) {
      direction.x = Input.GetAxisRaw("Horizontal");
    }
    if (Input.GetAxisRaw("Vertical") != 0) {
      direction.z = Input.GetAxisRaw("Vertical");
    }
    return direction;
  }

  private void SetXZVelocityDirToVector(Vector3 movementVector) {
    movementVector.Normalize();
    movementVector *= _moveSpeed;
    _rigidbody.velocity = new Vector3(movementVector.x, _rigidbody.velocity.y, movementVector.z);
  }
  private void SetColliderFriction(float dynamicValue, float staticValue) {
    _colliderMaterial.dynamicFriction = dynamicValue;
    _colliderMaterial.staticFriction = staticValue;
  }
  private void UpdateDragAndFrictionType() {
    _rigidbody.drag = _grounded ? _groundDrag : _airDrag;
    if (_sliding) {
      SetColliderFriction(_slideFriction, _colliderMaterial.staticFriction);
      _rigidbody.drag = _slideDrag;
    }
    else {
      SetColliderFriction(_moveFriction, _colliderMaterial.staticFriction);
    }
  }
  private void FixedUpdate() {
    Vector3 worldInputDirection = transform.TransformDirection(GetInputDirectionVector());
    if (worldInputDirection != Vector3.zero) {
      if (!_sliding) SetXZVelocityDirToVector(worldInputDirection);
    }
    UpdateDragAndFrictionType();
  }

  private void OnDrawGizmos() {
    Gizmos.color = Color.green;
    Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + _rigidbody.velocity);
  }
}
