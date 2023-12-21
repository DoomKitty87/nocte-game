using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerWorldGeneratorCompatibility : MonoBehaviour
{
    [SerializeField] private WorldGenerator _worldGeneratorObject;
    [SerializeField] private VisualEffect _rain;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _timeToInitialize;
    private bool _hasInitialized;

    // private Rigidbody _rb;
    // private RigidbodyConstraints _rbConstraints;

    private void Awake() {
        if (enabled && _worldGeneratorObject == null) {
            Debug.LogWarning("World Generator Object is missing. Either assign the script or disable " +
                             "PlayerWorldGeneratorCompatibility script if there is no WorldGenerator in scene.");
            enabled = false;
        }
    }

    private void Start() {
        // It works so i aint fixing it
        GetComponent<PlayerController>().enabled = false;
    }

    private void Update() {
        _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position 
            - new Vector3(0, GetComponent<Collider>().bounds.extents.y / 2, 0));

        _rain.SetVector3("PlayerPos", transform.position);
        
        // Delayed start
        if (!_hasInitialized && Time.time > _timeToInitialize && Time.timeScale != 0) {
            
            // Arbitrarily high origin, pointed downwards due to World Gen Chunks only having upwards facing colliders
            if (Physics.Raycast(Vector3.up * 10000, Vector3.down, out var hit, Mathf.Infinity, _groundMask)) {
                transform.position = hit.point + Vector3.up * 2f;
                _hasInitialized = true;
                GetComponent<PlayerController>().enabled = true;
            }
        }
    }
}
