using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
  [SerializeField] private CinemachineCamera _jetCamera;
  [SerializeField] private float _throttleIncrement = 0.1f;
  [SerializeField] private float _maxThrust = 200f;
  [SerializeField] private float _responsiveness = 10f;
  [SerializeField] private float _rollModifier = 0.5f;

  public Transform _playerSeat;

  private bool _inUse = false;

  private float _throttle;
  private float _pitch;
  private float _yaw;
  private float _roll;

  [SerializeField] private Rigidbody _rigidbody;
  [SerializeField] private MeshCollider _collider;

  [SerializeField] private GameObject _exhaust;

  private PlayerInput _input;

  private float _responseModifier {
    get {
      return (_rigidbody.mass / 10f) * _responsiveness;
    }
  }

  void Start() {
    _input = InputReader.Instance.PlayerInput;
    _jetCamera.enabled = false;
  }

  public void EnterVehicle() {
    _collider.convex = true;
    _rigidbody.isKinematic = false;
    _inUse = true;
    _exhaust.SetActive(true);
    CancelInvoke("DisableRigidBody");
    _jetCamera.enabled = true;
  }

  public void ExitVehicle() {
    _inUse = false;
    _exhaust.SetActive(false);
    Invoke("DisableRigidBody", 10f);
    _jetCamera.enabled = false;
  }

  private void DisableRigidBody() {
    _rigidbody.isKinematic = true;
    _collider.convex = false;
  }

  private void Update() {
    if (_inUse) HandleInput();
  }

  private void FixedUpdate() {
    if (_inUse) HandlePhysics();
  }

  private void HandlePhysics() {
    _rigidbody.AddRelativeForce(Vector3.forward * _throttle * _maxThrust);
    _rigidbody.AddRelativeTorque(Vector3.right * _pitch * _responseModifier);
    _rigidbody.AddRelativeTorque(Vector3.up * _yaw * _responseModifier);
    _rigidbody.AddRelativeTorque(-Vector3.forward * _roll * _responseModifier * _rollModifier);

    _rigidbody.AddForce(_rigidbody.velocity.normalized * _rigidbody.velocity.sqrMagnitude * -0.01f);

    //_rigidbody.AddRelativeForce(Vector3.up * _lift * _rigidbody.velocity.magnitude);
    //_rigidbody.AddRelativeForce(Vector3.up * _constantLift);
  }

  private void HandleInput() {
    _throttle = Mathf.Clamp(_throttle + _input.Flying.Throttle.ReadValue<float>() * _throttleIncrement, 0f, 1f);
    _pitch = _input.Flying.Pitch.ReadValue<float>();
    _yaw = _input.Flying.Yaw.ReadValue<float>();
    _roll = _input.Flying.Roll.ReadValue<float>();
  }

}
