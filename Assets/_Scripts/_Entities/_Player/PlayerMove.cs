using UnityEngine;

public class PlayerMove : MonoBehaviour
{
  public Transform _model;
  
  [SerializeField] private float _moveSmoothSpeed;
  [SerializeField] private float _gravityStrengh;
  [SerializeField] private float _jumpStrength;
  [SerializeField] private float _walkSpeed;
  [SerializeField] private float _runSpeed;
  [SerializeField] private float _crouchSpeed;

  private PlayerMovementHandler _handler;
  private CharacterController _characterController;
  private Vector3 _currentMoveVelocity;
  private Vector3 _moveDampVelocity;

  private Vector3 _currentForceVelocity;
  private float _gravityVelocity;

  private void Start() {
    _characterController = GetComponent<CharacterController>();
    _handler = GetComponent<PlayerMovementHandler>();
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
    switch (_handler.State) {
      case "walking": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        currentSpeed = _walkSpeed;
        break;
      }
      case "sprinting": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        currentSpeed = _runSpeed;
        break;
      }
      case "crouching": {
        moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        currentSpeed = _crouchSpeed;
        break;
      }
      case "air": {
        moveVector = _currentMoveVelocity.normalized;
        currentSpeed = _currentForceVelocity.magnitude;
        break;
      }
      case "freeze": {
        moveVector = Vector3.zero;
        currentSpeed = 0;
        break;
      }
      default: {
        Debug.LogWarning("Unexpected MovementState: " + _handler.State);
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
    if (_handler.State == "air") {
      // Equation for gravity: strength * seconds^2, so must multiply by Time.deltaTime twice
      _gravityVelocity -= _gravityStrengh * Time.deltaTime;
    }
    else
      _gravityVelocity = 0;

    return new Vector3(0, _gravityVelocity, 0) * Time.deltaTime;
  }

  private Vector3 GetJump() {
    if (_handler.State != "air") {
      if (Input.GetKey(_handler._jumpKey)) {
        _currentForceVelocity.y = _jumpStrength;
        _handler.State = "air";
      }
      else {
        _currentForceVelocity.y = 0;
      }
    }

    return _currentForceVelocity * Time.deltaTime;
  }
}
