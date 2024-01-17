using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    #region Exposed Variables
    
    [Header("State")]
    [SerializeField, Tooltip("Player's current state")] private PlayerStates _state;
    public PlayerStates State {
        get => _state;
        set => SetState(value);
    }

    [Header("References")]
    [SerializeField, Tooltip("Player's local")] private Transform _orientation;
    
    [Header("Movement")]
    [SerializeField, Tooltip("Default move speed on ground")] private float _moveSpeed;
    [SerializeField, Tooltip("Sprint speed on ground")] private float _sprintSpeed;
    [SerializeField, Tooltip("Air speed")] private float _airMoveSpeed;
    [SerializeField, Tooltip("Velocity at which player can accelerate towards")] private float _airSpeedCutoff;
    [SerializeField, Tooltip("Jump force")] private float _jumpForce;
    [SerializeField, Tooltip("Gravity, positive is downwards")] private float _gravity;
    [SerializeField, Tooltip("Friction force")] private float _frictionCoefficient;
    [SerializeField, Tooltip("Max angle at which ground is recognized as walkable")] private float _maxSlopeAngle;
    
    [Header("Vectors")]
    [SerializeField, Tooltip("Current position")] private Vector3 _position;
    [SerializeField, Tooltip("Current velocity")] private Vector3 _velocity;
    [SerializeField, Tooltip("Current acceleration")] private Vector3 _acceleration;
    [SerializeField, Tooltip("Normal of ground")] private Vector3 _normalVector;
    
    [Header("Ground")]
    [SerializeField, Tooltip("Layer for ground")] private LayerMask _groundMask;
    
    [Header("Keybinds")]
    public KeyCode _jumpKey = KeyCode.Space;
    public KeyCode _sprintKey = KeyCode.LeftShift;
    
    #endregion
    
    #region Hidden Variables

    private Rigidbody _rb;

    private bool _canMove;
    private bool _jumping;
    private bool _resetJump;
    private bool _sprinting;
    private bool _grounded;
    
    private Vector3 _inputVectorNormalized;
    
    private Vector3 _horizontalVelocity;
    private Vector3 _horizontalAcceleration;
    
    public enum PlayerStates
    {
        Idle,
        Moving,
        Air,
        Grappling,
        Frozen,
        Noclip
    }

    #endregion

    #region State Machine
    
    private void SetState(PlayerStates newState) {
        
        // Handle exiting state
        switch (_state) {
            case PlayerStates.Air:
                _resetJump = true;
            break;
        }

        _state = newState;

        // Handle entering new state
        switch (_state) {
            case PlayerStates.Grappling:
                // StartGrapple();
            break;
        }
    }

    private void UpdateStates() {
        if (State is PlayerStates.Frozen or PlayerStates.Noclip or PlayerStates.Grappling)
            return;
        
        if (!_grounded)
            SetState(PlayerStates.Air);
        else if (_inputVectorNormalized != Vector3.zero)
            SetState(PlayerStates.Moving);
        else
            SetState(PlayerStates.Idle);
    }
    
    #endregion
    
    #region Input

    private void GetInput() {
        _inputVectorNormalized = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        _jumping = Input.GetKey(_jumpKey);
        _sprinting = Input.GetKey(_sprintKey);
    }
    
    #endregion
    
    #region Collisions

    private bool _cancellingGrounded;
    private void OnCollisionStay(Collision other) {
        // Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (_groundMask != (_groundMask | (1 << layer))) return;
        
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
        const float delay = 3f;
        if (!_cancellingGrounded) {
            _cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }
    
    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < _maxSlopeAngle;
    }
    
    #endregion

    #region Update

    private void Update() {
        GetInput();
        UpdateStates();
    }

    private void FixedUpdate() {
        Move();
    }
    
    #endregion

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Move() {
        _position = transform.position;
        _velocity = _rb.velocity;
        _horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);

        _acceleration = Vector3.zero;
        _horizontalAcceleration = Vector3.zero;

        Vector3 forwardDirection = _orientation.forward;
        Vector3 rightDirection = _orientation.right;

        switch (State) {
            case PlayerStates.Moving: {
                // Transform the input vector to the orientation's forward and right directions
                Vector3 inputDirection =
                    (_inputVectorNormalized.x * rightDirection + _inputVectorNormalized.z * forwardDirection).
                    normalized;

                // Fixed movement for slope                
                Vector3 fixedVector = Vector3.ProjectOnPlane(inputDirection, _normalVector).normalized;

                _acceleration += fixedVector * (_sprinting ? _sprintSpeed : _moveSpeed);

                // Friction
                _acceleration -= Vector3.ProjectOnPlane(_velocity, _normalVector) * _frictionCoefficient;

                if (_jumping && _resetJump) {
                    _acceleration += _jumpForce * Vector3.up;
                    _jumping = false;
                    _resetJump = false;
                }

                break;
            }

            case PlayerStates.Idle: {

                // Friction
                _acceleration -= Vector3.ProjectOnPlane(_velocity, _normalVector) * _frictionCoefficient;

                if (_jumping && _resetJump) {
                    _acceleration += _jumpForce * Vector3.up;
                    _jumping = false;
                    _resetJump = false;
                }

                break;
            }

            case PlayerStates.Air: {
                // Transform the input vector to the orientation's forward and right directions
                Vector3 inputDirection =
                    (_inputVectorNormalized.x * rightDirection + _inputVectorNormalized.z * forwardDirection).
                    normalized;

                // Gravity
                _acceleration += _gravity * Time.fixedDeltaTime * Vector3.down;

                // 4 different cases:
                if (_horizontalVelocity.magnitude < _airSpeedCutoff) {
                    if ((_horizontalVelocity + inputDirection * _airMoveSpeed).magnitude > _airSpeedCutoff) {
                        // Case 1: Previous velocity is less then cutoff and new velocity is less then cutoff.
                        // New velocity is calculated normally
                        _acceleration += inputDirection * _airMoveSpeed;
                    }
                    else {
                        // Case 2: Previous velocity is less then cutoff and new velocity is more then cutoff.
                        // New velocity capped at cutoff
                        Vector3 newVelocity = _horizontalVelocity + inputDirection * _airMoveSpeed;
                        Vector3 cappedNewVelocity = Vector3.ClampMagnitude(newVelocity, _airSpeedCutoff);
                        Vector3 acceleration = cappedNewVelocity - _horizontalVelocity;
                        _acceleration += acceleration;
                    }
                }
                else {
                    if ((_horizontalVelocity + inputDirection * _airMoveSpeed).magnitude < _airSpeedCutoff) {
                        // Case 3: Previous velocity is more then cutoff and new velocity is less then cutoff.
                        // New velocity is calculated normally
                        _acceleration += inputDirection * _airMoveSpeed;
                    }
                    else {
                        // Case 4: Previous velocity is more then cutoff and new velocity ism ore then cutoff.
                        // New velocity is capped at previous velocity magnitude
                        Vector3 newVelocity = _horizontalVelocity + inputDirection * _airMoveSpeed;
                        Vector3 cappedNewVelocity = Vector3.ClampMagnitude(newVelocity, _horizontalVelocity.magnitude);
                        Vector3 acceleration = cappedNewVelocity - _horizontalVelocity;
                        _acceleration += acceleration;
                    }
                }
                break;
            }

            case PlayerStates.Grappling: {
                break;
            }
        }
    
        // Apply forces
        _rb.velocity = _velocity + _acceleration;
    }

    
    private void StopGrounded() { _grounded = false; }
}
