using ObserverPattern;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class PlayerGrapple : MonoBehaviour
{
    private PlayerMovementHandler _handler;
    private PlayerMove _playerMove;
    
    // Used in PlayerMove for indexing
    private int _forceIndex;
    
    [Header("References")] 
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _gunEnd;
    [SerializeField] private LayerMask _grapplable;
    private LineRenderer _lineRenderer;
    
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
    
    private void OnEnable() {
        _handler = GetComponent<PlayerMovementHandler>();
    }
    

    private void Start() {
        _lineRenderer = GetComponent<LineRenderer>();
        _playerMove = GetComponent<PlayerMove>();

        //_forceIndex = _playerMove.RegisterForce();
    }

    private void Update()
    {
        if (_grapplingCoolDownTimer > 0)
            _grapplingCoolDownTimer -= Time.deltaTime;

        if (_currentlyGrappling) {
            //_playerMove._additionalForces[_forceIndex] = _grappleForce;
        }
        else if (_currentState != "air") {
            _grappleForce = Vector3.zero;
            //_playerMove._additionalForces[_forceIndex] = Vector3.zero;
        }
    }
    
    private void LateUpdate()
    {
        if (_renderGrapple)
           _lineRenderer.SetPosition(0, _gunEnd.position);
    }

    private void StartGrapple()
    {
        if (_grapplingCoolDownTimer > 0) return;

        _renderGrapple = true;
        
        RaycastHit hit;
        if(Physics.Raycast(_camera.position, _camera.forward, out hit, _maxGrappleDistance, _grapplable))
        {
            _grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), _grappleDelayTime);
        }
        else
        {
            _grapplePoint = _camera.position + _camera.forward * _maxGrappleDistance;

            Invoke(nameof(StopGrapple), _grappleDelayTime);
        }

        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(1, _grapplePoint);
    }

    private void ExecuteGrapple() {
        if (_renderGrapple == false) return;
        
        _currentlyGrappling = true;
        
        _handler.State = "Grapple";
        
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = _grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + _overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = _overshootYAxis;
        
        _grappleForce = CalculateJumpVelocity(transform.position, _grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }
    
    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight) {
        //float gravity = -_playerMove._gravityStrength;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        
        //Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        //Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
        //                                      + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        //return (velocityXZ + velocityY) * Time.deltaTime;
        return Vector3.zero;
    }
    
    public void StopGrapple()
    {
        _renderGrapple = false;
        _currentlyGrappling = false;

        _handler.State = "Air";
        
        _grapplingCoolDownTimer = _grapplingCoolDown;

        _lineRenderer.enabled = false;
    }
    
    public void OnSceneChangeNotify(string previousState, string currentState) {
        _currentState = currentState;
    }

    public void OnGrappleNotify(int type) {
        switch (type) {
            case 0:
                StartGrapple();
                break;
            case 1:
                break;
            case 2:
                if (_renderGrapple) 
                    StopGrapple();
                break;
            default:
                break;
        }
    }
    
}
