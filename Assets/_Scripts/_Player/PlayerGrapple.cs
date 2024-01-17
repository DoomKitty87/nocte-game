using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
    public KeyCode _grappleButton = KeyCode.Mouse1;
    
    [Header("References")]
    [SerializeField] private bool _useOutsideScriptControl;
    [SerializeField] private Transform _camera;
    public Transform _gunEnd;
    [SerializeField] private LayerMask _grapplable;
    
    private NewPlayerController _playerController;
    public Rigidbody _rb;
    
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

    public Vector3 _grapplePoint;

    [Header("Cooldown")]
    [SerializeField] private float _grapplingCoolDown;
    private float _grapplingCoolDownTimer;

    public bool _renderGrapple;
    
    public bool _currentlyGrappling;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        _playerController = GetComponent<NewPlayerController>();
    }

    private void Update()
    {
        if (_grapplingCoolDownTimer > 0)
            _grapplingCoolDownTimer -= Time.deltaTime;
        if (Input.GetKeyDown(_grappleButton)) StartGrapple();
        if (Input.GetKeyUp(_grappleButton)) StopGrapple();
    }

    private void FixedUpdate() {
        if (_currentlyGrappling) {
            _rb.AddForce(GetGrappleForce(), ForceMode.Acceleration);
            _rb.AddForce(GetFrictionForce(), ForceMode.Force);
            CancelGrapple();
        }
    }

    public void StartGrapple()
    {
        if (_grapplingCoolDownTimer > 0) return;
        
        RaycastHit hit;
        if(Physics.Raycast(_camera.position, _camera.forward, out hit, _maxGrappleDistance, _grapplable))
        {
            _grapplePoint = hit.point;
            _grappleVectorNormal = (transform.position - _grapplePoint).normalized;
            _renderGrapple = true;
            Invoke(nameof(ExecuteGrapple), _grappleDelayTime);
        }
    }


    private float _time;
    
    private void ExecuteGrapple() {
        if (!Input.GetKey(_grappleButton)) return;
        _currentlyGrappling = true;
        _playerController.State = NewPlayerController.PlayerStates.Grappling;
        _time = 0;
    }

    private Vector3 GetGrappleForce() {
        Vector3 playerDirection = (_grapplePoint - _camera.position).normalized;
        Vector3 lookDirectionOffset = _camera.forward;
        Vector3 direction = (playerDirection + (lookDirectionOffset * _playerLookWhileGrapplingPower)).normalized;

        _time += Time.deltaTime / _timeToReachMaxForce;

        float animationForce = _grapplingForceCurve.Evaluate(_time);
        return _grappleForce * animationForce * direction;
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
            _grapplingCoolDownTimer = _grapplingCoolDown;
        }

        _renderGrapple = false;

        _playerController.State = NewPlayerController.PlayerStates.Air;
    }
}
