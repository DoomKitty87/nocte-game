using ObserverPattern;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class PlayerGrappleImpulse : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _gunEnd;
    [SerializeField] private LayerMask _grapplable;
    private LineRenderer _lineRenderer;
    private Rigidbody _rb;
    
    [Header("Grappling")]
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleDelayTime;
    [SerializeField] private float _overshootYAxis;

    private Vector3 _grapplePoint;

    [Header("Cooldown")]
    [SerializeField] private float _grapplingCoolDown;
    private float _grapplingCoolDownTimer;

    private bool _renderGrapple;
    private bool _currentlyGrappling;
    private Vector3 _grappleForce;

    private string _currentState;
    
    

    private void Start() {
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
    
    public void StopGrapple()
    {
        if (_currentlyGrappling) {
            _currentlyGrappling = false;
            _grapplingCoolDownTimer = _grapplingCoolDown;
        }

        _renderGrapple = false;

        _lineRenderer.enabled = false;
    }

    private void ExecuteGrapple() {
        if (!Input.GetKey(KeyCode.Mouse0)) return;
        _currentlyGrappling = true;
        
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = _grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + _overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = _overshootYAxis;
        
        _grappleForce = CalculateJumpVelocity(transform.position, _grapplePoint, highestPointOnArc);
        
        _rb.AddForce(_grappleForce - _rb.velocity, ForceMode.VelocityChange);

        Invoke(nameof(StopGrapple), 1f);
    }
    
    private static Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight) {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
                                              + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        return velocityXZ + velocityY;
    }
}
