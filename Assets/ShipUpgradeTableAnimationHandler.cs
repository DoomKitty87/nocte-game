using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class ShipUpgradeTableAnimationHandler : MonoBehaviour
{
    [SerializeField] private Material _hologramEffect;
    [SerializeField] private Transform _hologramTransform;

    public GameObject testTransform;

    private Camera mainCamera;

    private Vector3 _offset;
    
    private static readonly int Scale = Shader.PropertyToID("_Scale");
    private static readonly int Offset = Shader.PropertyToID("_Offset");
    private static readonly int Center = Shader.PropertyToID("_Center");

    private void OnEnable() {
        mainCamera = Camera.main;
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 hitPoint = Vector3.zero;
            if (Physics.Raycast(ray, out hit)) {
                _offset = hit.point;
            }
        }

        if (Input.GetMouseButton(0)) {
            UpdateTableVariables();
        }

        if (Input.GetMouseButtonUp(0)) {
            _offset = _hologramEffect.GetVector(Center);
        }
    }

    private void UpdateTableVariables() {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 hitPoint = Vector3.zero;
        if (Physics.Raycast(ray, out hit)) {
            hitPoint = hit.point;
        }

        Vector3 position = hitPoint - _offset;
        
        // _hologramEffect.SetFloat(Scale, 5 * ((Mathf.Sin(Time.time) + 2) / 3));
        _hologramEffect.SetVector(
            Center, 
            Vector3.Lerp((Vector3)_hologramEffect.GetVector(Center), position, 4f * Time.deltaTime));
    }
}
