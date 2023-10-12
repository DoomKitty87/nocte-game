using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class PlayerControl : MonoBehaviour
{

  [Header("References")] 
  [SerializeField] private Transform _transformAlignToMovement;
  [SerializeField] private float _alignSpeed = 0.5f;
  [Range(0, 180)][SerializeField] private float _snapInsteadLessThanAngle = 15;
  [SerializeField] private Movement _movementScript;
  [SerializeField] private Transform _xMouseMovementTransform;
  [SerializeField] private Transform _yMouseMovementTransform;
  [Header("Controls")]
  [SerializeField] private float _mouseSensitivity = 30;
  
  private float _desX, _desY;

  private void RotateMouseTransforms() {
    _desX += Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime;
    _desY -= Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime;
    _xMouseMovementTransform.transform.localRotation = Quaternion.Euler(0, _desX, 0);
    _yMouseMovementTransform.transform.localRotation = Quaternion.Euler(_desY, 0, 0);
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
    if (Mathf.Abs(Quaternion.Angle(target, current)) > _snapInsteadLessThanAngle) {
      _transformAlignToMovement.transform.rotation = Quaternion.Lerp(current, target, _alignSpeed);
    }
    else {
      _transformAlignToMovement.transform.rotation = target;
    }
  }
  
  private void OnValidate() {
    _movementScript = gameObject.GetComponent<Movement>();
  }

  private void Update() {
    RotateMouseTransforms();
    _movementScript.SetInputVector(GetInputVector());
    _movementScript.SetBoolInputs(Input.GetAxisRaw("Jump") > 0, Input.GetAxisRaw("Crouch") > 0);
  }
  private void FixedUpdate() {
    if (GetInputVector() != Vector3.zero) AlignToMovement();
  }

}
