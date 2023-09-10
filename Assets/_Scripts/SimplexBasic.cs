using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.noise;

public class SimplexBasic : MonoBehaviour
{

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    public int xSize = 50;
    public int zSize = 50;

    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        GenerateGrid();
        WindTriangles();
        UpdateMesh();
    }

    private void Update()
    {
    }

    private void GenerateGrid()
    {
        _vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = snoise(new float2(x * 0.01f, z * 0.01f)) * 50 + snoise(new float2(x * 0.1f, z * 0.1f)) * 2 + snoise(new float2(x * 15, z * 15)) * 0.1f;
                _vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
    }

    private void WindTriangles()
    {
        _triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                _triangles[tris] = vert + 0;
                _triangles[tris + 1] = vert + xSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + xSize + 1;
                _triangles[tris + 5] = vert + xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    private void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;

        _mesh.RecalculateNormals();
    }

}