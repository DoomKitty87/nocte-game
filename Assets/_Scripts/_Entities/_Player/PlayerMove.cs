using UnityEngine;
using ObserverPattern;

public class PlayerMove : MonoBehaviour, IObserver
{
  public Transform _model;
  
  [SerializeField] private float _moveSmoothSpeed;
  [SerializeField] private float _gravityStrengh;
  [SerializeField] private float _jumpStrength;
  [SerializeField] private float _walkSpeed;
  [SerializeField] private float _runSpeed;
  [SerializeField] private float _crouchSpeed;

  // Local current move speed for the played based on inputs
  private float _moveSpeed;

  [Space(10)] [Header("Air")] 
  [SerializeField] private float _airMaxSpeed;
  [SerializeField] private float _airTurnControl;
  [SerializeField] private float _airSpeedControl;
  
  [Space(10)]
  [Header("Sliding")]
  [SerializeField] private float _slideFriction;
  [SerializeField] private float _slideSpeedBoost;
  [SerializeField] private float _slideJumpBoost;
  [SerializeField] private float _slideCutoffSpeed;
  [SerializeField] private float _slideCoolDown;

  private PlayerMovementHandler _handler;
  private CharacterController _characterController;
  private Vector3 _currentMoveVelocity;
  private Vector3 _moveDampVelocity;

  private Vector3 _currentForceVelocity;
  private float _gravityVelocity;

  private string _currentState;

  private void OnEnable() {
    _handler = GetComponent<PlayerMovementHandler>();
    _handler.AddObserver(this);
  }

  private void OnDisable() {
    _handler.RemoveObserver(this);
  }

  private void Start() {
    _characterController = GetComponent<CharacterController>();
    _currentState = _handler.State;
  }

  private void Update() {
    _characterController.Move(GetMovementVelocity() + CalculateGravityVelocity() + GetJump());
  }
  
  private Vector3 GetMovementVelocity() {
    Vector3 playerInput = new Vector3 {
      x = Input.GetAxisRaw("Horizontal"),
      y = 0f,
      z = Input.GetAxisRaw("Vertical")
    };
    
    if (playerInput.magnitude > 1) playerInput.Normalize();

    Vector3 moveVector;
    float currentSpeed;
    switch (_currentState) {
      case "Walking": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        _moveSpeed = _walkSpeed;
        currentSpeed = _moveSpeed;
        break;
      }
      case "Sprinting": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        _moveSpeed = _runSpeed;
        currentSpeed = _moveSpeed;
        break;
      }
      case "Sliding": {
        var velocity = _characterController.velocity;
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
        var velocity = _characterController.velocity;
        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        
        moveVector = (horizontalVelocity.normalized + 
                      ((_model.forward * playerInput.z + _model.right * playerInput.x) * _airTurnControl)).normalized;
        currentSpeed = Mathf.Min(
        (horizontalVelocity + ((_model.forward * playerInput.z + _model.right * playerInput.x) * _airSpeedControl)).magnitude, 
        horizontalVelocity.magnitude);
        // Calculates air speed based off _airSpeedControl and previous velocity
        // currentSpeed = Mathf.Min(
        //   (horizontalVelocity + ((_model.forward * playerInput.z + _model.right * playerInput.x) * _airSpeedControl)).magnitude, 
        //   _moveSpeed);
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

    Vector3 AirMovement(Vector3 moveVector, Vector3 previousVelocity)
    {
      // project the velocity onto the movevector
      Vector3 projVel = Vector3.Project(previousVelocity, moveVector);

      // check if the movevector is moving towards or away from the projected velocity
      bool isAway = Vector3.Dot(moveVector, projVel) <= 0f;

      // only apply force if moving away from velocity or velocity is below MaxAirSpeed
      if (projVel.magnitude < _airMaxSpeed || isAway)
      {
        // calculate the ideal movement force
        Vector3 vc = moveVector.normalized * _airMaxSpeed;

        // cap it if it would accelerate beyond MaxAirSpeed directly.
        if (!isAway)
        {
          vc = Vector3.ClampMagnitude(vc, _airMaxSpeed - projVel.magnitude);
        }
        else
        {
          vc = Vector3.ClampMagnitude(vc, _airMaxSpeed + projVel.magnitude);
        }

        // Apply the force
        return vc;
      }

      return previousVelocity;
    }
    
    _currentMoveVelocity = Vector3.SmoothDamp(
      _currentMoveVelocity,
      moveVector * currentSpeed,
      ref _moveDampVelocity,
      _moveSmoothSpeed
    );

    return _currentMoveVelocity * Time.deltaTime;
  }

  private Vector3 CalculateGravityVelocity() {
    if (_currentState == "Air") 
      // Equation for gravity: strength * seconds^2, so must multiply by Time.deltaTime twice
      _gravityVelocity -= _gravityStrengh * Time.deltaTime;
    else
      _gravityVelocity = 0f;

    return new Vector3(0, _gravityVelocity, 0) * Time.deltaTime;
  }

  private Vector3 GetJump() {
    if (_currentState != "Air") {
      if (Input.GetKey(_handler._jumpKey)) {
        _currentForceVelocity.y = _jumpStrength;
        _handler.State = "Air";
      }
      else {
        _currentForceVelocity.y = 0;
      }
    }
    return _currentForceVelocity * Time.deltaTime;
  }

  public void OnNotify(string previousState, string currentState) {
    
    _currentState = currentState;
  }
}
