using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.noise;

public class SimplexTiled : MonoBehaviour
{

    public int xSize = 32;
    public int zSize = 32;
    
    public int xTiles = 1;
    public int zTiles = 1;

    public float largeScale = 1000;
    public float mediumScale = 50;
    public float smallScale = 0.1f;

    public float largeAmplitude = 50;
    public float mediumAmplitude = 10;
    public float smallAmplitude = 0.1f;

    public Material material;

    private Mesh[] _meshes;

    private void Start()
    {
        largeScale = 1 / largeScale;
        mediumScale = 1 / mediumScale;
        smallScale = 1 / smallScale;
        for (int z = 0; z < zTiles; z++)
        {
            for (int x = 0; x < xTiles; x++)
            {
                GameObject go = new GameObject("Tile");
                go.transform.parent = transform;
                
                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = material;
                mf.mesh = new Mesh();
                GenerateGrid(mf.mesh, x * xSize, z * zSize);
                WindTriangles(mf.mesh);
                UpdateMesh(mf.mesh);
                go.transform.position = new Vector3(x * xSize, 0, z * zSize);
            }
        }
    }

    private void GenerateGrid(Mesh targetMesh, float xOffset, float zOffset)
    {
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = snoise(new float2((x + xOffset) * largeScale, (z + zOffset) * largeScale)) * largeAmplitude + snoise(new float2((x + xOffset) * mediumScale, (z + zOffset) * mediumScale)) * (Math.Abs(snoise(new float2((x + xOffset) * mediumScale, (z + zOffset) * mediumScale))) > 0.6f ? mediumAmplitude * ((Math.Abs(snoise(new float2((x + xOffset) * mediumScale, (z + zOffset) * mediumScale))) - 0.6f) / 0.4f) : 0) + snoise(new float2((x + xOffset) * smallScale, (z + zOffset) * smallScale)) * smallAmplitude;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
        
        targetMesh.vertices = vertices;
    }

    private void WindTriangles(Mesh targetMesh)
    {
        int[] triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        targetMesh.triangles = triangles;
    }

    private static void UpdateMesh(Mesh targetMesh)
    {
        targetMesh.RecalculateNormals();
    }

}