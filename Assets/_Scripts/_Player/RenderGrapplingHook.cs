// Many thanks to:
// https://www.youtube.com/watch?v=8nENcDnxeVE
// https://github.com/affaxltd/rope-tutorial

using UnityEngine;

public class RenderGrapplingHook : MonoBehaviour
{
    private PlayerGrapple _playerGrapple;

    private Spring _spring;
    private LineRenderer _lineRenderer;
    private Vector3 _currentGrapplePosition;
    public int _quality;
    public float _damper;
    public float _strength;
    public float _velocity;
    public float _waveCount;
    public float _waveHeight;
    public AnimationCurve _affectCurve;

    private bool _grappling;
        
    void Awake() {
        _playerGrapple = GetComponent<PlayerGrapple>();
        _lineRenderer = GetComponent<LineRenderer>();
        _spring = new Spring();
        _spring.SetTarget(0);
    }
    
    private void LateUpdate() {
        DrawGrapple();
    }

    private void DrawGrapple() {
        if (!_playerGrapple._renderGrapple) {
            // Cancel grapple
            if (_grappling) {
                _lineRenderer.positionCount = 0;
                _lineRenderer.enabled = false;
                _grappling = false;
            }   

            return;
        }

        // Initiate grapple
        if (!_grappling) {
            _currentGrapplePosition = _playerGrapple._gunEnd.position;
            _spring.SetVelocity(_velocity);
            _lineRenderer.positionCount = _quality + 1;
            _lineRenderer.enabled = true;
            _grappling = true;
        }
        
        _spring.SetDamper(_damper);
        _spring.SetStrength(_strength);
        _spring.Update(Time.fixedDeltaTime);

        var grapplePoint = _playerGrapple._grapplePoint;
        var gunTipPosition = _playerGrapple._gunEnd.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        _currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, grapplePoint, Time.fixedDeltaTime * 12f);

        for (var i = 0; i < _quality + 1; i++) {
            var delta = i / (float) _quality;
            var offset = _waveHeight * Mathf.Sin(delta * _waveCount * Mathf.PI) * _spring.Value *
                         _affectCurve.Evaluate(delta) * up;
            _lineRenderer.SetPosition(i, Vector3.Lerp(gunTipPosition, _currentGrapplePosition, delta) + offset);
        }
    }
}

public class Spring {
    private float _strength;
    private float _damper;
    private float _target;
    private float _velocity;
    private float _value;

    public void Update(float deltaTime) {
        var direction = _target - _value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(_target - _value) * _strength;
        _velocity += (force * direction - _velocity * _damper) * deltaTime;
        _value += _velocity * deltaTime;
    }

    public void Reset() {
        _velocity = 0f;
        _value = 0f;
    }
        
    public void SetValue(float value) {
        this._value = value;
    }
        
    public void SetTarget(float target) {
        this._target = target;
    }

    public void SetDamper(float damper) {
        this._damper = damper;
    }
        
    public void SetStrength(float strength) {
        this._strength = strength;
    }

    public void SetVelocity(float velocity) {
        this._velocity = velocity;
    }
        
    public float Value => _value;
}
