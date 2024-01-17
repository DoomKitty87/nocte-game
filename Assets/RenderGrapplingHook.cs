using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderGrapplingHook : MonoBehaviour
{
    public PlayerGrapple _playerGrapple;

    private Spring spring;
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;
    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;

    private bool _grappling;
        
    void Awake() {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }
    
    private void LateUpdate() {
        DrawGrapple();
    }

    private void DrawGrapple() {
        if (!_playerGrapple._renderGrapple) {
            if (_grappling) {
                lr.positionCount = 0;
                _grappling = false;
            }

            return;
        }

        _grappling = true;
        
        if (lr.positionCount == 0) {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }
        
        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var grapplePoint = _playerGrapple._grapplePoint;
        var gunTipPosition = _playerGrapple._gunEnd.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for (var i = 0; i < quality + 1; i++) {
            var delta = i / (float) quality;
            var offset = waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                         affectCurve.Evaluate(delta) * up;
            
            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}

public class Spring {
    private float strength;
    private float damper;
    private float target;
    private float velocity;
    private float value;

    public void Update(float deltaTime) {
        var direction = target - value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(target - value) * strength;
        velocity += (force * direction - velocity * damper) * deltaTime;
        value += velocity * deltaTime;
    }

    public void Reset() {
        velocity = 0f;
        value = 0f;
    }
        
    public void SetValue(float value) {
        this.value = value;
    }
        
    public void SetTarget(float target) {
        this.target = target;
    }

    public void SetDamper(float damper) {
        this.damper = damper;
    }
        
    public void SetStrength(float strength) {
        this.strength = strength;
    }

    public void SetVelocity(float velocity) {
        this.velocity = velocity;
    }
        
    public float Value => value;
}
