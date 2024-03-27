using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCameraController : MonoBehaviour
{
    private PlayerInput _input;

    [SerializeField] private bool _freezeOnTimescale0;

    [HideInInspector] public bool _playerFreeze;
    
    [SerializeField] private Transform _cameraTarget;
    
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float _lookSensitivityX = 2.0f;
    [SerializeField, Range(1, 10)] private float _lookSensitivityY = 2.0f;
    [SerializeField, Range(1, 180)] private float _upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float _lowerLookLimit = 80.0f;

    private Quaternion _currentRotation;

    private Transform _parent;
    private bool _usingParent;
    private bool _useXClamp;
    private float _clampAngle;
    
    private void Start() {
        _input = InputReader.Instance.PlayerInput;

        PlayerController.Freeze += CameraFreeze;
        PlayerController.UnFreeze += CameraUnFreeze;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate() {
        if (_freezeOnTimescale0 && Time.timeScale == 0 || _playerFreeze) return;
        Look();
    }

    private float _xRotation;
    private float _desiredX;

    private void Look() {

        Vector2 lookDelta;
        if (_input.Player.Look.ReadValue<Vector2>() == Vector2.zero) lookDelta = _input.Driving.Look.ReadValue<Vector2>();
        else lookDelta = _input.Player.Look.ReadValue<Vector2>();

        float mouseX = lookDelta.x * _lookSensitivityX;
        float mouseY = lookDelta.y * _lookSensitivityY;

        Vector3 rot = _currentRotation.eulerAngles;
        _desiredX = rot.y + mouseX;

        if (_useXClamp) {
            if (_desiredX > 180)
                _desiredX = Mathf.Clamp(_desiredX, 360 - _clampAngle, 360 + _clampAngle);
            else
                _desiredX = Mathf.Clamp(_desiredX, 0 - _clampAngle, 0 + _clampAngle);
        }

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -_lowerLookLimit, _upperLookLimit);

        _currentRotation = Quaternion.Euler(_xRotation, _desiredX, 0);

        if (_usingParent)
            _cameraTarget.rotation = _parent.rotation * _currentRotation;
        else
            _cameraTarget.rotation = _currentRotation;
    }

    public void ResetRotation() {
        _xRotation = 0;
        _currentRotation = Quaternion.identity;
    }
    
    public void SetParent(Transform parent) {
        _parent = parent;
        _usingParent = true;
    }

    public void ResetParent() {
        _parent = null;
        _usingParent = false;
    }

    public void UseClamp(float angle) {
        _useXClamp = true;
        _clampAngle = angle;
    }

    public void ResetClamp() =>
        _useXClamp = false;

    private void CameraFreeze() =>
        _playerFreeze = true;

    private void CameraUnFreeze() =>
        _playerFreeze = false;
}
