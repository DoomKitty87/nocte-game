using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    [SerializeField] private Transform _orientation;
    [SerializeField] private Transform _camera;

    private Rigidbody _rb;

    [Space(10)] 
    [Header("General")] 
    [SerializeField] private float _gravity = 15f;
    [SerializeField] private float _friction = 0.2f;

    public float Gravity {
        get {
            return _gravity;
        }
        set
        {
            Physics.gravity = Vector3.down * value;
            _gravity = value;
        }
    }
    
    [Space(10)] 
    [Header("Keybinds")] 
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode _crouchKey = KeyCode.LeftControl;
    
    [Space(10)]
    [Header("Moving")] 
    [SerializeField] private float _moveSpeed = 4500f;
    [SerializeField] private float _maxSpeed = 20f;
    [SerializeField] private LayerMask _ground;
    [HideInInspector] public bool _grounded;

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
    private Vector2 _inputVector;
    private bool _jumping, _sprinting, _crouching;

    // Sliding
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector;

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
        if (Mathf.Sqrt((Mathf.Pow(_rb.velocity.x, 2) + Mathf.Pow(_rb.velocity.z, 2))) > _maxSpeed) {
            float fallSpeed = _rb.velocity.y;
            Vector3 newVelocity = _rb.velocity.normalized * _maxSpeed;
            _rb.velocity = new Vector3(newVelocity.x, fallSpeed, newVelocity.z);
        }
    }
    
    private Vector3 CorrectForSlope(Vector2 vector) {
        Vector3 fixedVector;
        
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2);
    
        Vector3 slopeNormal = hit.normal;

        fixedVector = Vector3.ProjectOnPlane(new Vector3(vector.x, 0, vector.y), slopeNormal).normalized;

        return fixedVector;
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
        Physics.gravity = Vector3.down * _gravity;
        
        HandleInput();
    }

    private void FixedUpdate() {
        HandleMovement();
        Debug.Log(new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude);
    }

    private void HandleInput() {
        _inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        
        _jumping = Input.GetKey(_jumpKey);
        _crouching = Input.GetKey(_crouchKey);

        if (Input.GetKeyDown(_crouchKey)) StartCrouch();
        else if (Input.GetKeyUp(_crouchKey)) StopCrouch();
    }

    private void StartCrouch() {
        transform.localScale = _crouchScale;
        
        // TODO: Fix this translation, possibly import lerp?
        transform.Translate(Vector3.down * (_playerScale.y / 2));
        if (_rb.velocity.magnitude > _minimumSlideMomentum && _grounded) {
            _rb.AddForce(_orientation.forward * _slideForce);
        }
    }

    private void StopCrouch() {
        transform.localScale = _playerScale;
        transform.Translate(Vector3.up * (_playerScale.y / 2));
    }

    private void HandleMovement() {
        _rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        
        Vector2 magnitude = FindVelRelativeToLook();
        
        CalculateFriction(_inputVector.x, _inputVector.y, magnitude);
        
        if (_grounded && _readyToJump && _jumping) Jump();
        
        if (_crouching && _grounded && _readyToJump) {
            _rb.AddForce(Time.deltaTime * 3000 * Vector3.down);
            return;
        }
        
        /*
        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (_inputVector.x > 0 && magnitude.x > _maxSpeed) _inputVector.x = 0;
        if (_inputVector.x < 0 && magnitude.x < -_maxSpeed) _inputVector.x = 0;
        if (_inputVector.y > 0 && magnitude.y > _maxSpeed) _inputVector.y = 0;
        if (_inputVector.y < 0 && magnitude.y < -_maxSpeed) _inputVector.y = 0;
        */

        //Some multipliers
        float multiplier = 1f;
        
        // Movement in air
        if (!_grounded) {
            multiplier = 0.5f;
        }
        
        // Movement while sliding
        // if (_grounded && _crouching) multiplierV = 0f;

        Vector3 _slopeFixedVelocity = CorrectForSlope(_inputVector);
        
        //Apply forces to move player
        _rb.AddForce(_orientation.transform.forward * _slopeFixedVelocity.z * _moveSpeed * Time.deltaTime * multiplier);
        _rb.AddForce(_orientation.transform.right * _slopeFixedVelocity.x * _moveSpeed * Time.deltaTime * multiplier);
        _rb.AddForce(_orientation.transform.up * _slopeFixedVelocity.y * _moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump() {
        _readyToJump = false;

        Vector3 velocity =_rb.velocity;
        if (velocity.y <= 0) _rb.velocity = new Vector3(velocity.x, 0f, velocity.z);
        
        // Sort of arbitrary values that cause player to jump in direction of normal
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
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (_ground != (_ground | (1 << layer))) return;
        
        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal)) {
                _grounded = true;
                _cancellingGrounded = false;
                _normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }
        
        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!_cancellingGrounded) {
            _cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }
    
    private void StopGrounded() { _grounded = false; }
    
}
