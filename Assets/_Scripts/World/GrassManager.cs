using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoBehaviour
{
    [SerializeField] private int _distance;
    
    private Vector3[][] _vertices;
    private int[][] _tris;
    private Bounds[] _bounds;
    private Vector3[] _positions;
    private WorldGenerator _worldGenerator;
    private RenderGrass _renderGrass;
    
    private void Awake() {
        _worldGenerator = GetComponent<WorldGenerator>();
        _renderGrass = GetComponent<RenderGrass>();
    }

    public void GenerateGrass() {
        int numberOfChunks = (2 * _distance - 1) * (2 * _distance - 1);
        var tileMesh = _worldGenerator.GetVertices(_distance);
        int numberOfVertices = tileMesh.Item1[0].vertexCount;
        for (int i = 0; i < numberOfChunks; i++) {
            Mesh currentMesh = tileMesh.Item1[i];
            _tris[i] = currentMesh.triangles;
            _vertices[i] = currentMesh.vertices;
            
            for (int j = 0; j < numberOfVertices; j++) {
                _vertices[i][j] += _positions[i];
            }
        }
        
        _renderGrass._numberOfChunks = numberOfChunks;
        _renderGrass._tris = _tris;
        _renderGrass._vertices = _vertices;
        _renderGrass._bounds = _bounds;

        _renderGrass._regenerateGrass = true;
        _renderGrass._enableGrass = true;
    }
}
