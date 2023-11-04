using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
  public MovementState _movementState;
  
  private PlayerMove _playerMove;

  [SerializeField] private LayerMask _groundMask;
  
  [Header("Keybinds")] 
  public KeyCode _jumpKey = KeyCode.Space;
  public KeyCode _sprintKey = KeyCode.LeftShift;
  public KeyCode _crouchKey = KeyCode.LeftControl;
  
  public enum MovementState
  {
    freeze,
    walking,
    sprinting,
    crouching,
    sliding,
    air
  }

  private void Start() {
    _playerMove = GetComponent<PlayerMove>();
  }

  private void Update() {
    UpdateMovementState();
  }

  private void UpdateMovementState() {
    Ray groundCheckRay = new Ray(transform.position, Vector3.down);
    if (!Physics.Raycast(groundCheckRay, 1.2f, _groundMask)) 
      _movementState = MovementState.air;
    else if (Input.GetKey(_sprintKey)) 
      _movementState = MovementState.sprinting;
    else 
      _movementState = MovementState.walking;
  }
}
