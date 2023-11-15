using System;
using UnityEngine;

using ObserverPattern;

public class PlayerMovementHandler : Subject
{
  [SerializeField] MovementState _movementState;
  private CharacterController _characterController;

  private string _previousState;

  public string State {
    get => _movementState.ToString();
    set => _movementState = Enum.Parse<MovementState>(value, true);
  }

  [SerializeField] private LayerMask _groundMask;

  [Header("Physics")] 
  [SerializeField] private float _frictionCoefficient;
  [SerializeField] private float _gravity;
  [SerializeField] private float _groundedGravity;
  public float _decayFactor;

  private Vector3 _accelerationVector;
  private Vector3 _velocityVector;
  public Vector3 _newVelocity;
  
  [Header("Keybinds")] 
  public KeyCode _jumpKey = KeyCode.Space;
  // public KeyCode _sprintKey = KeyCode.LeftShift;
  public KeyCode _crouchKey = KeyCode.LeftControl;
  public KeyCode _slideKey = KeyCode.LeftControl;
  public KeyCode _grappleKey = KeyCode.Mouse0;

  
  private enum MovementState
  {
    Freeze,
    Walking,
    Sprinting,
    Crouching,
    Sliding,
    Air,
    Grapple
  }

  private void Awake() {
    _characterController = GetComponent<CharacterController>();
  }
  
  private void Start() {
    _previousState = State;
  }

  private void LateUpdate() {
    _characterController.Move(_newVelocity * Time.deltaTime);
    _newVelocity = Vector3.zero;

    HandleGravity();
    UpdateState();
  }

  private void Update() {
    Debug.Log(_newVelocity.y);
  }

  private void HandleGravity() {
    if (!_characterController.isGrounded) {
      _newVelocity.y += _characterController.velocity.y + (_gravity * Time.deltaTime);
    }
    else {
      _newVelocity.y += _characterController.velocity.y + _groundedGravity;
    }
  }
  
  /*
  private void Update() {
    UpdateMovementState();

    CalculateGravity();
  }

  private void LateUpdate() {
    ApplyForce();
    _characterController.Move(_newVelocity * Time.deltaTime);
    _accelerationVector = Vector3.zero;
  }

  private void ApplyForce() {
    // _velocityVector = _characterController.velocity;
    // 
    // // Apply friction if grounded
    // if (State != "Air") {
    //   _velocityVector += new Vector3(CalculateFriction(_characterController.velocity.x), 0f,
    //     CalculateFriction(_characterController.velocity.z));
    // }
    // 
    _velocityVector += _accelerationVector;
    
    _newVelocity = _velocityVector;
    // Debug.Log(_characterController.velocity.magnitude);
  }
  
  public void AddForce(Vector3 forceVector) {
    _accelerationVector += forceVector;
  }

  private void CalculateGravity() {
    if (_characterController.isGrounded)
      _accelerationVector.y += _groundedGravity * Time.deltaTime;
    else 
      _accelerationVector.y += _gravity * Time.deltaTime;
  }
  
  private float CalculateFriction(float magnitude) {
    float magnitudeWithFriction = magnitude * _frictionCoefficient;
    return -magnitudeWithFriction * Time.deltaTime;
  }
  */
  
  private void UpdateState() {
    if (State != "Freeze" && State != "Grapple") {
      if (!_characterController.isGrounded) {
        State = "Air";
      }
      else if (Input.GetKey(_slideKey))
        State = "Sliding";
      //else if (Input.GetKey(_sprintKey))
      //  State = "Sprinting";
      else if (Input.GetKey(_crouchKey))
        State = "Crouching";
      else
        State = "Sprinting";
    }

    if (_previousState == State) return;
    NewSceneNotification(_previousState, State);
    _previousState = State;
    
  }
}

