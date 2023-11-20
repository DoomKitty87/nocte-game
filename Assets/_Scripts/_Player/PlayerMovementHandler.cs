using System;
using UnityEngine;

using ObserverPattern;

public class PlayerMovementHandler : Subject
{
  [SerializeField] private Transform _groundCheck;

  [SerializeField] MovementState _movementState;

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

  private bool Grounded => IsGrounded();

  public bool OnSlope => IsOnSlope();
  
  private enum MovementState
  {
    Freeze,
    Walking,
    Sprinting,
    Crouching,
    Sliding,
    Air
  }

  private void Start() {
    _previousState = State;
  }

  private void Update() {
    UpdateState();
  }
  
  private bool IsGrounded() {
    return Physics.CheckSphere(_groundCheck.position, 0.5f, _groundMask);
  }

  private bool IsOnSlope() {
    return false;
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
}