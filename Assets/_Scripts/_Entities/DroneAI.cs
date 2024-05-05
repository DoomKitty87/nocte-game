using _Scripts._Entities.Creatures;
using UnityEngine;

public class DroneAI : MonoBehaviour
{

  private Transform _playerTarget;
  
  [SerializeField] private float _stopDistance;
  [SerializeField] private float _attackDistance;
  [SerializeField] private float _rotationSpeed;
  [SerializeField] private float _force;
  [SerializeField] private float _distanceFromTargetFalloff; // Drone will 'hang' close to the target position in order to prevent jittering and turning upside down
  
  [SerializeField] private AnimationCurve speedDistanceFalloff;

  [SerializeField] private CreatureCombat _combat;
  [SerializeField] private CreatureAttack _attack;

  private Rigidbody _rb;

  [SerializeField] private ParticleSystem _deathParticles;

  private float _attackTimer;
  private bool _dead;
  
  private Vector3 _moveTowards;
  
  private void Start() {
    _playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
    _rb = GetComponent<Rigidbody>();

  }
  
  private void FixedUpdate() {
    if (_dead) return;
    
    // Temporary, here add attack patters, patrolling, etc
    // Realistically moveTowards should never be the player position exactly as this could lead to stuttering once it reaches player position.
    _moveTowards = _playerTarget.position + Vector3.up * 3f;
    // Possibly add obstacle avoidance?
    
    var t = transform;
        
    Vector3 velocity = _rb.velocity;
    Vector3 acceleration = Vector3.zero;
        
    acceleration.y -= 9.8f * Time.fixedDeltaTime; // Gravity

    Vector3 targetDirection = _moveTowards - (t.position + velocity + acceleration);
    Quaternion upRotation = Quaternion.FromToRotation(t.up, targetDirection.normalized);

    // Always look at player by rotating on the free plane
    Quaternion forwardRotation = Quaternion.FromToRotation(t.right, Vector3.ProjectOnPlane(_playerTarget.transform.position - t.position, t.up));

    // Slow down the drone once its nearing its location using both speedDistanceFalloff and distanceFromTargetFalloff
    float localForce = _force;
    if (targetDirection.magnitude < _distanceFromTargetFalloff)
      localForce *= speedDistanceFalloff.Evaluate((_distanceFromTargetFalloff - targetDirection.magnitude) / _distanceFromTargetFalloff);
    
    // Apply force
    acceleration += t.up * (localForce * Time.fixedDeltaTime);
    
    // Finalizing and slerping rotation for smoothness
    Quaternion finalRotation = forwardRotation * upRotation * t.rotation;
    float lerpFactor = (180 - Vector3.Angle(t.forward, finalRotation * Vector3.forward)) / 180;
    Quaternion finalRotationSlerped = Quaternion.Slerp(t.rotation, finalRotation,
      Time.fixedDeltaTime * _rotationSpeed * lerpFactor);
    
    // Here I try to clamp the Z (pitch) to prevent turning upside down and other weird movements, but this ended up making the drone a buggy mess
    
    // Vector3 eulerRotation = finalRotationSlerped.eulerAngles;
    // Debug.Log(eulerRotation.z - 360);
    // Quaternion clampedRotation = Quaternion.Euler(
    //   new Vector3(eulerRotation.x, eulerRotation.y, eulerRotation.z) //Mathf.Clamp(eulerRotation.z, 360 - 60, Mathf.Infinity))
    // );
    
    t.rotation = finalRotationSlerped;
        
    velocity += acceleration;
    _rb.velocity = velocity;

    // Attempt to attack player
    // Does this work? Idk if it ever worked.
    if (targetDirection.magnitude < _attackDistance && _attackTimer > _attack._attackRepeatSeconds) {
      _combat.Attack(_attack);
      _attackTimer = 0;
    }

    _attackTimer += Time.fixedDeltaTime;
  }

  // Ansel's old update function
  // private void FixedUpdate() {
  //   if (_dead) return;
  //   Vector3 direction = _playerTarget.position - transform.position + Vector3.up * 0.5f;
  //   Quaternion rotation = Quaternion.LookRotation(direction);
  // 
  //   float rotSpeed = _speedCurve.Evaluate(Vector3.Angle(transform.forward, direction) / 180);
  // 
  //   transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * _rotationSpeed * rotSpeed);
  // 
  //   float speed = _speedCurve.Evaluate((direction.magnitude - _stopDistance) / _maxSpeedDistance) * _maxSpeed;
  // 
  //   _rigidbody.AddForce(direction.normalized * Time.fixedDeltaTime * speed);
  // 
  //   // Attempt to attack player
  //   if (direction.magnitude < _attackDistance && _attackTimer > _attack._attackRepeatSeconds) {
  //     _combat.Attack(_attack);
  //     _attackTimer = 0;
  //   }
  // 
  //   _attackTimer += Time.fixedDeltaTime;
  // }

  public void Neutralized() {
    _deathParticles.Play();
    _rb.useGravity = true;
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