using System;
using UnityEngine;

using ObserverPattern;
using UnityEngine.Serialization;

public class PlayerMovementHandler : Subject
{
  [SerializeField] private Transform _groundCheck;

  [SerializeField] private MovementState _movementState;

  private string _previousState;

  public string State {
    get => _movementState.ToString();
    set => _movementState = Enum.Parse<MovementState>(value, true);
  }

  [SerializeField] private LayerMask _groundMask;

  [Header("Keybinds")] 
  public KeyCode _jumpKey = KeyCode.Space;
  // public KeyCode _sprintKey = KeyCode.LeftShift;
  public KeyCode _crouchKey = KeyCode.LeftControl;
  public KeyCode _slideKey = KeyCode.LeftControl;

  private CharacterController _characterController;

  public bool Grounded => IsGrounded();
  public bool OnSteepSlope => IsOnSteepSlope();
  public bool IsSliding => Input.GetKey(_slideKey);

  public Vector3 _velocity;

  // Rider is mad at me, if you know how to fix this please do
  public Vector3 Velocity { get => _velocity; }

  private Vector3 _newAcceleration;
  private Vector3 _newEarlyVelocity;
  
  private enum MovementState
  {
    Freeze,
    Walking,
    Sprinting,
    Crouching,
    Sliding,
    Air
  }

  private void Awake() {
    _characterController = GetComponent<CharacterController>();
  }

  private void Start() {
    _previousState = State;
  }

  private void Update() {
    UpdateState();
    HandleGravity();
  }

  private void LateUpdate() {
    _characterController.Move((_characterController.velocity * Time.deltaTime) + _newAcceleration);
    Debug.Log("Velocity: " + Velocity + _newAcceleration);
    Debug.Log("Acceleration: " + _newAcceleration);
    
    _newAcceleration = Vector3.zero;
    _velocity = _characterController.velocity;
  }
  
  private bool IsGrounded() {
    return Physics.CheckSphere(_groundCheck.position, 0.5f, _groundMask);
  }

  private bool IsOnSteepSlope() {
    Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2);
    return (Vector3.Angle(hit.normal, Vector3.up) > _characterController.slopeLimit);
  }
  
  private void UpdateState() {
    if (State != "Freeze" && State != "Grapple") {
      if (!Grounded) {
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

  private void HandleGravity() {
    if (State == "Air") {
      AddForceContinuous(Vector3.down * 15f);
    }
  }

  public void AddForceContinuous(Vector3 newForce) {
    _newAcceleration += newForce * Time.deltaTime;
  }

  public void AddForceImpulse(Vector3 newForce) {
    _newAcceleration += newForce;
  }

  public void SetVerticalVelocity(float newForce) {
    _newAcceleration += (Vector3.up * newForce - new Vector3(0, Velocity.y, 0));
    Debug.Log(_newAcceleration);
  }

  public void SetHorizontalVelocity(Vector3 newForce) {
    _newAcceleration += newForce * Time.deltaTime - new Vector3(Velocity.x, 0, Velocity.z);
  }
  
  public void SetVelocityLate(Vector3 newVelocity) {
    _newAcceleration += newVelocity - Velocity;
  }}