using System;
using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
  [SerializeField] MovementState _movementState;

  public float distance;
  public float radius;
  
  public string State {
    get => _movementState.ToString();
    set => _movementState = Enum.Parse<MovementState>(value);
  }
  
  [SerializeField] private LayerMask _groundMask;
  
  [Header("Keybinds")] 
  public KeyCode _jumpKey = KeyCode.Space;
  public KeyCode _sprintKey = KeyCode.LeftShift;
  public KeyCode _crouchKey = KeyCode.LeftControl;
  public KeyCode _slideKey = KeyCode.LeftControl;
  
  private enum MovementState
  {
    freeze,
    walking,
    sprinting,
    crouching,
    sliding,
    air
  }

  private void Update() {
    UpdateMovementState();
  }

  private void UpdateMovementState() {
    // TODO: Improve ground check
    // Sphere cast? More raycasts?
    Ray groundCheckRay = new Ray(transform.position + (Vector3.down * 0.6f), Vector3.down);
    //if (!Physics.Raycast(groundCheckRay, 1.25f, _groundMask)) 
    if (!Physics.SphereCast(groundCheckRay, 0.25f, 0.65f, _groundMask))
      State = "air";
    else if (Input.GetKey(_sprintKey))
      State = "sprinting";
    else if (Input.GetKey(_crouchKey))
      State = "crouching";
    else
      State = "walking";
  }

  private void OnDrawGizmos() {
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(transform.position + (Vector3.down * distance), radius);
  }
}
