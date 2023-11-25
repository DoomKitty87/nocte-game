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

    private void Awake() {
        if (enabled && _worldGeneratorObject == null) {
            Debug.LogWarning("World Generator Object is missing. Either assign the script or disable " +
                             "PlayerWorldGeneratorCompatibility script if there is no WorldGenerator in scene.");
            enabled = false;
        }
    }

    private void Start() {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Update() {
        _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position 
            - new Vector3(0, GetComponent<Collider>().bounds.extents.y / 2, 0));
        
        // Delayed start
        if (!_hasInitialized && Time.time > _timeToInitialize && Time.timeScale != 0) {
            RaycastHit hit;
            
            // Arbitrarily high origin, pointed downwards due to World Gen Chunks only having upwards facing colliders
            if (Physics.Raycast(Vector3.up * 10000, Vector3.down, out hit, Mathf.Infinity, _groundMask)) { 
                _hasInitialized = true;
            
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
    }
}
