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
    public struct SimplexNoiseJob : IJobParallelFor
    {

        public int chunkSize;
        public float xOffset;
        public float zOffset;
        public float scale;
        public float amplitude;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % chunkSize;
            float sampleZ = index / chunkSize;
            float noiseValue1 = snoise(new float2((sampleX + xOffset) * scale, (sampleZ + zOffset) * scale));
            float noise1 = noiseValue1 * (noiseValue1 > 0.4f ? Mathf.SmoothStep(0, 1, (noiseValue1 - 0.4f) / 0.6f) * amplitude * 15 + amplitude : amplitude);
            float noiseValue2 = snoise(new float2((sampleX + xOffset) * (scale / 2), (sampleZ * zOffset) * (scale / 2)));
            float noise2 = noiseValue2 * amplitude / 2;
            float noiseValue3 = snoise(new float2((sampleX + xOffset) * (scale / 4), (sampleZ * zOffset) * (scale / 4)));
            float noise3 = noiseValue3 * amplitude / 4;
            float noise = noise1 + noise2 + noise3;
            output[index] = noise;
        }

    }

    public static Vector3[] GenerateTerrain(float xOffset, float zOffset, int xSize, float scale, float amplitude)
    {
        var jobResult = new NativeArray<float>((xSize + 1) * (xSize + 1), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            chunkSize = xSize + 1,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = scale,
            amplitude = amplitude,
            output = jobResult
        };

        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        Vector3[] vertices = new Vector3[(xSize + 1) * (xSize + 1)];
        for (int z = 0, i = 0; z <= xSize; z++)
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