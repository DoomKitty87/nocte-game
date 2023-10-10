using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class PlayerControl : MonoBehaviour
{

  [Header("References")] 
  [SerializeField] private Transform _transformAlignToMovement;
  [SerializeField] private float _alignSpeed = 12;
  [Range(0, 180)][SerializeField] private float _snapInsteadAngle = 5;
  [SerializeField] private Movement _movementScript;
  [SerializeField] private Transform _xMouseMovementTransform;
  [SerializeField] private Transform _yMouseMovementTransform;
  [Header("Controls")]
  [SerializeField] private float _mouseSensitivity = 30;

  public float _smoothingFactor;
  private float _currX, _currY;
  private float _desX, _desY;

  private void RotateMouseTransforms() {
    _desX += Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime;
    _desY -= Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime;
    _currX = Mathf.Lerp(_currX, _desX, _smoothingFactor * Time.deltaTime);
    _currY = Mathf.Lerp(_currY, _desY, _smoothingFactor * Time.deltaTime);
    _xMouseMovementTransform.transform.localRotation = Quaternion.Euler(0, _currX, 0);
    _yMouseMovementTransform.transform.localRotation = Quaternion.Euler(_currY, 0, 0);
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

  private float ConvertTo360(float angle) {
    if (angle < 0) {
      return angle + 360;
    }
    else {
      return angle;
    }
  }
  
  private void AlignToMovement() {
    Quaternion target = Quaternion.LookRotation(GetInputVector(), Vector3.up);
    Quaternion current = _transformAlignToMovement.transform.rotation;
    float targetY = ConvertTo360(target.eulerAngles.y);
    float currentY = ConvertTo360(current.eulerAngles.y);
    if (targetY - currentY > _snapInsteadAngle) {
      Debug.Log($"Lerped: {currentY}, {targetY}");
      _transformAlignToMovement.transform.rotation = Quaternion.Slerp(current, target, _alignSpeed);
    }
    else {
      Debug.Log($"Snapped: {currentY}, {targetY}");
      _transformAlignToMovement.transform.rotation = target;
    }
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
}
