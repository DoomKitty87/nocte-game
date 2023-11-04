using System;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
  public LayerMask groundMask;
  public Transform _model;
  
  [SerializeField] private float _moveSmoothSpeed;
  [SerializeField] private float _gravityStrengh;
  [SerializeField] private float _jumpStrength;
  [SerializeField] private float _walkSpeed;
  [SerializeField] private float _runSpeed;

  private PlayerMovementHandler _handler;
  private CharacterController _characterController;
  private Vector3 _currentMoveVelocity;
  private Vector3 _moveDampVelocity;

  private Vector3 _currentForceVelocity;
  private float _gravityVelocity;

  private void Start() {
    _characterController = GetComponent<CharacterController>();
    _handler = GetComponent<PlayerMovementHandler>();

    // Application.targetFrameRate = 10;
  }

  private void Update() {

    GetMovementVelocity();
    _characterController.Move(_currentMoveVelocity * Time.deltaTime);
    
    CalculateGravityVelocity();
    _characterController.Move(new Vector3(0, _gravityVelocity, 0) * Time.deltaTime);

    GetJump();
    _characterController.Move(_currentForceVelocity * Time.deltaTime);
  }
  
  private void GetMovementVelocity() {
    Vector3 playerInput = new Vector3 {
      x = Input.GetAxisRaw("Horizontal"),
      y = 0f,
      z = Input.GetAxisRaw("Vertical")
    };
    
    if (playerInput.magnitude > 1) playerInput.Normalize();

    Vector3 moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
    float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? _runSpeed : _walkSpeed;

    _currentMoveVelocity = Vector3.SmoothDamp(
      _currentMoveVelocity,
      moveVector * currentSpeed,
      ref _moveDampVelocity,
      _moveSmoothSpeed
    );
  }

  private void CalculateGravityVelocity() {
    if (_handler._movementState == PlayerMovementHandler.MovementState.air) {
      // Equation for gravity: strength * seconds^2, so must multiply by Time.deltaTime twice
      _gravityVelocity -= _gravityStrengh * Time.deltaTime;
    }
    else
      _gravityVelocity = 0;
  }

  private void GetJump() {
    if (_handler._movementState != PlayerMovementHandler.MovementState.air) {
      if (Input.GetKey(_handler._jumpKey)) {
        _currentForceVelocity.y = _jumpStrength;
      }
      else {
        _currentForceVelocity.y = 0;
      }
    }
  }
}
