using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AlternativePlayerGrapple : MonoBehaviour
{
    
    [Header("References")] 
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _gunEnd;
    [SerializeField] private LayerMask _grapplable;
    private LineRenderer _lineRenderer;
    private SpringJoint _spring;
    
    [Header("Grappling")]
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleDelayTime;
    
    [SerializeField] private float _springStrength;
    [SerializeField] private float _springDamper;
    [SerializeField] private float _springMassScale;
    [SerializeField] private float _grappleStrengthLerpValue;

    private Vector3 _grapplePoint;

    [Header("Cooldown")]
    [SerializeField] private float _grapplingCoolDown;
    private float _grapplingCoolDownTimer;

    private bool _renderGrapple;
    private Vector3 _grappleForce;
    
    public bool _currentlyGrappling;
    
    private void Start() {
        _lineRenderer = GetComponent<LineRenderer>();
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

    private void StartGrapple()
    {
        if (_grapplingCoolDownTimer > 0) return;
        
        RaycastHit hit;
        if(Physics.Raycast(_camera.position, _camera.forward, out hit, _maxGrappleDistance, _grapplable))
        {
            _grapplePoint = hit.point;
            _renderGrapple = true;
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(1, _grapplePoint);
            Invoke(nameof(ExecuteGrapple), _grappleDelayTime);
        }
    }

    private void ExecuteGrapple() {
        if (!Input.GetKey(KeyCode.Mouse0)) return;
        _currentlyGrappling = true;

        SetUpSpringJoint();
        StartCoroutine(LerpTowardsLocation(Vector3.Distance(transform.position, _grapplePoint)));

        //Invoke(nameof(StopGrapple), 1f);
    }

    private void SetUpSpringJoint() {
        _spring = gameObject.AddComponent<SpringJoint>();
        _spring.autoConfigureConnectedAnchor = false;
        _spring.connectedAnchor = _grapplePoint;
        
        _spring.maxDistance = 0;
        _spring.minDistance = 0;

        _spring.spring = 0;
        _spring.damper = _springDamper;
        _spring.massScale = _springMassScale;
    }

    /// <summary>
    /// Causes the spring joint to slowly increase in strength proportional to the distance between player
    /// and grapple point
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private IEnumerator LerpTowardsLocation(float distance) {
        float currentSpringStrength = 0;
        while (currentSpringStrength < _springStrength) {
            currentSpringStrength += _grappleStrengthLerpValue / distance;
            _spring.spring = currentSpringStrength;
            yield return null;
        }

        _spring.spring = _springStrength;
    }
    
    public void StopGrapple()
    {
        if (_currentlyGrappling) {
            StopAllCoroutines();
            Destroy(_spring);
            _currentlyGrappling = false;
            _grapplingCoolDownTimer = _grapplingCoolDown;
        }

        _renderGrapple = false;

        _lineRenderer.enabled = false;
    }
}
