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

  [SerializeField] private Rigidbody _rigidbody;

  [SerializeField] private ParticleSystem _deathParticles;

  private float _attackTimer;
  private bool _dead;

  private void Start() {
    _playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
  }

  private void FixedUpdate() {
    if (_dead) return;
    Vector3 direction = _playerTarget.position - transform.position + Vector3.up * 0.5f;
    Quaternion rotation = Quaternion.LookRotation(direction);

    float rotSpeed = _speedCurve.Evaluate(Vector3.Angle(transform.forward, direction) / 180);

    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * _rotationSpeed * rotSpeed);

    float speed = _speedCurve.Evaluate((direction.magnitude - _stopDistance) / _maxSpeedDistance) * _maxSpeed;

    _rigidbody.AddForce(direction.normalized * Time.fixedDeltaTime * speed);

    // Attempt to attack player
    if (direction.magnitude < _attackDistance && _attackTimer > _attack._attackRepeatSeconds) {
      _combat.Attack(_attack);
      _attackTimer = 0;
    }

    _attackTimer += Time.fixedDeltaTime;
  }

  public void Neutralized() {
    _deathParticles.Play();
    _rigidbody.useGravity = true;
    _dead = true;
    StartCoroutine(DestroyDrone());
  }

  private System.Collections.IEnumerator DestroyDrone() {
    yield return new WaitForSeconds(8);
    GetComponent<Collider>().enabled = false;
    yield return new WaitForSeconds(2);
    Destroy(gameObject);
  }

}