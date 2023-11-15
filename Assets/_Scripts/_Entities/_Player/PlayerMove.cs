using System;
using System.Collections.Generic;
using UnityEngine;
using ObserverPattern;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour, IObserver
{
  public Transform _model;
  
  public float _gravityStrength;
  
  [SerializeField] private float _moveSmoothSpeed;
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
  private CharacterController _characterController;
  private Vector3 _currentMoveVelocity;
  private Vector3 _moveDampVelocity;

  private Vector3 _currentForceVelocity;
  private float _gravityVelocity;

  // Should be used by other scripts to add forces to player object
  public Vector3[] _additionalForces = new Vector3[0];
  
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
    GetJump();
    GetMovementVelocity();
  }
  
  private void GetMovementVelocity() {
    Vector3 playerInput = new Vector3 {
      x = Input.GetAxisRaw("Horizontal"),
      y = 0f,
      z = Input.GetAxisRaw("Vertical")  
    };
    
    if (playerInput + _characterController.velocity == Vector3.zero) return;
    
    if (playerInput.magnitude > 1) playerInput.Normalize();
    
    switch (_currentState) {
      case "Sprinting": {
        Vector3 moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        ApproachVector(_characterController.velocity, moveVector * _runSpeed);
        break;
      }
      //case "Sliding": {
      //  var velocity = _characterController.velocity;
      //  var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
      //  
      //  moveVector = horizontalVelocity.normalized;
      //  currentSpeed = horizontalVelocity.magnitude - (_slideFriction * Time.deltaTime);
      //  break;
      //}
      case "Crouching": {
        Vector3 moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        ApproachVector(_characterController.velocity, moveVector * _crouchSpeed);
        break;
      }
      case "Air": {
        Vector3 moveVector = _model.forward * playerInput.z + _model.right * playerInput.x;
        AirMovement(_characterController.velocity, moveVector);
        
        // Enable if you don't want air strafing
        //moveVector = (horizontalVelocity.normalized + 
        //              ((_model.forward * playerInput.z + _model.right * playerInput.x) * _airTurnControl)).normalized;
        //currentSpeed = Mathf.Min(
        //(horizontalVelocity + ((_model.forward * playerInput.z + _model.right * playerInput.x) * _airSpeedControl)).magnitude, 
        //horizontalVelocity.magnitude);
        
        break;
      }
      //case "Grapple": {
      //  moveVector = Vector3.zero;
      //  _moveSpeed = 0;
      //  currentSpeed = 0;
      //  break;
      //}
      //case "Freeze": {
      //  moveVector = Vector3.zero;
      //  _moveSpeed = 0;
      //  currentSpeed = 0;
      //  break;
      //}
      default: {
        Debug.LogWarning("Unexpected MovementState: " + _currentState);
        return;
      }
    }
    
    //_currentMoveVelocity = Vector3.SmoothDamp(
    //   _currentMoveVelocity,
    //   moveVector * currentSpeed,
    //   ref _moveDampVelocity,
    //   _moveSmoothSpeed
    // );

    //_handler.AddForce(_currentMoveVelocity);
  }

  private void ApproachVector(Vector3 previousVector, Vector3 targetVector) {
    // Calculate the difference between previousVector and targetVector
    Vector3 vectorDifference = targetVector - previousVector;

    // Use exponential decay to approach targetVector
    Vector3 nextVector = previousVector + vectorDifference * Mathf.Exp(-_handler._decayFactor);

    _handler._newVelocity += nextVector;
  }
  private void AirMovement(Vector3 previousVelocity, Vector3 moveVector)
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
        
      _handler._newVelocity += vc;
    }
  }

  private void GetJump() {
    if (_handler.State != "Air" && _currentState != "Grapple") {
      if (Input.GetKey(_handler._jumpKey)) {
        _handler._newVelocity += new Vector3(0, _jumpStrength, 0);
      }
    }
  }

  // Called by a different script to 'register' a spot in _additionalForces array, returning the index to be edited
  public int RegisterForce() {
    Array.Resize(ref _additionalForces, _additionalForces.Length + 1);
    return _additionalForces.Length - 1;
  }

  // Sums up all external forces
  private Vector3 GetOtherForces() {
    if (_additionalForces.Length == 0) return Vector3.zero;
    Vector3 forces = Vector3.zero;
    for (int i = 0; i < _additionalForces.Length; i++)
      forces += _additionalForces[i];

    return forces;
  }
  
  public void OnSceneChangeNotify(string previousState, string currentState) {
    _currentState = currentState;
  }
}
