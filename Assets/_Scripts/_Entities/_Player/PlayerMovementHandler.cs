using System;
using UnityEngine;

using ObserverPattern;

public class PlayerMovementHandler : Subject
{
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
  public KeyCode _grappleKey = KeyCode.Mouse0;

  public int _localGrappleType = -1;
  
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

  private void Start() {
    _previousState = State;
  }

  private void Update() {
    UpdateMovementState();
    CheckForGrapple();
  }

  private void UpdateMovementState() {
    if (State != "Freeze" && State != "Grapple") {
      var groundCheckRay = new Ray(transform.position + (Vector3.down * 0.6f), Vector3.down);
      if (!Physics.SphereCast(groundCheckRay, 0.25f, 0.65f, _groundMask))
        State = "Air";
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

  private void CheckForGrapple() {
    if (Input.GetKeyDown(_grappleKey) && _localGrappleType != 0) {
      _localGrappleType = 0;
      GrappleNotification(0);
    } 
    else if (Input.GetKey(_grappleKey) && _localGrappleType != 1) {
      _localGrappleType = 1;
      GrappleNotification(1);
    }
    else if (Input.GetKeyUp(_grappleKey) && _localGrappleType != 2) {
      _localGrappleType = 2;
      GrappleNotification(2);
    }
  }
}

