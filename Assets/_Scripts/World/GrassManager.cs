using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoBehaviour
{
    [SerializeField] private int _distance;

    private Vector3[][] _vertices;
    private int[][] _tris;
    private WorldGenerator _worldGenerator;
    private RenderGrass _renderGrass;
    
    private void Awake() {
        _worldGenerator = GetComponent<WorldGenerator>();
    }

    public void GenerateGrass() {
        var vertices = _worldGenerator.GetVertices(_distance);
        _vertices = vertices.Item1;
        _tris = vertices.Item2;

        _renderGrass._numberOfChunks = (2 * _distance - 1) * (2 * _distance - 1);
        _renderGrass._vertices = _vertices;
        _renderGrass._tris = _tris;

        _renderGrass._regenerateGrass = true;
    }

    
}
