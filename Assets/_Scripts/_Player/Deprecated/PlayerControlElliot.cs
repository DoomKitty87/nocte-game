using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MovementDeprecated))]
public class PlayerRotateElliot : MonoBehaviour
{
 
  [SerializeField] private float _mouseSensitivity;
  [SerializeField] private float _speed;
  [SerializeField] private float _maxDeltaRotationPerFrame;

  [SerializeField] private Transform _cameraTarget;
  private Rigidbody rb;
  private MovementDeprecated _movementScript;

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
    _movementScript = GetComponent<MovementDeprecated>();
  }
  
  private void FixedUpdate() {
    rb.AddForce(Quaternion.Euler(0, _cameraTarget.eulerAngles.y, 0) * direction * _speed, ForceMode.Impulse);
    //direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    
    _movementScript.SetInputVector(Quaternion.Euler(0, _cameraTarget.eulerAngles.y, 0) * direction);
    
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
      
      // Corrects for incorrect targetYRotation

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
/*
  [Header("References")] 
  [SerializeField] private Transform _transformAlignToMovement;
  [SerializeField] private float _alignSpeed = 0.5f;
  [Range(0, 180)][SerializeField] private float _snapInsteadLessThanAngle = 15;
  [SerializeField] private Movement _movementScript;
  [SerializeField] private Transform _xMouseMovementTransform;
  [SerializeField] private Transform _yMouseMovementTransform;
  [Header("Controls")]
  [SerializeField] private float _mouseSensitivity = 30;
  [SerializeField] private bool _invertY;
  
  private float _desX, _desY;

  private void RotateMouseTransforms() {
    _xMouseMovementTransform.transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * _mouseSensitivity);
    if (_invertY) {
      _yMouseMovementTransform.transform.Rotate(Vector3.left, Input.GetAxisRaw("Mouse Y") * _mouseSensitivity);
    }
    else {
      _yMouseMovementTransform.transform.Rotate(Vector3.right, Input.GetAxisRaw("Mouse Y") * _mouseSensitivity);
    }
  }
  
  private Vector3 GetInputVector() {
    Vector3 rawDirection = Vector3.zero;
    if (Input.GetAxisRaw("Horizontal") != 0) {
      rawDirection.x = Input.GetAxisRaw("Horizontal");
    }
    if (Input.GetAxisRaw("Vertical") != 0) {
      rawDirection.z = Input.GetAxisRaw("Vertical");
    }
    return _xMouseMovementTransform.TransformDirection(rawDirection);
  }
   
  private void AlignToMovement() {
    Quaternion target = Quaternion.LookRotation(GetInputVector(), Vector3.up);
    Quaternion current = _transformAlignToMovement.transform.rotation;
    _transformAlignToMovement.transform.rotation = target;
    // if (Mathf.Abs(Quaternion.Angle(target, current)) > _snapInsteadLessThanAngle) {
    //   _transformAlignToMovement.transform.rotation = Quaternion.Lerp(current, target, _alignSpeed);
    // }
    // else {
    //   _transformAlignToMovement.transform.rotation = target;
    // }
  }
  
  private void OnValidate() {
    _movementScript = gameObject.GetComponent<Movement>();
  }

  private void Update() {
    RotateMouseTransforms();
    _movementScript.SetInputVector(GetInputVector());
    if (GetInputVector() != Vector3.zero) AlignToMovement();
    _movementScript.SetBoolInputs(Input.GetAxisRaw("Jump") > 0, Input.GetAxisRaw("Crouch") > 0);
  }
*/
}
