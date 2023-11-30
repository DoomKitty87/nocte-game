using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerGrapple : MonoBehaviour
{
    
    [Header("References")] 
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _gunEnd;
    [SerializeField] private LayerMask _grapplable;
    
    private LineRenderer _lineRenderer;
    private PlayerController _playerController;
    private Rigidbody _rb;
    
    [Header("Grappling")]
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleDelayTime;
    
    [SerializeField] private float _grappleForce;
    
    [Space(10)]
    [SerializeField] private AnimationCurve _grapplingForceCurve;
    [SerializeField] private float _timeToReachMaxForce;
    [Space(10)]
    
    [SerializeField, Range(0, 1)] private float _airFriction;
    [SerializeField] private float _gravityWhileGrappling;
    [SerializeField, Range(0, 1)] private float _playerLookWhileGrapplingPower;

    private Vector3 _grapplePoint;

    [Header("Cooldown")]
    [SerializeField] private float _grapplingCoolDown;
    private float _grapplingCoolDownTimer;

    private bool _renderGrapple;
    
    public bool _currentlyGrappling;
    
    private void Start() {
        _playerController = GetComponent<PlayerController>();
        _lineRenderer = GetComponent<LineRenderer>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_grapplingCoolDownTimer > 0)
            _grapplingCoolDownTimer -= Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.Mouse0)) StartGrapple();
        if (Input.GetKeyUp(KeyCode.Mouse0)) StopGrapple();
    }
    
    private void LateUpdate()
    {
        if (_renderGrapple)
           _lineRenderer.SetPosition(0, _gunEnd.position);
    }

    private void FixedUpdate() {
        if (_currentlyGrappling) {
            _rb.AddForce(GetGrappleForce(), ForceMode.Acceleration);
            _rb.AddForce(GetFrictionForce(), ForceMode.Force);
            CancelGrapple();
        }
    }

    private void StartGrapple()
    {
        if (_grapplingCoolDownTimer > 0) return;
        
        RaycastHit hit;
        if(Physics.Raycast(_camera.position, _camera.forward, out hit, _maxGrappleDistance, _grapplable))
        {
            _grapplePoint = hit.point;
            _grappleVectorNormal = (transform.position - _grapplePoint).normalized;
            _renderGrapple = true;
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(1, _grapplePoint);
            Invoke(nameof(ExecuteGrapple), _grappleDelayTime);
        }
    }


    private float _time;
    
    private void ExecuteGrapple() {
        if (!Input.GetKey(KeyCode.Mouse0)) return;
        _currentlyGrappling = true;
        _playerController.Gravity = -_gravityWhileGrappling;
        _playerController._useGroundFriction = false;

        _time = 0;

    }

    private Vector3 GetGrappleForce() {
        Vector3 playerDirection = (_grapplePoint - _camera.position).normalized;
        Vector3 lookDirectionOffset = _camera.forward;
        Vector3 direction = (playerDirection + (lookDirectionOffset * _playerLookWhileGrapplingPower)).normalized;

        _time += Time.deltaTime / _timeToReachMaxForce;

        float animationForce = _grapplingForceCurve.Evaluate(_time);
        return direction * _grappleForce * animationForce;
    }

    private Vector3 _grappleVectorNormal;
    private void CancelGrapple() {
        Vector3 playerVector = transform.position - _grapplePoint;
        float dotProduct = Vector3.Dot(playerVector, _grappleVectorNormal);
        if (dotProduct <= 0) StopGrapple();
    }

    private Vector3 GetFrictionForce() {
        Vector3 direction = -_rb.velocity.normalized;
        float magnitude = _rb.velocity.magnitude * _airFriction;
        
        return direction * magnitude;
    }
    
    public void StopGrapple()
    {
        if (_currentlyGrappling) {
            _currentlyGrappling = false;
            _playerController.ResetGravity();
            _playerController._useGroundFriction = true;
            _grapplingCoolDownTimer = _grapplingCoolDown;
        }

        _renderGrapple = false;

        _lineRenderer.enabled = false;
    }
}
