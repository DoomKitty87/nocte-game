using UnityEngine;

public class EnemyController : MonoBehaviour
{
    
    #region Exposed Variables
    
    [HideInInspector] public bool _disableMovement;
    
    [Header("State")]
    [SerializeField, Tooltip("Player's current state")] public PlayerStates _state;
    public PlayerStates State {
        get => _state;
        set => SetState(value);
    }

    [Header("References")]
    [SerializeField, Tooltip("Player's local")] private Transform _orientation;
    
    [Header("Movement")]
    [SerializeField, Tooltip("Default walk speed on ground")] private float _walkSpeed;
    [SerializeField, Tooltip("Sprint speed on ground")] private float _sprintSpeed;
    [SerializeField, Tooltip("Crouching speed on ground")] private float _crouchSpeed;
    [SerializeField, Tooltip("Air speed")] private float _airMoveSpeed;
    [SerializeField, Tooltip("Slide/Crouch threshold")] private float _slideThreshold;
    [SerializeField, Tooltip("Velocity at which player can accelerate towards")] private float _airSpeedThreshold;
    [SerializeField, Tooltip("Jump force")] private float _jumpForce;
    [SerializeField, Tooltip("Gravity, positive is downwards")] private float _gravity;
    [SerializeField, Tooltip("Friction force")] private float _frictionCoefficient;
    [SerializeField, Tooltip("Friction force while sliding")] private float _slidingFrictionCoefficient;
    [SerializeField, Tooltip("Max angle at which ground is recognized as walkable")] private float _maxSlopeAngle;
    
    [Header("Vectors")]
    [SerializeField, Tooltip("Current position")] private Vector3 _position;
    [SerializeField, Tooltip("Current velocity")] private Vector3 _velocity;
    [SerializeField, Tooltip("Current acceleration")] private Vector3 _acceleration;
    [SerializeField, Tooltip("Normal of ground")] private Vector3 _normalVector;
    [SerializeField] private float _velocityMagnitude;
    [SerializeField] private float _horizontalVelocityMagnitude;
    
    [Header("Ground")]
    [SerializeField, Tooltip("Layer for ground")] private LayerMask _groundMask;
    
    [Header("Keybinds")]
    public KeyCode _jumpKey = KeyCode.Space;
    public KeyCode _sprintKey = KeyCode.LeftShift;
    public KeyCode _crouchKey = KeyCode.LeftControl;
    
    [Header("Stamina")]
    public float _maxStamina;
    public float _currentStamina;
    
    #endregion
    
    #region Hidden Variables

    private Rigidbody _rb;
    
    private bool _jumping;
    private bool _resetJump;
    private bool _sprinting;
    private bool _crouching;
    private bool _grounded;
    private bool _useVelocity = true;
    private bool _useGravity = true;
    
    private Vector3 _inputVectorNormalized;
    
    private Vector3 _horizontalVelocity;
    private Vector3 _horizontalAcceleration;

    private Transform _parent;

    private Vector3 _goalPos;
    private Vector3 _denPos;
    
    public enum PlayerStates
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Sliding,
        Air,
        Grappling,
        Driving,
        Frozen,
        NoClip
    }
    
    #endregion

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }
    
    #region State Machine
    
    public void SetState(PlayerStates newState) {
        
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
                break;
        }

        _state = newState;

        // Entering new state
        switch (_state) {
            case PlayerStates.Grappling:
                _useVelocity = false;
                break;
            
            case PlayerStates.Driving:
                _useVelocity = false;
                _useGravity = false;
                break;
        }
    }

    private void UpdateStates() {
        if (State is PlayerStates.Frozen or PlayerStates.NoClip or PlayerStates.Grappling or PlayerStates.Driving)
            return;
        
        if (!_grounded)
            SetState(PlayerStates.Air);
        else if (Vector3.Distance(_velocity, Vector3.zero) < 0.1f && _inputVectorNormalized == Vector3.zero)
            SetState(PlayerStates.Idle);
        else if (_crouching) {
            if (_horizontalVelocityMagnitude < _slideThreshold)
                SetState(PlayerStates.Crouching);
            else
                SetState(PlayerStates.Sliding);
        }
        else if (_sprinting)
            SetState(PlayerStates.Sprinting);
        else
            SetState(PlayerStates.Walking);
    }
    
    #endregion
    
    #region Input

    public void SetInputVector(Vector3 inputVector) {
        _inputVectorNormalized = inputVector;
    }

    public void SetGoalPos(Vector2 goalPos) {
        _goalPos = goalPos;
    }
    
    public Vector2 GetGoalPos() {
        return _goalPos;
    }

    public void SetDenPos(Vector3 denPos) {
        _denPos = denPos;
    }
    
    public Vector3 GetDenPos() {
        return _denPos;
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
        UpdateStates();
        _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
    }

    private void FixedUpdate() {
        Move();
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(_goalPos.x, transform.position.y, _goalPos.y), 1);
    }

    #endregion
    
    #region Movement
    private void Move() {
        if (_disableMovement) return;
        
        _position = transform.position;
        _velocity = _rb.velocity;
        _horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);

        _acceleration = Vector3.zero;
        _horizontalAcceleration = Vector3.zero;

        _velocityMagnitude = _velocity.magnitude;
        _horizontalVelocityMagnitude = _horizontalVelocity.magnitude;

        Vector3 forwardDirection = _orientation.forward;
        Vector3 rightDirection = _orientation.right;

        switch (State) {
            case PlayerStates.Walking: {
                // Transform the input vector to the orientation's forward and right directions
                Vector3 inputDirection =
                    (_inputVectorNormalized.x * rightDirection + _inputVectorNormalized.z * forwardDirection).
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
                    (_inputVectorNormalized.x * rightDirection + _inputVectorNormalized.z * forwardDirection).
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
                    (_inputVectorNormalized.x * rightDirection + _inputVectorNormalized.z * forwardDirection).
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
                    (_inputVectorNormalized.x * rightDirection + _inputVectorNormalized.z * forwardDirection).
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
                        Vector3 cappedNewVelocity = Vector3.ClampMagnitude(newVelocity, _horizontalVelocityMagnitude);
                        Vector3 acceleration = cappedNewVelocity - _horizontalVelocity;
                        _acceleration += acceleration;
                    }
                }
                
                break;
            }

            case PlayerStates.Driving: {
                transform.position = _parent.position;
                
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

    #endregion
    
    #region Public functions
    
    public void SetPosition(Vector3 position) {
        transform.position = position;
    }

    public void SetParent(Transform parent) {
        _parent = parent;
    }
    
    #endregion
    
    #region Helper functions
    
    private void  StopGrounded() { _grounded = false; }
    
    #endregion
    
    #region Health functions

    public void OnHealthZero() {
        Destroy(this.gameObject);
    }
    
    #endregion

}
