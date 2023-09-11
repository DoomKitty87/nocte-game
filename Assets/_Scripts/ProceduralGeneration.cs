using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.noise;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class ProceduralGeneration : MonoBehaviour
{

    [BurstCompile]
    private struct SimplexNoiseJob : IJobParallelFor
    {

        public int chunkSize;
        public float xOffset;
        public float zOffset;
        public float largeScale;
        public float mediumScale;
        public float smallScale;
        public float largeAmplitude;
        public float mediumAmplitude;
        public float smallAmplitude;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % chunkSize;
            float sampleZ = index / chunkSize;
            float largeNoise = snoise(new float2((sampleX + xOffset) * largeScale, (sampleZ + zOffset) * largeScale));
            float mediumNoise = snoise(new float2((sampleX + xOffset) * mediumScale, (sampleZ + zOffset) * mediumScale));
            float smallNoise = snoise(new float2((sampleX + xOffset) * smallScale, (sampleZ + zOffset) * smallScale));
            float largeValue = largeNoise * (largeNoise > 0.4f ? Mathf.SmoothStep(0, 1, (largeNoise - 0.4f) / 0.6f) * largeAmplitude * 25 + largeAmplitude : largeAmplitude);
            float mediumValue = mediumNoise * (Math.Abs(mediumNoise) > 0.6f && largeNoise > 0.4f ? mediumAmplitude * ((Math.Abs(mediumNoise) - 0.6f) / 0.4f) : 0);
            float smallValue = smallNoise * smallAmplitude;
            float noise = largeValue + mediumValue + smallValue;
            output[index] = noise;
        }

    }

    public int xSize = 32;
    public int zSize = 32;
    
    public int xTiles = 1;
    public int zTiles = 1;

    public float largeScale = 2500;
    public float mediumScale = 300;
    public float smallScale = 0.1f;

    public float largeAmplitude = 50;
    public float mediumAmplitude = 20;
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
                Mesh msh = mf.mesh;
                msh.vertices = GenerateTerrain(x * xSize, z * zSize);
                WindTriangles(msh);
                UpdateMesh(msh);
                go.transform.position = new Vector3(x * xSize, 0, z * zSize);
            }
        }
    }

    private Vector3[] GenerateTerrain(float xOffset, float zOffset)
    {
        var jobResult = new NativeArray<float>((xSize + 1) * (zSize + 1), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            chunkSize = xSize + 1,
            xOffset = xOffset,
            zOffset = zOffset,
            largeScale = largeScale,
            mediumScale = mediumScale,
            smallScale = smallScale,
            largeAmplitude = largeAmplitude,
            mediumAmplitude = mediumAmplitude,
            smallAmplitude = smallAmplitude,
            output = jobResult
        };

        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int z = 0, i = 0; z <= xSize; z++)
        {
            for (int x = 0; x <= zSize; x++)
            {
                vertices[i] = new Vector3(x, jobResult[i], z);
                i++;
            }
        }

        jobResult.Dispose();
        return vertices;
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