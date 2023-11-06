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
        currentSpeed = _walkSpeed;
        break;
      }
      case "Sprinting": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        currentSpeed = _runSpeed;
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
        currentSpeed = _crouchSpeed;
        break;
      }
      case "Air": {
        var velocity = _characterController.velocity;
        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        
        moveVector = horizontalVelocity.normalized;
        currentSpeed = horizontalVelocity.magnitude;
        break;
      }
      case "Freeze": {
        moveVector = Vector3.zero;
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
