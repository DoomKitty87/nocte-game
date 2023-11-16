using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovementElliotDeprecated : MonoBehaviour
{

  [SerializeField] private float _mouseSensitivity;
  [SerializeField] private float _speed;
  [SerializeField] private float _maxDeltaRotationPerFrame;

  [SerializeField] private Transform _cameraTarget;
  private Rigidbody rb;

  private Vector3 direction;
  private float rotationX;
  private float rotationY;
  private float previousRotationDelta;

  // Helper function
  // -1 indicates Vector2 = (0, 0)
  private float Vector2ToDegrees(Vector2 vector) {
    if (vector == Vector2.zero) return -1;
    float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

    angle = -angle + 90; // Offset value
    
    // Ensure the angle is between 0 and 360 degrees
    if (angle < 0)
    {
      angle += 360;
    }
    
    return angle;
  }
  
  private void Awake() {
    rb = gameObject.GetComponent<Rigidbody>();
  }
  
  private void FixedUpdate() {
    rb.AddForce(Quaternion.Euler(0, _cameraTarget.eulerAngles.y, 0) * direction * _speed);
    
    direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    
    DoRotation();

    MatchRotations(previousRotationDelta);
  }

  private void DoRotation() {
    float mouseX = Input.GetAxis("Mouse X");
    float mouseY = Input.GetAxis("Mouse Y");

    // This is Y rotation, no idea why it takes Mouse X
    _cameraTarget.Rotate(mouseX * _mouseSensitivity * Vector3.up);

    rotationX -= mouseY * _mouseSensitivity;
    rotationX = Mathf.Clamp(rotationX, -70.0f, 70.0f);

    _cameraTarget.rotation = Quaternion.Euler(rotationX, _cameraTarget.eulerAngles.y, 0f);
  }

  private void MatchRotations(float previousRotationVelocity) {
    if (Mathf.Abs(Vector3.Distance(rb.velocity, Vector3.zero)) < 10f) return; // 'Deadzone' for player to start rotating
    float currentYRotation = transform.rotation.eulerAngles.y;
    float targetYRotation;
    
    Vector3 velocity = rb.velocity;
    targetYRotation = Vector2ToDegrees(new Vector2(velocity.x, velocity.z));
    float stashedCameraYRotation = _cameraTarget.eulerAngles.y;

    if (targetYRotation == -1) return; // If velocity is equal to 0 return
    if (currentYRotation == targetYRotation) return; // If already looking in correct direction return
    
    // Lerping towards target location
    // TODO: Make this a quadratic lerp function
    if (Mathf.Abs(currentYRotation - targetYRotation) < _maxDeltaRotationPerFrame) currentYRotation = targetYRotation;
    else {
      // Corrects for values over 360 degrees
      if (Mathf.Abs(currentYRotation - targetYRotation) > Mathf.Abs((currentYRotation + 360) - targetYRotation))
        currentYRotation += 360;
      
      // Moves currentYRotation towards targetYRotation
      if (currentYRotation > targetYRotation) {
        currentYRotation -= _maxDeltaRotationPerFrame;
      }
      else {
        currentYRotation += _maxDeltaRotationPerFrame;
      }
    }
    
    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, currentYRotation, 0f);
    _cameraTarget.rotation = Quaternion.Euler(_cameraTarget.eulerAngles.x, stashedCameraYRotation, 0f);
  }
}
