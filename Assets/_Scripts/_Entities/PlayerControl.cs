using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class PlayerControl : MonoBehaviour
{

  [Header("References")] 
  [SerializeField] private Movement _movementScript;
  [SerializeField] private Transform _xMouseMovementTransform;
  [SerializeField] private Transform _yMouseMovementTransform;
  [Header("Controls")]
  [SerializeField] private float _mouseSensitivity = 30;

  private void OnValidate() {
    _movementScript = gameObject.GetComponent<Movement>();
  }

  private void Update() {
    _xMouseMovementTransform.transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime, Space.Self);
    _yMouseMovementTransform.transform.Rotate(Vector3.left, Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime, Space.Self);
    Vector3 direction = Vector3.zero;
    if (Input.GetAxisRaw("Horizontal") != 0) {
      direction.x = Input.GetAxisRaw("Horizontal");
    }
    if (Input.GetAxisRaw("Vertical") != 0) {
      direction.z = Input.GetAxisRaw("Vertical");
    }
    _movementScript.SetInputVector(direction);
    _movementScript.SetBoolInputs(Input.GetAxisRaw("Jump") > 0, Input.GetAxisRaw("Crouch") > 0);
  }
}
