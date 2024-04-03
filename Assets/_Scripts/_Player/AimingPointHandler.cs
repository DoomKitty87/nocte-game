using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingPointHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _aimingPoint;
    [Header("Settings")]
    [SerializeField] private float _maxRaycastDistance = 1000f;
    [SerializeField] private LayerMask _raycastLayerMask;
    // Start is called before the first frame update
    void Start()
    {
        if (_mainCamera == null) {
            Debug.LogError("AimingPointHandler: Main Camera is not set.");
        }
        if (_aimingPoint == null) {
            Debug.LogError("AimingPointHandler: Aiming Point is not set.");
        }    
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out RaycastHit hit, _maxRaycastDistance, _raycastLayerMask)) {
            _aimingPoint.position = hit.point;
        }
        else {
            _aimingPoint.position = _mainCamera.transform.position + _mainCamera.transform.forward * _maxRaycastDistance;
        }
        
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_aimingPoint.transform.position, 0.1f);
    }
}
