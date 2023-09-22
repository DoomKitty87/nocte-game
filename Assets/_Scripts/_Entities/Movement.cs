using System;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Serialization;
// ===============================================================
// Matthew
// Desc: Handles movement (with sliding, jumping, etc) for
//       player object, highly configurable
// ===============================================================
[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Rigidbody _rigidbody;
  [SerializeField] private PhysicMaterial _colliderMaterial;
  [SerializeField] private Vector3 _groundCheckOffset;
  [SerializeField] private string _groundTag = "Ground";

  [Header("Script Interfaces")] 
  [SerializeField] private Vector3 _inputVector;
  // Set vector to move towards, emulates wasd
  public void SetInputVector(Vector3 inputVector) {
    _inputVector = inputVector.normalized;
  }
  [SerializeField] private bool _jumpInput;
  [SerializeField] private bool _slideInput;
  // Emulates Crouch / Jump RawAxis
  public void SetBoolInputs(bool jumpInput, bool slideInput) {
    _jumpInput = jumpInput;
    _slideInput = slideInput;
  }
  
  [Header("Settings")]
  [SerializeField] private float _maxMoveSpeed = 10;
  [SerializeField] private float _maxMoveSpeedInit = 5;
  [SerializeField] private float _moveTimeInitToMax = 2;
  [SerializeField] private float _moveAccel = 2;
  [SerializeField] private float _moveFriction = 0.5f;
  [SerializeField] private float _moveDrag = 5;
  [SerializeField][Range(0, 180)] private float _maxDownwardsSlopeAngle = 45;
  [SerializeField] private float _slideStartSpeed = 20;
  [SerializeField] private float _slideFriction = 0.1f;
  [SerializeField] private float _slideExitSpeed = 9;
  [SerializeField] private float _slideForceExitSpeed = 0;
  [SerializeField] private float _slideMinSpeed = 9.8f;
  [SerializeField] private float _slideDrag = 0.2f;
  [SerializeField] private float _jumpForce = 8;
  [SerializeField] private float _jumpMaxMoveSpeed = 4;
  [SerializeField] private float _jumpMoveAccel = 0.5f;
  [SerializeField] private float _airDrag = 0;
  [SerializeField] private float _groundCheckSphereSize = 0.1f;
  

  public enum MovementState
  {
    Walking,
    Dashing,
    Sliding,
    Falling,
  }
  public MovementState _moveState;
  
  [Header("Other")]
  [SerializeField] private WorldGen _procGen;
  [SerializeField] private int _delayGroundCheckAfterJumpUpdates = 8;
  private int _groundCheckDelay;
  private Vector3 _lastInputVector;
  [SerializeField] private float _timeSinceLastDirectionChange;

  // For gizmos testing of jump accel only
  private Vector3 _accel;
  
  private void SetColliderFriction(float dynamicValue, float staticValue) {
    _colliderMaterial.dynamicFriction = dynamicValue;
    _colliderMaterial.staticFriction = staticValue;
  }
  private bool Grounded() {
    Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position + _groundCheckOffset, _groundCheckSphereSize);
    return colliders.Any(foundCollider => foundCollider.gameObject.CompareTag(_groundTag));
  }

  private Vector3 GetVelocityXZ() {
    return new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
  }
  
  // Gets slope angle by finding inverse tangent of downwards raycast magnitude and hit point normal (without y factor)
  // Gets slope direction from the hit point normal without y factor
  private (Vector3, float) SlopeParallelDirAndAngle() {
    Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit);
    Vector3 xzNormal = new Vector3(hit.normal.x, 0, hit.normal.z);
    return (xzNormal, Mathf.Atan2(xzNormal.magnitude, hit.distance) * 180 / Mathf.PI);
  }

  private Vector3 ModifyVectorBySlope(Vector3 input) {
    (Vector3 slopeGradeDirection, float slopeAngle) slopeOut = SlopeParallelDirAndAngle();
    // Rotates acceleration vector by cross product of slope direction by slope angle; < 0.1 deg disabled to prevent weirdness
    if ((slopeOut.slopeAngle < _maxDownwardsSlopeAngle) && !(slopeOut.slopeAngle < 0.1f)) {
      Quaternion slopeRotation = Quaternion.AngleAxis(slopeOut.slopeAngle, Vector3.Cross(slopeOut.slopeGradeDirection, Vector3.down));
      return slopeRotation * input;
    }
    else {
      return input;
    }
  }

  private void OnValidate() {
    _rigidbody = gameObject.GetComponent<Rigidbody>();
  }
  private void Start() {
    Cursor.lockState = CursorLockMode.Locked;
    _groundCheckDelay = _delayGroundCheckAfterJumpUpdates;
    _lastInputVector = Vector3.zero;
    _inputVector = Vector3.zero;
    _jumpInput = false;
    _slideInput = false;
  }
  private void Update() {
    if (_inputVector != _lastInputVector) {
      _timeSinceLastDirectionChange = 0;
    }
    else {
      _timeSinceLastDirectionChange += Time.deltaTime;
    }
    _lastInputVector = _inputVector;
    
    if (_procGen != null) _procGen.UpdatePlayerLoadedChunks(transform.position - new Vector3(0, GetComponent<Collider>().bounds.extents.y / 2, 0));
  }
  private void FixedUpdate() {
    if (_moveState == MovementState.Walking) { // ------------
      
      if (!Grounded()) {
        _moveState = MovementState.Falling;
      }
      else if (Grounded() && _jumpInput) {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _groundCheckDelay = _delayGroundCheckAfterJumpUpdates;
        _moveState = MovementState.Falling;
      }
      else if (Grounded() && _slideInput && _rigidbody.velocity.magnitude > _slideMinSpeed) {
        _rigidbody.drag = _slideDrag;
        SetColliderFriction(_slideFriction, _colliderMaterial.staticFriction);
        Vector3 velocity = _rigidbody.velocity;
        _rigidbody.velocity = _rigidbody.velocity.normalized * _slideStartSpeed;
        _moveState = MovementState.Sliding;
      }
      
      else {
        _rigidbody.drag = 0;
        if (_inputVector == Vector3.zero) {
          _rigidbody.drag = _moveDrag;
        }
        SetColliderFriction(_moveFriction, _colliderMaterial.staticFriction);
        Vector3 input = _inputVector;
        Vector3 accel = transform.TransformDirection(input) * _moveAccel;
        accel = ModifyVectorBySlope(accel);
        _rigidbody.AddForce(accel, ForceMode.VelocityChange);
        float speedCap = Mathf.Lerp(_maxMoveSpeedInit, _maxMoveSpeed, _timeSinceLastDirectionChange / _moveTimeInitToMax);
        if (_rigidbody.velocity.magnitude >= speedCap) {
          _rigidbody.velocity = _rigidbody.velocity.normalized * speedCap;
        }
      }
      
    }
    if (_moveState == MovementState.Dashing) { // -----------

      throw new NotImplementedException();

    }
    if (_moveState == MovementState.Sliding) { // ----------
      
      if (!Grounded() && !_slideInput) {
        _moveState = MovementState.Falling;
      }
      else if (Grounded() && _jumpInput) {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _groundCheckDelay = _delayGroundCheckAfterJumpUpdates;
        _moveState = MovementState.Falling;
      }
      
      else {
        if (!_slideInput) {
          _rigidbody.velocity = _rigidbody.velocity.normalized * _slideExitSpeed;
          _timeSinceLastDirectionChange = 0;
          _moveState = MovementState.Walking;
        }
        if (_rigidbody.velocity.magnitude < _slideForceExitSpeed) {
          _moveState = MovementState.Walking;
        }
      }
      
    }
    if (_moveState == MovementState.Falling) { // -----------
      
      if (Grounded() && _groundCheckDelay <= 0) {
        if (_slideInput) {
          // TODO: When landing from a jump, sliding causes stupid movement speed, still an issue
          _rigidbody.drag = _slideDrag;
          SetColliderFriction(_slideFriction, _colliderMaterial.staticFriction);
          _rigidbody.velocity = GetVelocityXZ() .normalized * _slideStartSpeed;
          _moveState = MovementState.Sliding;
        }
        else { 
          _rigidbody.drag = _moveDrag;
          SetColliderFriction(0, 0);
          _rigidbody.velocity /= 2;
          _timeSinceLastDirectionChange = 0;
          _moveState = MovementState.Walking;
        }
      }
      // https://www.reddit.com/r/Unity3D/comments/dcmvf5/i_replicated_the_source_engines_air_strafing/ <-- this is a great resource to fix this
      else {
        _groundCheckDelay--;
        _rigidbody.drag = _airDrag;
        SetColliderFriction(_moveFriction, _colliderMaterial.staticFriction);
        Vector3 velocityXZ = GetVelocityXZ();
        Vector3 inputVector = transform.TransformDirection(_inputVector);
        // ---- straight from link above
        Vector3 projectedVelocity = Vector3.Project(GetVelocityXZ(), inputVector);
        bool isAway = Vector3.Dot(inputVector, projectedVelocity) <= 0f;
        _accel = inputVector * _jumpMoveAccel;
        if (projectedVelocity.magnitude < _jumpMaxMoveSpeed || isAway) {
          if (!isAway) {
            _accel = Vector3.ClampMagnitude(_accel, _jumpMaxMoveSpeed - projectedVelocity.magnitude);
          }
          else {
            _accel = Vector3.ClampMagnitude(_accel, _jumpMaxMoveSpeed + projectedVelocity.magnitude);
          }
        }
        // ----
        else {
          _accel = Vector3.zero;
        }
        Physics.Raycast(transform.position, inputVector, out RaycastHit hit, inputVector.magnitude);
        if (hit.collider != null) {
          _accel = Vector3.zero;
        }
        _rigidbody.AddForce(_accel, ForceMode.VelocityChange);
      }
      
    }
  }
  private void OnDrawGizmos() {
    Gizmos.color = Color.green;
    Vector3 transformPosition = gameObject.transform.position;
    Gizmos.DrawLine(transformPosition, transformPosition + _rigidbody.velocity);
    Gizmos.DrawSphere(transformPosition + _groundCheckOffset, _groundCheckSphereSize);
    RaycastHit hit;
    Physics.Raycast(transform.position, Vector3.down, out hit);
    Gizmos.color = Color.cyan;
    Gizmos.DrawLine(transformPosition, hit.point);
    Gizmos.DrawLine(hit.point, hit.point + hit.normal);
    (Vector3 slopeGradeDirection, float slopeAngle) slopeOut = SlopeParallelDirAndAngle();
    Gizmos.DrawLine(transformPosition, transformPosition + Vector3.Cross(slopeOut.slopeGradeDirection.normalized, Vector3.up));
    Vector3 vel = transform.TransformDirection(_inputVector) * _moveAccel;
    Quaternion slopeRotation = Quaternion.AngleAxis(slopeOut.slopeAngle, Vector3.Cross(slopeOut.slopeGradeDirection, Vector3.down));
    Gizmos.color = Color.magenta;
    Gizmos.DrawLine(transformPosition, transformPosition + slopeRotation * vel);
  }
}
