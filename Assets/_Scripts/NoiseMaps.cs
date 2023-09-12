using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.noise;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System.Linq;

public static class NoiseMaps
{

    [BurstCompile]
    private struct SimplexNoiseJob : IJobParallelFor
    {

        public int xSize;
        public float xOffset;
        public float zOffset;
        public float scale;
        public float amplitude;
        public int octaves;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % xSize;
            float sampleZ = index / xSize;
            float noise = 0;
            for (int i = 0; i < octaves; i++)
            {
                float octaveNoise = snoise(new float2((sampleX + xOffset) * (scale * Mathf.Pow(2, i)), (sampleZ + zOffset) * (scale * Mathf.Pow(2, i)))) * amplitude * (1 / Mathf.Pow(2, i));
                noise += octaveNoise;
            }
            output[index] = Math.Min(Math.Abs(noise), amplitude * 2);
        }

    }

    public static Vector3[] GenerateTerrain(float xOffset, float zOffset, int xSize, int zSize, float scale, float amplitude, int octaves, AnimationCurve easeCurve)
    {
        var jobResult = new NativeArray<float>((xSize + 1) * (xSize + 1), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            xSize = xSize + 1,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = scale,
            amplitude = amplitude,
            octaves = octaves,
            output = jobResult
        };

        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        float maxHeight = amplitude * 2;
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {   
                float y = jobResult[i] / maxHeight;
                //Using animationCurve
                //vertices[i] = new Vector3(x, easeCurve.Evaluate(jobResult[i] / maxHeight) * maxHeight, z);
                
                //Using custom falloff function
                //vertices[i] = new Vector3(x, Mathf.Pow(1 - y * y, 3) * maxHeight, z);
                //vertices[i] = new Vector3(x, ((25 * Mathf.Pow(y, 4)) - (48 * Mathf.Pow(y, 5)) + (25 * Mathf.Pow(y, 6)) - Mathf.Pow(y, 10)) * maxHeight, z);
                vertices[i] = new Vector3(x, ((6 * Mathf.Pow(y, 5)) - (15 * Mathf.Pow(y, 4)) + (10 * Mathf.Pow(y, 3))) * maxHeight, z);
                i++;
            }
        }

        jobResult.Dispose();
        return vertices;
    }

}