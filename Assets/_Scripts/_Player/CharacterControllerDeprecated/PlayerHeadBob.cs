using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHeadBob : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField, Range(0, 0.1f)] private float _amplitude = 0.015f; 
    [SerializeField, Range(0, 30)] private float _frequency = 10.0f;
    [SerializeField] private Transform _cameraHolder;

    private Rigidbody rb;

    private float _toggleSpeed = 3.0f;
    private Vector3 _startPos;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        _startPos = _cameraHolder.localPosition;
    }

    void Update()
    {
        CheckMotion();
        // ResetPosition();
        // _camera.LookAt(FocusTarget());
    }
    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * _frequency) * _amplitude;
        pos.x += Mathf.Cos(Time.time * _frequency / 2) * _amplitude * 2;
        return pos;
    }
    private void CheckMotion()
    {
        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        if (speed < _toggleSpeed) {
            ResetPosition();
            return;
        }

        PlayMotion(FootStepMotion());
    }
    private void PlayMotion(Vector3 motion)
    {
        _cameraHolder.localPosition += motion;
        Debug.Log("Happening");
    }
    
    private void ResetPosition()
    {
        if (_cameraHolder.localPosition == _startPos) return;
        _cameraHolder.localPosition = Vector3.Lerp(_cameraHolder.localPosition, _startPos, 1 * Time.deltaTime);
    }
}
