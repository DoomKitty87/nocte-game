using _Scripts._Entities.Creatures;
using UnityEngine;

public class DroneAI : MonoBehaviour
{

  private Transform _playerTarget;

  [SerializeField] private float _maxSpeed;
  [SerializeField] private float _maxSpeedDistance;

  [SerializeField] private AnimationCurve _speedCurve;

  [SerializeField] private float _rotationSpeed;

  [SerializeField] private float _stopDistance;
  [SerializeField] private float _attackDistance;

  [SerializeField] private CreatureCombat _combat;

  [SerializeField] private CreatureAttack _attack;

  private float _attackTimer;

  private void Start() {
    _playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
  }

  private void Update() {
    Vector3 direction = _playerTarget.position - transform.position + Vector3.up * 0.5f;
    Quaternion rotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * _rotationSpeed);
    float speed = _speedCurve.Evaluate((direction.magnitude - _stopDistance) / _maxSpeedDistance) * _maxSpeed;
    transform.position += transform.forward * Time.deltaTime * speed;

    // Attempt to attack player
    if (direction.magnitude < _attackDistance && _attackTimer > _attack._attackRepeatSeconds) {
      _combat.Attack(_attack);
      _attackTimer = 0;
    }

    _attackTimer += Time.deltaTime;
  }

  public void Neutralized() {
    Destroy(gameObject);
  }

}