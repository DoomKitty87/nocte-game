using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerWorldGeneratorCompatibility : MonoBehaviour
{
    [SerializeField] private WorldGenerator _worldGeneratorObject;

    private bool _hasInitialized;

    private void Awake() {
        if (_worldGeneratorObject == null) {
            Debug.LogWarning("World Generator Object is missing. Either assign the script or disable " +
                             "PlayerWorldGeneratorCompatibility script if there is no WorldGenerator in scene.");
            enabled = false;
        }
    }

    private void Update() {
        _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position 
            - new Vector3(0, GetComponent<Collider>().bounds.extents.y / 2, 0));
        
        // Delayed start
        if (!_hasInitialized && Time.time > 0.1 && Time.timeScale > 0) {
            RaycastHit hit;
            Physics.Raycast(Vector3.up * 2500, Vector3.down, out hit);
            transform.position = hit.point + Vector3.up * 5;
            _hasInitialized = true;
        }
    }
}
