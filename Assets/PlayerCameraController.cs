using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCameraController : MonoBehaviour
{

    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _orientation;
    
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float _lookSensitivityX = 2.0f;
    [SerializeField, Range(1, 10)] private float _lookSensitivityY = 2.0f;
    [SerializeField, Range(0, 10)] private float _sensitivityMultiplier = 1.0f;
    [SerializeField] private bool _invertMouseX = false;
    [SerializeField] private bool _invertMouseY = false;
    [SerializeField, Range(1, 180)] private float _upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float _lowerLookLimit = 80.0f;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        Look();
    }

    private float _xRotation;
    private float _desiredX;

    private void Look() {
        float mouseX = Input.GetAxis("Mouse X") * (_lookSensitivityX * 20) * Time.fixedDeltaTime * _sensitivityMultiplier * (_invertMouseX ? -1 : 1);
        float mouseY = Input.GetAxis("Mouse Y") * (_lookSensitivityY * 20) * Time.fixedDeltaTime * _sensitivityMultiplier * (_invertMouseY ? -1 : 1);

        Vector3 rot = _camera.localRotation.eulerAngles;
        _desiredX = rot.y + mouseX;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -_lowerLookLimit, _upperLookLimit);

        _camera.localRotation = Quaternion.Euler(_xRotation, _desiredX, 0);
        _orientation.localRotation = Quaternion.Euler(0, _desiredX, 0);
    }
}
