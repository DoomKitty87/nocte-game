using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldGeneratorCompatibility : MonoBehaviour
{
    [SerializeField] private WorldGenerator _worldGeneratorObject;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _timeToInitialize;
    private bool _hasInitialized;

    private Rigidbody _rb;
    private RigidbodyConstraints _rbConstraints;

    private void Awake() {
        if (enabled && _worldGeneratorObject == null) {
            Debug.LogWarning("World Generator Object is missing. Either assign the script or disable " +
                             "PlayerWorldGeneratorCompatibility script if there is no WorldGenerator in scene.");
            enabled = false;
        }
    }

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _rbConstraints = _rb.constraints;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        
        // It works so i aint fixing it
        GetComponent<PlayerController>().enabled = false;
    }

    private void Update() {
        _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position 
            - new Vector3(0, GetComponent<Collider>().bounds.extents.y / 2, 0));
        
        // Delayed start
        if (!_hasInitialized && Time.time > _timeToInitialize && Time.timeScale != 0) {
            
            // Arbitrarily high origin, pointed downwards due to World Gen Chunks only having upwards facing colliders
            if (Physics.Raycast(Vector3.up * 10000, Vector3.down, out var hit, Mathf.Infinity, _groundMask)) {
                transform.position = hit.point + Vector3.up * 2f;
                _hasInitialized = true;
                _rb.constraints = _rbConstraints;
                GetComponent<PlayerController>().enabled = true;
            }
        }
    }
}
