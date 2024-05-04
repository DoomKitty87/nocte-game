using UnityEngine;

public class DroneAI : MonoBehaviour
{

  private Transform _playerTarget;

  [SerializeField] private float _maxSpeed;
  [SerializeField] private float _maxSpeedDistance;

  [SerializeField] private AnimationCurve _speedCurve;

  [SerializeField] private float _rotationSpeed;

  [SerializeField] private float _stopDistance;

  private void Start() {
    _playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
  }

  private void Update() {
    Vector3 direction = _playerTarget.position - transform.position;
    Quaternion rotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * _rotationSpeed);
    float speed = _speedCurve.Evaluate((direction.magnitude - _stopDistance) / _maxSpeedDistance) * _maxSpeed;
    transform.position += transform.forward * Time.deltaTime * speed;
  }

}