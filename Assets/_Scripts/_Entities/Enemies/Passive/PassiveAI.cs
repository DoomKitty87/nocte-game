using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PassiveAI : MonoBehaviour
{

  [Header("Dependancies")] 
  [SerializeField] private Movement _movement;
  
  [Header("State")] 
  public AIState _state;
  public enum AIState 
  {
    Wandering,
    Watching,
    Fleeing,
    StartGrouping,
    Grouping,
  }

  [SerializeField] private GameObject _player;
  
  [Header("Wandering")] 
  [SerializeField] private float _maxNewWaypointDistance = 15;
  [SerializeField] private float _minNewWaypointDistance = 5;
  [SerializeField] private float _waypointArriveRadius = 2;
  [SerializeField] private Vector3 _waypoint;
  private bool _firstWanderFrame;
  
  [Header("Watching")] 
  [SerializeField] private float _watchIfDistanceLess = 10;
  [SerializeField] private float _timeWatchMin = 2;
  [SerializeField] private float _timeWatchMax = 1;
  [SerializeField] private float _timeWatchLimit = 0;
  [SerializeField] private float _timeSinceWatchStart = 0;
  private bool _firstWatchingFrame;
  
  private float GetDistanceToPlayer() {
    if (_player == null) {
      _player = GameObject.FindWithTag("Player");
    }
    return Vector3.Distance(_player.transform.position, transform.position);
  }
  
  private Vector3 GetRandomWaypoint() {
    float dist = Random.Range(_minNewWaypointDistance, _maxNewWaypointDistance);
    Vector2 dir = Random.insideUnitCircle;
    Vector3 vector = new Vector3(dir.x * dist, 0, dir.y * dist);
    Vector3 rawPos = transform.position + vector;
    rawPos = new Vector3(rawPos.x, 1000, rawPos.z);
    
    Physics.Raycast(rawPos, Vector3.down, out RaycastHit hit, 5000f);
    if (hit.collider == null) {
      Debug.LogError("PassiveAI: GetRandomWaypoint could not find terrain position! ");
      return Vector3.zero;
    }
    return hit.point;
  }
  
  private void Start() {
    _state = AIState.Wandering;
  }

  private void Update() {
    if (_state == AIState.Wandering) {
      
      if (GetDistanceToPlayer() < _watchIfDistanceLess && _firstWanderFrame) {
        _firstWanderFrame = true;
      }
      if (GetDistanceToPlayer() < _watchIfDistanceLess && !_firstWanderFrame) {
        _firstWatchingFrame = true;
        _state = AIState.Watching;
      }
      else {
        if (GetDistanceToPlayer() < _watchIfDistanceLess && _firstWanderFrame) {
          _firstWanderFrame = true;
        }
        if (Vector3.Distance(transform.position, _waypoint) < _waypointArriveRadius || _waypoint == Vector3.zero) {
          _waypoint = GetRandomWaypoint();
        }
        Vector3 inputVector = (_waypoint - transform.position).normalized;
        inputVector = new Vector3(inputVector.x, 0, inputVector.z);
        _movement.SetInputVector(transform.InverseTransformDirection(inputVector));
        _movement.SetBoolInputs(false, false);
      }
      
    }
    if (_state == AIState.Watching) {
      
      if (_firstWatchingFrame) {
        _timeWatchLimit = Random.Range(_timeWatchMin, _timeWatchMax);
      }
      if (_timeSinceWatchStart > _timeWatchLimit) {
        _timeSinceWatchStart = 0;
        _timeWatchLimit = 0;
        _firstWanderFrame = true;
        _state = AIState.Wandering;
      }
      else {
        _firstWatchingFrame = false;
        _movement.SetInputVector(Vector3.zero);
        _timeSinceWatchStart += Time.deltaTime;
      }
      
    }
    if (_state == AIState.Fleeing) {
      
      
      
    }
    if (_state == AIState.StartGrouping) {
      
    }
    if (_state == AIState.Grouping) {
      
    }
  }

  private void OnDrawGizmos() {
    if (_player == null) {
      _player = GameObject.FindWithTag("Player");
    }
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(_waypoint, _waypointArriveRadius);
    Gizmos.DrawWireSphere(_player.transform.position, _watchIfDistanceLess);
  }
  
  

}
