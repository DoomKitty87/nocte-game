using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class PlayerControl : MonoBehaviour
{

  [Header("References")] 
  [SerializeField] private Transform _transformAlignToMovement;
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

  private void AlignToMovement() {
    // _transformAlignToMovement.transform.rotation = Quaternion.LookRotation(GetInputVector(), Vector3.up);
  }
  
  private void OnValidate() {
    _movementScript = gameObject.GetComponent<Movement>();
  }

  private void Update() {
    RotateMouseTransforms();
    AlignToMovement();
    _movementScript.SetInputVector(GetInputVector());
    _movementScript.SetBoolInputs(Input.GetAxisRaw("Jump") > 0, Input.GetAxisRaw("Crouch") > 0);
  }
}
