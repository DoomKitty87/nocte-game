using System;
using Console;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _input;

    public static PlayerController Instance { get; private set; }
    
    // Temporary fix for visualizing crouching - See HandleCrouchingCameraPosition()
    [SerializeField] private Transform _cameraPosition;
    private Vector3 _defaultCameraPosition;
    [SerializeField] Vector3 _crouchingCameraPosition;
    
    #region Exposed Variables
    
    [HideInInspector] public bool _disableMovement;
    
    [Header("State")]
    [SerializeField, Tooltip("Player's current state")] private PlayerStates _state;
    public PlayerStates State {
        get => _state;
        set => SetState(value);
    }

    [Header("References")]
    [SerializeField] private Transform _movementOrientation;
    [SerializeField, Tooltip("Player's local")] private Transform _modelOrientation;

    [SerializeField] private WorldGenerator _worldGen;
    
    [Header("Movement")]
    [SerializeField, Tooltip("Speed at which model rotates to follow movement")] private float _rotationSpeed = 360;
    [SerializeField, Tooltip("Default walk speed on ground")] private float _walkSpeed;
    [SerializeField, Tooltip("Default walk speed in water")] private float _swimmingSpeed;
    [SerializeField, Tooltip("Sprint speed on ground")] private float _sprintSpeed;
    [SerializeField, Tooltip("Crouching speed on ground")] private float _crouchSpeed;
    [SerializeField, Tooltip("Air speed")] private float _airMoveSpeed;
    [SerializeField, Tooltip("Slide/Crouch threshold")] private float _slideThreshold;
    [SerializeField, Tooltip("Velocity at which player can accelerate towards")] private float _airSpeedThreshold;
    [SerializeField, Tooltip("Jump force")] private float _jumpForce;
    [SerializeField, Tooltip("Swimming upwards force")] private float _swimmingBuoyantForce;
    [SerializeField, Tooltip("Gravity, positive is downwards")] private float _gravity;
    [SerializeField, Tooltip("Friction force")] private float _frictionCoefficient;
    [SerializeField, Tooltip("Friction force in water")] private float _waterFrictionCoefficient;
    [SerializeField, Tooltip("Friction force while sliding")] private float _slidingFrictionCoefficient;
    [SerializeField, Tooltip("Max angle at which ground is recognized as walkable")] private float _maxSlopeAngle;
    [SerializeField, Tooltip("Noclip player speed")] private float _NoclipSpeed;
    
    [Header("Vectors")]
    [SerializeField, Tooltip("Current position")] private Vector3 _position;
    [SerializeField, Tooltip("Current velocity")] public Vector3 _velocity; // Referenced by PlayerAnimHandler, clean up in function or property if you want -Matthew
    [SerializeField, Tooltip("Current acceleration")] private Vector3 _acceleration;
    [SerializeField, Tooltip("Normal of ground")] private Vector3 _normalVector;
    [SerializeField] private float _velocityMagnitude;
    [SerializeField] private float _horizontalVelocityMagnitude;
    
    [Header("Ground")]
    [SerializeField, Tooltip("Layer for ground")] private LayerMask _groundMask;
    #endregion
    
    #region Events
    public delegate void OnFreeze();
    public delegate void OnUnFreeze();
    
    public static event OnFreeze Freeze;
    public static event OnUnFreeze UnFreeze;
    
    #endregion
    
    #region Hidden Variables

    private Rigidbody _rb;
    private Collider _collider;
    
    [HideInInspector] public bool _jumping;
    [HideInInspector] public bool _resetJump;
    [HideInInspector] public bool _walking;
    [HideInInspector] public bool _sprintingForward;
    [HideInInspector] public bool _sprinting;
    [HideInInspector] public bool _crouching;
    [HideInInspector] public bool _grounded;
    [HideInInspector] public bool _useVelocity = true;
    [HideInInspector] public bool _useGravity = true;
    
    private float _currentWaterHeight;

    private Vector3 _inputVector;
    
    private Vector3 _horizontalVelocity;
    private Vector3 _horizontalAcceleration;

    private Transform _parent;
    
    public enum PlayerStates
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Sliding,
        Air,
        Swimming,
        Grappling,
        Driving,
        Frozen,
        Noclip
    }

    #endregion

    
    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(this); 
        } 
        else { 
            Instance = this; 
        } 
        
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _defaultCameraPosition = _cameraPosition.localPosition;
    }

    private void OnEnable() {
        _rb = GetComponent<Rigidbody>();
        
        if (TryGetComponent(out ConsoleController controller)) {
            if (BackgroundInfo._freezeTimeOnEnterConsole) {
                ConsoleController.ConsoleOpened += DisablePlayer;
                ConsoleController.ConsoleClosed += EnablePlayer;
            }
        }
    }

    void Start() {
        _input = InputReader.Instance.PlayerInput;

        _input.Player.Jump.performed += _ => _jumping = true;
        _input.Player.Jump.canceled += _ => _jumping = false;

        _input.Player.Sprint.performed += _ => _sprinting = true;
        _input.Player.Sprint.canceled += _ => _sprinting = false;

        _input.Player.Crouch.performed += _ => _crouching = true;
        _input.Player.Crouch.canceled += _ => _crouching = false;

        _input.Player.Movement.performed += context => _inputVector = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);
        _input.Player.Movement.canceled += _ => _inputVector = Vector3.zero;
    }

    #region State Machine
    
    private void SetState(PlayerStates newState) {
        
        // Exiting state
        switch (_state) {
            case PlayerStates.Air:
                _resetJump = true;
                break; 
            
            case PlayerStates.Grappling:
                _useVelocity = true;
                break;
            
            case PlayerStates.Driving:
                _useVelocity = true;
                _useGravity = true;
                
                EnableColliders();
                break;
            
            case PlayerStates.Frozen:
                if (_rb != null && _rb.isKinematic == false)
                    _rb.velocity = _velocity;
                
                if (UnFreeze != null)
                    UnFreeze();
                break;
            
            case PlayerStates.Noclip:
                _useGravity = true;
                _useVelocity = true;
                EnableColliders();
                break;
        }

        _state = newState;

        // Entering new state
        switch (_state) {
            case PlayerStates.Grappling:
                _useVelocity = false;
                break;
            
            case PlayerStates.Driving:
                DisableColliders();
                _useVelocity = false;
                _useGravity = false;
                break;
            
            case PlayerStates.Frozen:
                if (_rb != null)
                    _rb.velocity = Vector3.zero;

                if (Freeze != null) 
                    Freeze();
                break;
            
            case PlayerStates.Noclip:
                DisableColliders();
                _useGravity = false;
                _useVelocity = false;
                break;
        }
    }
    
    private bool _disableWorldGen = false;
    private void UpdateStates() {
        if (!_disableWorldGen) {
            if (_worldGen != null) {
                _currentWaterHeight = _worldGen.GetWaterHeight(new Vector2(transform.position.x, transform.position.z));
                _currentWaterHeight = Math.Max(_currentWaterHeight, WorldGenInfo._lakePlaneHeight);
            }
            else
                _disableWorldGen = true;
        }

        if (State is PlayerStates.Frozen or PlayerStates.Noclip or PlayerStates.Grappling or PlayerStates.Driving)
            return;

        if (!_grounded)
            SetState(PlayerStates.Air);
        else if (Vector3.Distance(_velocity, Vector3.zero) < 0.1f && _inputVector == Vector3.zero)
            SetState(PlayerStates.Idle);
        else if (_crouching) {
            if (_horizontalVelocityMagnitude < _slideThreshold)
                SetState(PlayerStates.Crouching);
            else
                SetState(PlayerStates.Sliding);
        }
        else if (_sprintingForward)
            SetState(PlayerStates.Sprinting);
        else
            SetState(PlayerStates.Walking);
        
        if (!_disableWorldGen) {
            if (_currentWaterHeight > transform.position.y)
                SetState(PlayerStates.Swimming);
        }
    }
    
    #endregion
    
    #region Input

    private void GetInput() {
        _sprintingForward = (_sprinting && _inputVector.z > 0); // Can only sprint when forward component in input

        // Little bit dumb but it works
        _walking = _inputVector != Vector3.zero && !_sprinting;
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
    
    #region Movement
    private void Move() {
        if (_disableMovement) return;

        if (State is PlayerStates.Frozen) return;


        _position = transform.position;
        _velocity = _rb.velocity;
        _horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);

        _acceleration = Vector3.zero;
        _horizontalAcceleration = Vector3.zero;

        _velocityMagnitude = _velocity.magnitude;
        _horizontalVelocityMagnitude = _horizontalVelocity.magnitude;

        Vector3 forwardDirection = new Vector3(_movementOrientation.forward.x, 0, _movementOrientation.forward.z).normalized;
        Vector3 rightDirection = new Vector3(_movementOrientation.right.x, 0, _movementOrientation.right.z).normalized;
        
        RotateModelOrientationToMovement((_inputVector.x * rightDirection + _inputVector.z * forwardDirection).normalized);

        switch (State) {
            case PlayerStates.Walking: {
                // Transform the input vector to the orientation's forward and right directions
                Vector3 inputDirection =
                    (_inputVector.x * rightDirection + _inputVector.z * forwardDirection).
                    normalized;

                // Fixed movement for slope                
                Vector3 fixedVector = Vector3.ProjectOnPlane(inputDirection, _normalVector).normalized;

                _acceleration += fixedVector * _walkSpeed;

                // Friction
                _acceleration -= Vector3.ProjectOnPlane(_velocity, _normalVector) * _frictionCoefficient;

                if (_jumping && _resetJump) {
                    _acceleration += _jumpForce * Vector3.up;
                    _jumping = false;
                    _resetJump = false;
                }

                break;
            }

            case PlayerStates.Sprinting: {
                // Transform the input vector to the orientation's forward and right directions
                Vector3 inputDirection =
                    (_inputVector.x * rightDirection + _inputVector.z * forwardDirection).
                    normalized;

                // Fixed movement for slope                
                Vector3 fixedVector = Vector3.ProjectOnPlane(inputDirection, _normalVector).normalized;

                _acceleration += fixedVector * _sprintSpeed;

                // Friction
                _acceleration -= Vector3.ProjectOnPlane(_velocity, _normalVector) * _frictionCoefficient;

                if (_jumping && _resetJump) {
                    _acceleration += _jumpForce * Vector3.up;
                    _jumping = false;
                    _resetJump = false;
                }

                break;
            }

            case PlayerStates.Crouching: {
                // Transform the input vector to the orientation's forward and right directions
                Vector3 inputDirection =
                    (_inputVector.x * rightDirection + _inputVector.z * forwardDirection).
                    normalized;

                // Fixed movement for slope                
                Vector3 fixedVector = Vector3.ProjectOnPlane(inputDirection, _normalVector).normalized;

                _acceleration += fixedVector * _crouchSpeed;

                // Friction
                _acceleration -= Vector3.ProjectOnPlane(_velocity, _normalVector) * _frictionCoefficient;

                if (_jumping && _resetJump) {
                    _acceleration += _jumpForce * Vector3.up;
                    _jumping = false;
                    _resetJump = false;
                }

                break;
            }

            case PlayerStates.Sliding: {
                // Note that gravity applies some friction to movement already

                _acceleration -= Vector3.ProjectOnPlane(_velocity, _normalVector) * _slidingFrictionCoefficient;

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
                    (_inputVector.x * rightDirection + _inputVector.z * forwardDirection).
                    normalized;

                // 4 different cases:
                if (_horizontalVelocityMagnitude < _airSpeedThreshold) {
                    if ((_horizontalVelocity + inputDirection * _airMoveSpeed).magnitude > _airSpeedThreshold) {
                        // Case 1: Previous velocity is less then cutoff and new velocity is less then cutoff.
                        // New velocity is calculated normally
                        _acceleration += inputDirection * _airMoveSpeed;
                    }
                    else {
                        // Case 2: Previous velocity is less then cutoff and new velocity is more then cutoff.
                        // New velocity capped at cutoff
                        Vector3 newVelocity = _horizontalVelocity + inputDirection * _airMoveSpeed;
                        Vector3 cappedNewVelocity = Vector3.ClampMagnitude(newVelocity, _airSpeedThreshold);
                        Vector3 acceleration = cappedNewVelocity - _horizontalVelocity;
                        _acceleration += acceleration;
                    }
                }
                else {
                    if ((_horizontalVelocity + inputDirection * _airMoveSpeed).magnitude < _airSpeedThreshold) {
                        // Case 3: Previous velocity is more then cutoff and new velocity is less then cutoff.
                        // New velocity is calculated normally
                        _acceleration += inputDirection * _airMoveSpeed;
                    }
                    else {
                        // Case 4: Previous velocity is more then cutoff and new velocity ism ore then cutoff.
                        // New velocity is capped at previous velocity magnitude
                        Vector3 newVelocity = _horizontalVelocity + inputDirection * _airMoveSpeed;
                        Vector3 cappedNewVelocity = Vector3.ClampMagnitude(
                            newVelocity,
                            _horizontalVelocityMagnitude
                        );
                        Vector3 acceleration = cappedNewVelocity - _horizontalVelocity;
                        _acceleration += acceleration;
                    }
                }

                break;
            }

            case PlayerStates.Swimming: {
                Vector3 inputDirection =
                    (_inputVector.x * rightDirection + _inputVector.z * forwardDirection).
                    normalized;

                _acceleration += inputDirection * _swimmingSpeed;
                _acceleration -= _velocity * _waterFrictionCoefficient;


                _acceleration += Vector3.up * (_swimmingBuoyantForce *
                                               Mathf.Max(
                                                   _currentWaterHeight - (transform.position.y),
                                                   0
                                               ));

                break;
            }

            case PlayerStates.Driving: {
                transform.position = _parent.position;

                break;
            }

            case PlayerStates.Noclip: {
                if (Camera.main == null)
                    throw new NullReferenceException("No camera tagged 'MainCamera' in scene.");
                Transform mainCamera = Camera.main.transform;

                Vector3 inputDirection =
                    (_inputVector.x * mainCamera.right + _inputVector.z * mainCamera.forward).
                    normalized;

                float verticalMove = _input.Player.VerticalMovement.ReadValue<float>();
                if (verticalMove > 0) inputDirection += mainCamera.up;
                if (verticalMove < 0) inputDirection -= mainCamera.up;

                // Horrible but funny
                _velocity = inputDirection *
                            (_crouching
                                ? (_sprinting ? _NoclipSpeed * 10 : _NoclipSpeed / 2)
                                : (_sprinting ? _NoclipSpeed * 3 : _NoclipSpeed));

                transform.Translate(_velocity * Time.fixedDeltaTime);

                break;
            }
        }

        // Apply gravity
        if (_useGravity)
            _acceleration += _gravity * Time.fixedDeltaTime * Vector3.down;
        
        // Apply forces
        // Boolean used for cases when rb.AddForce is required
        if (_useVelocity)
            _rb.velocity = _velocity + _acceleration;
    }
    
    private void RotateModelOrientationToMovement(Vector3 inputVector) {
        if (inputVector != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(inputVector, Vector3.up);
            _modelOrientation.rotation = Quaternion.RotateTowards(_modelOrientation.rotation, toRotation, _rotationSpeed * Time.deltaTime);            
        }
    }
    
    #endregion
    
    #region Public functions
    
    public void SetPosition(Vector3 position) {
        transform.position = position;
    }

    public void SetParent(Transform parent) {
        _parent = parent;
    }

    // void OnDrawGizmos() {
    //   Gizmos.color = Color.yellow;
    //   Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - _collider.bounds.size.y / 2, transform.position.z), 1);
    //   Gizmos.DrawSphere(new Vector3(transform.position.x, _worldGen.GetWaterHeight(new Vector2(transform.position.x, transform.position.z)), transform.position.z), 1);
    // }
    
    #endregion
    
    #region Helper functions

    private void DisablePlayer() {
        _acceleration = Vector3.zero;
        _disableMovement = true;
    }

    private void EnablePlayer() =>
        _disableMovement = false;
    
    private void StopGrounded() { _grounded = false; }

    private void DisableColliders() {
        _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        _rb.isKinematic = true;
        _collider.enabled = false;
    }

    private void EnableColliders() {
        _rb.isKinematic = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _collider.enabled = true;
    }
    
    #endregion
    
}
