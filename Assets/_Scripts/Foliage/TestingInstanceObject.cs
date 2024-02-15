using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingInstanceObject : MonoBehaviour
{
    [SerializeField] private GameObject _testPrefab;
    private Vector3[] positions;
    public int numberOfInstancesInOneDirection;
    public float distanceBetweenInstance;
    public float randomLocationOffset;

    public bool GPUInstance;

    public Material material;
    public Mesh mesh;

    private WorldGenInfo.AmalgamNoiseParams _noiseParams;

    private void Awake() {
        positions = new Vector3[numberOfInstancesInOneDirection * numberOfInstancesInOneDirection];

        int currentInstance = 0;
        for (int i = 0; i < numberOfInstancesInOneDirection; i++) {
            for (int j = 0; j < numberOfInstancesInOneDirection; j++) {
                float xPosition = i * distanceBetweenInstance + UnityEngine.Random.Range(
                    -randomLocationOffset / 2,
                    randomLocationOffset / 2
                );
                float zPosition = j * distanceBetweenInstance + UnityEngine.Random.Range(
                    -randomLocationOffset / 2, 
                    randomLocationOffset / 2
                );
                float yPosition = AmalgamNoise.GetPosition(xPosition, zPosition);
                positions[currentInstance] = new Vector3(xPosition, yPosition, zPosition);
                currentInstance++;
            }
        }
    }

    private void Start() {
        if (GPUInstance) return;
        GameObject parentObject = new GameObject {
            name = "Foliage Holder"
        };
        Instantiate(parentObject);

        foreach (var t in positions) {
            Instantiate(
                _testPrefab, t,
                Quaternion.identity,
                parent: parentObject.transform
            );
        }
    }

    private void Update() {
        if (!GPUInstance) return;
        RenderParams rp = new RenderParams(material);
        Matrix4x4[] instData = new Matrix4x4[positions.Length];
        for (int i = 0; i < positions.Length; i++) instData[i] = Matrix4x4.Translate(positions[i]);
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
    }
}
