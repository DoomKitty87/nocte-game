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
    UpdateMovementState();
  }

  private void UpdateMovementState() {
    if (State != "Freeze") {
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
    NotifyObservers(_previousState, State);
    _previousState = State;
    
  }
}

