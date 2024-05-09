using _Scripts._Entities.Creatures;
using Unity.VisualScripting;
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

  [SerializeField] private Material _passiveMaterial;
  [SerializeField] private Material _attackingMaterial;

  [SerializeField] private Renderer[] _meshRenderers;

  [SerializeField] private GameObject _activeCollider;
  [SerializeField] private GameObject _inactiveCollider;

  [SerializeField] private Rigidbody[] _rigidbodies;

  private float _attackTimer;
  private bool _dead;

  private bool _attacking;

  private float _lastSpeed;

  private void Start() {
    _playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
  }

  private void FixedUpdate() {
    if (_dead) return;
    Vector3 direction = _playerTarget.position - transform.position + Vector3.up * 0.75f;

    Quaternion upRotation = Quaternion.FromToRotation(transform.up, (direction + Vector3.up * 25).normalized);

    // Always look at player by rotating on the free plane
    Quaternion forwardRotation = Quaternion.FromToRotation(transform.right, Vector3.ProjectOnPlane(_playerTarget.transform.position - transform.position, transform.up));
    
    // Finalizing and slerping rotation for smoothness
    Quaternion finalRotation = forwardRotation * upRotation * transform.rotation;
    float lerpFactor = (180 - Vector3.Angle(transform.forward, finalRotation * Vector3.forward)) / 180;
    Quaternion finalRotationSlerped = Quaternion.Slerp(transform.rotation, finalRotation,
      Time.fixedDeltaTime * _rotationSpeed * lerpFactor);

    transform.rotation = finalRotationSlerped;

    float speed = _speedCurve.Evaluate((direction.magnitude - _stopDistance) / _maxSpeedDistance) * _maxSpeed;
    _lastSpeed = speed;

    _rigidbody.AddForce(direction.normalized * Time.fixedDeltaTime * speed);

    // Attempt to attack player
    if (direction.magnitude < _attackDistance) {
      if (!_attacking) {
        _attacking = true;
        foreach (Renderer renderer in _meshRenderers) {
          renderer.material = _attackingMaterial;
        }
      }
      if (_attackTimer > _attack._attackRepeatSeconds) {
        _combat.Attack(_attack);
        _attackTimer = 0;
      }
    } else {
      if (_attacking) {
        _attacking = false;
        foreach (Renderer renderer in _meshRenderers) {
          renderer.material = _passiveMaterial;
        }
      }
    }
    
    _attackTimer += Time.fixedDeltaTime;
  }

  public void Neutralized() {
    _deathParticles.Play();
    _rigidbody.isKinematic = true;
    GetComponent<Collider>().enabled = false;

    _activeCollider.SetActive(false);
    _inactiveCollider.SetActive(true);
    foreach (Rigidbody rb in _rigidbodies) {
      rb.AddExplosionForce(500, transform.position, 5);
    }
    _dead = true;
    StartCoroutine(DestroyDrone());
  }

  private System.Collections.IEnumerator DestroyDrone() {
    yield return new WaitForSeconds(8);
    GetComponent<Collider>().enabled = false;
    yield return new WaitForSeconds(2);
    Destroy(gameObject);
  }

  private void OnCollisionEnter(Collision other) {
    if (other.gameObject.TryGetComponent(out Rigidbody rb)) {
      if (Mathf.Abs(rb.velocity.magnitude) + Mathf.Abs(_rigidbody.velocity.magnitude) > (_maxSpeed * 0.25f)) Neutralized();
    } else if (_lastSpeed / _maxSpeed > 0.25f) Neutralized();
  }

}