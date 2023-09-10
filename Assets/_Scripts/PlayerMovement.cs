using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
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
  [SerializeField] private Vector3 _groundCheckOffset;
  [SerializeField] private string _groundTag = "Ground";
  [Header("Settings")]
  [SerializeField] private float _maxMoveSpeed = 6;
  [SerializeField] private float _moveAccel = 2;
  [SerializeField] private float _moveFriction = 0.5f;
  [SerializeField] private float _moveDrag = 5;
  [SerializeField] private float _slideSpeed = 8;
  [SerializeField] private float _slideFriction = 0f;
  [SerializeField] private float _slideForceExitSpeed = 0.1f;
  [SerializeField] private float _slideDrag = 0.1f;
  [SerializeField] private float _jumpForce = 8;
  [SerializeField] private float _jumpMaxMoveSpeed = 6;
  [SerializeField] private float _jumpMoveAccel = 2;
  [SerializeField] private float _airDrag = 0;
  [SerializeField] private float _groundCheckSphereSize = 0.1f;
  [Header("Controls")]
  [SerializeField] private float _mouseSensitivity = 30;
  public enum MovementState
  {
    Walking,
    Dashing,
    Sliding,
    Falling,
  }
  [Header("Other")]
  public MovementState _moveState;
  
  [SerializeField] private int _delayGroundCheckAfterJumpUpdates = 8;
  private int _groundCheckDelay;
  [SerializeField] private bool _sliding;
  private bool _firstCrouchDown;
  
  
  private Vector3 GetInputDirectionVector() {
    Vector3 direction = Vector3.zero;
    if (Input.GetAxisRaw("Horizontal") != 0) {
      direction.x = Input.GetAxisRaw("Horizontal");
    }
    if (Input.GetAxisRaw("Vertical") != 0) {
      direction.z = Input.GetAxisRaw("Vertical");
    }
    return direction.normalized;
  }
  private void SetColliderFriction(float dynamicValue, float staticValue) {
    _colliderMaterial.dynamicFriction = dynamicValue;
    _colliderMaterial.staticFriction = staticValue;
  }
  private bool Grounded() {
    Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position + _groundCheckOffset, _groundCheckSphereSize);
    foreach (Collider foundCollider in colliders) {
      if (foundCollider.gameObject.CompareTag("Ground")) {
        return true;
      }
    }
    return false;
  }
  
  
  private void OnValidate() {
    _rigidbody = gameObject.GetComponent<Rigidbody>();
  }
  private void Start() {
    Cursor.lockState = CursorLockMode.Locked; 
    _firstCrouchDown = true;
    _groundCheckDelay = _delayGroundCheckAfterJumpUpdates;
  }
  private void Update() {
    _xMouseMovementTransform.transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime, Space.Self);
    _yMouseMovementTransform.transform.Rotate(Vector3.left, Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime, Space.Self);
    if (_moveState == MovementState.Walking) {
      
    }
    if (_moveState == MovementState.Dashing) {
      
    }
    if (_moveState == MovementState.Sliding) {
      
    }
    if (_moveState == MovementState.Falling) {
      
    }
  }
  private void FixedUpdate() {
    if (_moveState == MovementState.Walking) { // ------------
      
      if (!Grounded()) {
        _moveState = MovementState.Falling;
      }
      else if (Grounded() && Input.GetAxisRaw("Jump") > 0) {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _groundCheckDelay = _delayGroundCheckAfterJumpUpdates;
        _moveState = MovementState.Falling;
      }
      else if (Grounded() && Input.GetAxisRaw("Crouch") > 0) {
        _rigidbody.drag = _slideDrag;
        SetColliderFriction(_slideFriction, _colliderMaterial.staticFriction);
        _rigidbody.velocity = _rigidbody.velocity.normalized * _slideSpeed;
        _moveState = MovementState.Sliding;
      }
      else {
        _rigidbody.drag = _moveDrag;
        SetColliderFriction(_moveFriction, _colliderMaterial.staticFriction);
        _rigidbody.AddForce(transform.TransformDirection(GetInputDirectionVector()) * _moveAccel, ForceMode.VelocityChange);
        if (_rigidbody.velocity.magnitude >= _maxMoveSpeed) {
          _rigidbody.velocity = _rigidbody.velocity.normalized * _maxMoveSpeed;
        }
      }
      
    }
    if (_moveState == MovementState.Dashing) { // -----------
      
      if (_moveState != MovementState.Dashing) return;
      
    }
    if (_moveState == MovementState.Sliding) { // ----------
      
      if (!Grounded()) {
        _moveState = MovementState.Falling;
      }
      else {
        if (Input.GetAxisRaw("Crouch") == 0) {
          _moveState = MovementState.Walking;
        }
        if (_rigidbody.velocity.magnitude < _slideForceExitSpeed) {
          _moveState = MovementState.Walking;
        }
      }
      
    }
    if (_moveState == MovementState.Falling) { // -----------
      
      if (Grounded() && _groundCheckDelay <= 0) {
        if (Input.GetAxisRaw("Crouch") > 0) {
          _rigidbody.drag = _slideDrag;
          SetColliderFriction(_slideFriction, _colliderMaterial.staticFriction);
          _rigidbody.velocity = _rigidbody.velocity.normalized * _slideSpeed;
          _moveState = MovementState.Sliding;
        }
        else {
          _rigidbody.drag = _moveDrag;
          SetColliderFriction(_moveFriction, _colliderMaterial.staticFriction);
          _moveState = MovementState.Walking;
        }
      }
      else {
        _groundCheckDelay--;
        _rigidbody.drag = _airDrag;
        SetColliderFriction(_moveFriction, _colliderMaterial.staticFriction);
        _rigidbody.AddForce(transform.TransformDirection(GetInputDirectionVector()) * _jumpMoveAccel, ForceMode.VelocityChange);
        Vector3 velocityXZ = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        if (velocityXZ.magnitude >= _jumpMaxMoveSpeed) {
          velocityXZ = velocityXZ.normalized * _jumpMaxMoveSpeed;
          _rigidbody.velocity = new Vector3(velocityXZ.x, _rigidbody.velocity.y, velocityXZ.z);
        }
      }
      
    }
  }


  //
  // private void SlideCheck() {
  //   if (Input.GetAxisRaw("Crouch") > 0 && _grounded) {
  //     _sliding = true;
  //     if (_firstCrouchDown) {
  //       _rigidbody.velocity = _rigidbody.velocity.normalized * _slideSpeed;
  //       _firstCrouchDown = false;
  //     }
  //   }
  //   else {
  //     _sliding = false;
  //     _firstCrouchDown = true;
  //   }
  // }
  //

  // private void Update() {

  //   if (!Grounded()) _grounded = false;
  //   if (Input.GetAxisRaw("Jump") > 0 && _grounded) {
  //     
  //     _grounded = false; 
  //   }
  //   SlideCheck();
  // }
  // private void OnCollisionEnter(Collision other) {
  //   if (other.gameObject.CompareTag(_groundTag)) {
  //     _grounded = true;
  //     SlideCheck();
  //   }
  // }
  // private void OnCollisionStay(Collision other) {
  //   if (other.gameObject.CompareTag(_groundTag)) {
  //     _grounded = true;
  //   }
  // }

  //
  // private void SetXZVelocityDirToVector(Vector3 movementVector) {
  //   movementVector.Normalize();
  //   movementVector *= _moveSpeed;
  //   _rigidbody.velocity = new Vector3(movementVector.x, _rigidbody.velocity.y, movementVector.z);
  // }
  
  // private void UpdateDragAndFrictionType() {
  //   _rigidbody.drag = _grounded ? _groundDrag : _airDrag;
  //   if (_sliding) {
  //     SetColliderFriction(_slideFriction, _colliderMaterial.staticFriction);
  //     _rigidbody.drag = _slideDrag;
  //   }
  //   else {
  //     SetColliderFriction(_moveFriction, _colliderMaterial.staticFriction);
  //   }
  // }
  // private void FixedUpdate() {
  //   Vector3 worldInputDirection = transform.TransformDirection(GetInputDirectionVector());
  //   if (worldInputDirection != Vector3.zero) {
  //     if (!_sliding) SetXZVelocityDirToVector(worldInputDirection);
  //   }
  //   UpdateDragAndFrictionType();
  // }

  private void OnDrawGizmos() {
    Gizmos.color = Color.green;
    Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + _rigidbody.velocity);
    Gizmos.DrawSphere(gameObject.transform.position + _groundCheckOffset, _groundCheckSphereSize);
  }
}
