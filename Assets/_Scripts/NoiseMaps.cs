using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.noise;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

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

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % xSize;
            float sampleZ = index / xSize;
            float noiseValue1 = snoise(new float2((sampleX + xOffset) * scale, (sampleZ + zOffset) * scale));
            float noise1 = noiseValue1 * (noiseValue1 > 0.4f ? Mathf.SmoothStep(0, 1, (noiseValue1 - 0.4f) / 0.6f) * amplitude * 4 + amplitude : amplitude);
            float noiseValue2 = snoise(new float2((sampleX + xOffset) * (scale * 10), (sampleZ + zOffset) * (scale * 10)));
            float noise2 = noiseValue2 * amplitude * 0.1f;
            float noiseValue3 = snoise(new float2((sampleX + xOffset) * (scale * 1000), (sampleZ + zOffset) * (scale * 1000)));
            float noise3 = noiseValue3 * amplitude * 0.001f;
            float noise = noise1 + noise2 + noise3;
            output[index] = noise;
        }

    }

    public static Vector3[] GenerateTerrain(float xOffset, float zOffset, int xSize, int zSize, float scale, float amplitude)
    {
        var jobResult = new NativeArray<float>((xSize + 1) * (xSize + 1), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            xSize = xSize + 1,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = scale,
            amplitude = amplitude,
            output = jobResult
        };

        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3(x, jobResult[i], z);
                i++;
            }
        }

        jobResult.Dispose();
        return vertices;
    }

}