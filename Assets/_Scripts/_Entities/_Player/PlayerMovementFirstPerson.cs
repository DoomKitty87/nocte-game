using System;
using UnityEngine;

public class PlayerMovementFirstPerson : MonoBehaviour
{
  public LayerMask groundMask;
  public Transform _model;
  
  public float MoveSmoothSpeed;
  public float GravityStrenght;
  public float JumpStrength;
  public float WalkSpeed;
  public float RunSpeed;

  private CharacterController _characterController;
  private Vector3 CurrentMoveVelocity;
  private Vector3 MoveDampVelocity;

  private Vector3 CurrentForceVelocity;

  private void Start() {
    _characterController = GetComponent<CharacterController>();
  }

  private void Update() {
    Vector3 playerInput = new Vector3 {
      x = Input.GetAxisRaw("Horizontal"),
      y = 0f,
      z = Input.GetAxisRaw("Vertical")
    };
    
    if (playerInput.magnitude > 1) playerInput.Normalize();

    Vector3 MoveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
    float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed;

    CurrentMoveVelocity = Vector3.SmoothDamp(
      CurrentMoveVelocity,
      MoveVector * currentSpeed,
      ref MoveDampVelocity,
      MoveSmoothSpeed
    );

    _characterController.Move(CurrentMoveVelocity * Time.deltaTime);

    Ray groundCheckRay = new Ray(transform.position, Vector3.down);
    if (Physics.Raycast(groundCheckRay, 1.25f, groundMask)) {
      CurrentForceVelocity.y = -2f;

      if (Input.GetKey(KeyCode.Space)) {
        CurrentForceVelocity.y = JumpStrength;
      }
    }
    else {
      CurrentForceVelocity.y -= GravityStrenght * Time.deltaTime;
    }

    _characterController.Move(CurrentForceVelocity * Time.deltaTime);
  }
}
