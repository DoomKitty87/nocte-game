using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class EnemyController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    [SerializeField] private Transform _orientation;
    [SerializeField] private GameObject _player;

    private Rigidbody _rb;

    [Space(10)] 
    [Header("General")] 
    [SerializeField] private float _gravity = 15f;
    [SerializeField] private float _friction = 0.2f;
    public bool _useGravity = true;

    public float Gravity {
        get {
            return Physics.gravity.y;
        }
        set
        {
            Physics.gravity = Vector3.down * value;
        }
    }
    
    [Space(10)]
    [Header("Moving")] 
    [SerializeField] private float _moveSpeed = 4500f;
    [SerializeField] private float _maxGroundedSpeed = 20f;
    [SerializeField] private float _maxAirSpeed = 30f; // I kind of like the idea of having a unbounded max air speed
    [SerializeField] private LayerMask _ground;
    [HideInInspector] public bool _grounded;
    public bool _useGroundFriction = true;

    [SerializeField] private float _centerMovement = 0.175f;
    private float _threshold = 0.01f;
    [SerializeField] private float _maxSlopeAngle = 35;

    [Space(10)] 
    [Header("Crouching")] 
    [SerializeField] private float _slideForce = 400f;
    [SerializeField] private float _minimumSlideMomentum = 0.5f;
    private Vector3 _crouchScale = new Vector3(1f, 0.5f, 1f);
    private Vector3 _playerScale;

    [Space(10)] 
    [Header("Jumping")] 
    [SerializeField] private float _jumpForce = 550f;
    [SerializeField] private float _jumpCooldown = 0.25f;
    private bool _readyToJump = true;
    
    // Input
    private Vector3 _inputVector;
    private float _directionXKey, _directionYKey, _directionZKey;
    private bool _jumpKey, _crouchKey;
    private bool _jumping, _sprinting, _crouching;

    // Sliding
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector;

    private void CreateInput() {
        transform.LookAt(_player.transform);
        _directionXKey = 1f;
        _directionYKey = 0f;
        _directionZKey = 0f;
        _jumpKey = false;
        _crouchKey = false;
    }
    
    private Vector2 FindVelRelativeToLook() {
        float lookAngle = _orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rb.velocity.x, _rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = _rb.velocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }
    
    private void CalculateFriction(float x, float y, Vector2 magnitude) {
        if (!_grounded || _jumping) return;

        if (_crouching) {
            _rb.AddForce(_moveSpeed * Time.deltaTime * _friction * -_rb.velocity.normalized);
            return;
        }

        //Counter movement
        if (Math.Abs(magnitude.x) > _threshold && Math.Abs(x) < 0.05f || (magnitude.x < -_threshold && x > 0) || (magnitude.x > _threshold && x < 0)) {
            _rb.AddForce(_moveSpeed * Time.deltaTime * -magnitude.x * _friction * _orientation.transform.right);
        }
        if (Math.Abs(magnitude.y) > _threshold && Math.Abs(y) < 0.05f || (magnitude.y < -_threshold && y > 0) || (magnitude.y > _threshold && y < 0)) {
            _rb.AddForce(_moveSpeed * Time.deltaTime * -magnitude.y * _friction * _orientation.transform.forward);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(_rb.velocity.x, 2) + Mathf.Pow(_rb.velocity.z, 2))) > _maxGroundedSpeed) {
            float fallSpeed = _rb.velocity.y;
            Vector3 newVelocity = _rb.velocity.normalized * _maxGroundedSpeed;
            _rb.velocity = new Vector3(newVelocity.x, fallSpeed, newVelocity.z);
        }
    }
    
    private void CorrectForSlope(ref Vector3 vector) {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2);
        Vector3 slopeNormal = hit.normal;
        
        Vector3 fixedVector = Vector3.ProjectOnPlane(vector, slopeNormal).normalized;

        vector = fixedVector;
    }

    private void ClampMaximumVelocity(ref Vector3 vector) {
        
        Vector2 vectorNormalized = vector.normalized;

        float magnitude = Mathf.Clamp(vector.magnitude, 0, _maxGroundedSpeed);
        Vector2 clampedVelocity = vectorNormalized * magnitude;
        vector = clampedVelocity;
    }
    
    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        _rb.useGravity = false;
        Gravity = _gravity;
        _playerScale = transform.localScale;
    }

    private void Update() {
        CreateInput();
        HandleInput();
    }

    private void FixedUpdate() {
        HandleMovement();
        
        // Debug for horizontal velocity
        // Debug.Log($"Velocity: " + new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude, gameObject);
    }

    private void HandleInput() {
        _inputVector = new Vector3(_directionXKey, _directionYKey, _directionZKey).normalized;
        _jumping = _jumpKey;
        _crouching = _crouchKey;
    }

    private void HandleMovement() {
        if (_useGravity) _rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        
        Vector2 magnitude = FindVelRelativeToLook();
        
        // CalculateFriction(_inputVector.x, _inputVector.z, magnitude);
        
        if (_grounded && _readyToJump && _jumping) Jump();
        
        if (_crouching && _grounded && _readyToJump) {
            _rb.AddForce(Time.deltaTime * 3000 * Vector3.down);
            return;
        }
        
        // If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        // ClampMaximumVelocity(ref _inputVector);
        
        // Movement in air
        if (!_grounded) {
            AirMovement(ref _inputVector);
        } else {        
            CorrectForSlope(ref _inputVector);
        }
        
        //Apply forces to move player
        _rb.AddForce(_moveSpeed * Time.deltaTime * (_orientation.transform.rotation * _inputVector));
    }
    
    private void AirMovement(ref Vector3 moveVector) {
        Vector3 vector = new Vector3(moveVector.x, 0, moveVector.y);
        Vector3 projVel = Vector3.Project(_rb.velocity, vector);
        bool isAway = Vector3.Dot(vector, projVel) <= 0f;

        if (projVel.magnitude < _maxAirSpeed || isAway)
        {
            Vector3 vc = vector.normalized * _maxAirSpeed;
            if (!isAway)
                vc = Vector3.ClampMagnitude(vc, _maxGroundedSpeed - projVel.magnitude);
            else
                vc = Vector3.ClampMagnitude(vc, _maxGroundedSpeed + projVel.magnitude);

            moveVector = vc * Time.deltaTime;
        }
    }

    private void Jump() {
        _readyToJump = false;

        Vector3 velocity =_rb.velocity;
        if (velocity.y <= 0) _rb.velocity = new Vector3(velocity.x, 0f, velocity.z);
        
        // Sort of arbitrary values that cause player to jump in direction of ground normal
        _rb.AddForce(_jumpForce * 1.5f * Vector3.up);
        _rb.AddForce(_jumpForce * 0.5f * _normalVector);
        
        Invoke(nameof(ResetJump), _jumpCooldown);
    }

    private void ResetJump() { _readyToJump = true; }
    
    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < _maxSlopeAngle;
    }
    
    private bool _cancellingGrounded;
    private void OnCollisionStay(Collision other) {
        // If gravity is turned off, shouldn't slow down player
        if (!_useGravity) return;
        if (!_useGroundFriction) return;
        
        // Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (_ground != (_ground | (1 << layer))) return;
        
        // Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            if (IsFloor(normal)) {
                _grounded = true;
                _cancellingGrounded = false;
                _normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }
        
        // Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!_cancellingGrounded) {
            _cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }
    
    private void StopGrounded() { _grounded = false; }

    public void ResetGravity() { Gravity = _gravity; }

}
