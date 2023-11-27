using UnityEngine;
using ObserverPattern;

public class PlayerMove : MonoBehaviour, IObserver
{
  public Transform _model;
  
  [SerializeField] private float _moveSmoothSpeed;
  [SerializeField] private float _gravityStrengh;
  [SerializeField] private float _jumpStrength;
  [SerializeField] private float _runSpeed;
  [SerializeField] private float _crouchSpeed;

  // Local current move speed for the played based on inputs
  private float _moveSpeed;

  [Space(10)]
  [Header("Air")]
  [SerializeField] private float _airMaxSpeed;
  
  [Space(10)]
  [Header("Sliding")]
  [SerializeField] private float _slideFriction;
  [SerializeField] private float _slideSpeedBoost;
  [SerializeField] private float _slideJumpBoost;
  [SerializeField] private float _slideCutoffSpeed;
  [SerializeField] private float _slideCoolDown;

  private PlayerMovementHandler _handler;
  // private CharacterController _characterController;
  private Vector3 _currentMoveVelocity;
  private Vector3 _moveDampVelocity;

  private Vector3 _currentForceVelocity;
  private float _gravityVelocity;

  private string _currentState;

  // Mini state machine used for insuring only one jump input at a time
  private int _jumpReset = 0;

  private void OnEnable() {
    _handler = GetComponent<PlayerMovementHandler>();
    _handler.AddObserver(this);
  }

  private void OnDisable() {
    _handler.RemoveObserver(this);
  }

  private void Start() {
    _currentState = _handler.State;
  }

  private void Update() {
    if (_jumpReset != 0) ManageJump();
    
    GetMovementVelocity();
    GetJump();
  }
  
  private void GetMovementVelocity() {
    Vector3 playerInput = new Vector3 {
      x = Input.GetAxisRaw("Horizontal"),
      y = 0f,
      z = Input.GetAxisRaw("Vertical")
    };
    
    if (playerInput.magnitude > 1) playerInput.Normalize();

    Vector3 moveVector;
    float currentSpeed;
    switch (_currentState) {
      case "Sprinting": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        _moveSpeed = _runSpeed;
        currentSpeed = _moveSpeed;
        moveVector = CorrectForSlope(moveVector);
        break;
      }
      case "Sliding": {
        var velocity = _handler.Velocity;
        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        
        moveVector = horizontalVelocity.normalized;
        currentSpeed = horizontalVelocity.magnitude - (_slideFriction * Time.deltaTime);
        break;
      }
      case "Crouching": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        _moveSpeed = _crouchSpeed;
        currentSpeed = _moveSpeed;
        break;
      }
      case "Air": {
        var velocity = _handler.Velocity;
        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        Vector3 newDirection = AirMovement((_model.forward * playerInput.z + _model.right * playerInput.x).normalized, horizontalVelocity);
        
        moveVector = newDirection.normalized;
        currentSpeed = newDirection.magnitude;
        break;
      }
      case "Freeze": {
        moveVector = Vector3.zero;
        _moveSpeed = 0;
        currentSpeed = 0;
        break;
      }
      default: {
        Debug.LogWarning("Unexpected MovementState: " + _currentState);
        moveVector = Vector3.zero;
        currentSpeed = 0;
        break;
      }
    }
    
    _currentMoveVelocity = Vector3.SmoothDamp(
      _currentMoveVelocity,
      moveVector * currentSpeed,
      ref _moveDampVelocity,
      _moveSmoothSpeed
    );

    if (_currentMoveVelocity == Vector3.zero && playerInput == Vector3.zero) return;
    
    _handler.SetHorizontalVelocity(_currentMoveVelocity * Time.deltaTime * Time.deltaTime);
  }
  
  private Vector3 AirMovement(Vector3 moveVector, Vector3 previousVelocity)
  {
    Vector3 projVel = Vector3.Project(previousVelocity, moveVector);
    bool isAway = Vector3.Dot(moveVector, projVel) <= 0f;

    if (projVel.magnitude < _airMaxSpeed || isAway)
    {
      Vector3 vc = moveVector.normalized * _airMaxSpeed;
      if (!isAway)
        vc = Vector3.ClampMagnitude(vc, _airMaxSpeed - projVel.magnitude);
      else
        vc = Vector3.ClampMagnitude(vc, _airMaxSpeed + projVel.magnitude);
        
      return vc;
    }

    return Vector3.zero;
  }

  private Vector3 CorrectForSlope(Vector3 vector) {
    Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2);
    // if (Vector3.Angle(hit.normal, Vector3.up) > _characterController.slopeLimit) {
    //   // Player should slide down slope that is too steeply angled
    //   float magnitude = Mathf.Cos(Vector3.Angle(hit.normal, Vector3.up))
    //   return Vector3.zero;
    // }
    
    Vector3 slopeNormal = hit.normal;

    Vector3 inputOnSlope = Vector3.ProjectOnPlane(vector, slopeNormal).normalized;

    return inputOnSlope;
  }

  private Vector3 CalculateGravityVelocity() {
    if (_currentState == "Air") 
      // Equation for gravity: strength * seconds^2, so must multiply by Time.deltaTime twice
      _gravityVelocity -= _gravityStrengh * Time.deltaTime;
    else if (_handler.OnSteepSlope || _handler.IsSliding)
      _gravityVelocity = 0f;
    else
      _gravityVelocity = 0f;
    

    return new Vector3(0, _gravityVelocity, 0) * Time.deltaTime;
  }

  private void GetJump() {
    if (_jumpReset != 0) return;
    if (_handler.Grounded) {
      if (Input.GetKey(_handler._jumpKey)) {
        Debug.Log("Jump!");
        _jumpReset = 2;
        ManageJump();
        _handler.SetVerticalVelocity( _jumpStrength * 0.1f);
      }
    }
  }

  private void ManageJump() {
    // 2 means Jump input has been registered, player still colliding with ground
    // 1 means Jump input has been registered, player is not colliding with ground
    // 0 means Jump input has not yet been registered, player is colliding with ground
    switch (_jumpReset) {
      case 2:
        if (_handler.Grounded) break;
        _jumpReset = 1;
        break;
      case 1:
        if (!_handler.Grounded) break;
        _jumpReset = 0;
        break;
      default:
        Debug.LogWarning($"JumpInitialized has been called with _jumpReset value = " + _jumpReset + ". This shouldn't happen!");
        break;
    }
  }

  public void OnSceneChangeNotify(string previousState, string currentState) {
    
    _currentState = currentState;
  }
}
