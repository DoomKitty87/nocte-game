using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceObject : MonoBehaviour
{
    [SerializeField] private GameObject _testPrefab;
    private Vector2[] positions;
    public int numberOfInstancesInOneDirection;
    public float distanceBetweenInstance;
    public float randomLocationOffset;

    private WorldGenInfo.AmalgamNoiseParams _noiseParams;

    private void Awake() {
        positions = new Vector2[numberOfInstancesInOneDirection * numberOfInstancesInOneDirection];

        int currentInstance = 0;
        for (int i = 0; i < numberOfInstancesInOneDirection; i++) {
            for (int j = 0; j < numberOfInstancesInOneDirection; j++) {
                positions[currentInstance] = new Vector2(i * distanceBetweenInstance, j * distanceBetweenInstance) + new Vector2(UnityEngine.Random.Range(-randomLocationOffset / 2, randomLocationOffset), UnityEngine.Random.Range(-randomLocationOffset / 2, randomLocationOffset));
                currentInstance++;
            }
        }
    }

    private void Start() {
        GameObject parentObject = new GameObject();
        parentObject.name = "Foliage Holder";
        Instantiate(parentObject);
        
        for (int i = 0; i < positions.Length; i++) {
            Instantiate(_testPrefab, new Vector3(positions[i].x, AmalgamNoise.GetPosition(positions[i].x, positions[i].y), positions[i].y), Quaternion.identity, parent: parentObject.transform);
        }
    }
}
