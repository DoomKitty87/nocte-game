using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static NoiseMaps;
using Random = UnityEngine.Random;

public class HydraulicErosion : MonoBehaviour
{
    
    public int xSize = 32;
    public int zSize = 32;
    
    public int xTiles = 1;
    public int zTiles = 1;

    public float scale = 1000;

    public float amplitude = 50;
    public int octaves = 4;
    
    public Material material;

    private Mesh[] _meshes;

    private void Start()
    {
        scale = 1 / scale;
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
                Mesh msh = mf.mesh;
                msh.vertices = NoiseMaps.GenerateTerrain(x * xSize, z * zSize, xSize, zSize, scale, amplitude, octaves);
                Erode(msh);
                WindTriangles(msh);
                UpdateMesh(msh);
                go.transform.position = new Vector3(x * xSize, 0, z * zSize);
            }
        }
    }
    
    private void Erode(Mesh targetMesh)
    {
        Vector3[] vertices = targetMesh.vertices;
        float[] heightMap = new float[vertices.Length];
        int size = (int) Mathf.Sqrt(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            heightMap[i] = (vertices[i].y);
        }
        for (int i = 0; i < 100000; i++)
        {
            int dropletPosition = Random.Range(0, heightMap.Length);
            float sediment = 0;
            float water = 1;
            while (water > 0)
            {   
                int lowestNeighbour = dropletPosition;
                for (int j = 0; j < 9; j++)
                {
                    if (j == 4) continue;
                    int neighbour = i - size - 1 + (j % 3) + (j / 3 * size);
                    if (neighbour >= heightMap.Length || neighbour < 0) continue;
                    if (heightMap[neighbour] < heightMap[lowestNeighbour])
                    {
                        lowestNeighbour = neighbour;
                    }
                }

                if (heightMap[lowestNeighbour] < heightMap[dropletPosition])
                {
                    if (sediment < 0.9f)
                    {
                        heightMap[lowestNeighbour] -= (1 - sediment) * 0.25f;
                        sediment += (1 - sediment) * 0.25f;
                    }
                    else
                    {
                        heightMap[lowestNeighbour] += sediment * 0.25f;
                        sediment *= 0.75f;
                    }

                    dropletPosition = lowestNeighbour;
                }
                water -= 0.001f;
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = heightMap[i];
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