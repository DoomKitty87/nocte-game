using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceObject : MonoBehaviour
{
    [SerializeField] private GameObject _testPrefab;
    public Vector2[] positions;
    public int numberOfInstances;

    private WorldGenInfo.AmalgamNoiseParams _noiseParams;

    private void Start() {
        for (int i = 0; i < numberOfInstances; i++) {
            positions[i].x = 5f;
            positions[i].y = 5 * i;
            Instantiate(_testPrefab, new Vector3(positions[i].x, AmalgamNoise.GetPosition(positions[i].x + , positions[i].y), positions[i].y), Quaternion.identity);
        }
    }
}
