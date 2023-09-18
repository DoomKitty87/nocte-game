using System;
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
        public int octaves;
        public float xResolution;
        public float zResolution;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % xSize;
            float sampleZ = index / xSize;
            float noise = 0;
            float normalization = 0;
            for (int i = 0; i < octaves; i++)
            {
                float octaveNoise = snoise(new float2((sampleX * xResolution + xOffset) * (scale * Mathf.Pow(2, i)), (sampleZ * zResolution + zOffset) * (scale * Mathf.Pow(2, i)))) * amplitude * (1 / Mathf.Pow(2, i));
                noise += octaveNoise;
                normalization += 1 / Mathf.Pow(2, i);
            }

            noise = Mathf.Abs(noise / normalization);
            output[index] = noise;
        }

    }
    
    [BurstCompile]
    private struct CellularNoiseJob : IJobParallelFor
    {

        public int xSize;
        public float xOffset;
        public float zOffset;
        public float scale;
        public float amplitude;
        public int octaves;
        public float xResolution;
        public float zResolution;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % xSize;
            float sampleZ = index / xSize;
            float noise = 0;
            float normalization = 0;
            for (int i = 0; i < octaves; i++)
            {
                float octaveNoise = cnoise(new float2((sampleX * xResolution + xOffset) * (scale * Mathf.Pow(2, i)), (sampleZ * zResolution + zOffset) * (scale * Mathf.Pow(2, i)))) * amplitude * (1 / Mathf.Pow(2, i));
                noise += octaveNoise;
                normalization += 1 / Mathf.Pow(2, i);
            }

            noise = Mathf.Abs(noise / normalization);
            output[index] = noise;
        }

    }


    [BurstCompile]
    private struct WindSimplexJob : IJobParallelFor
    {

        public int width;
        public float xOffset;
        public float zOffset;
        public float scale;

        [WriteOnly] public NativeArray<float> output;
        
        public void Execute(int index)
        {
            float sampleX = index % width;
            float sampleZ = index / width;
            float noise = snoise(new float2((sampleX + xOffset) * scale, (sampleZ + zOffset) * scale));
            output[index] = noise;
        }

    }

    public static Vector3[] GenerateTerrain(float xOffset, float zOffset, int xSize, int zSize, float scale, float amplitude, int octaves, AnimationCurve easeCurve, float xResolution=1, float zResolution=1)
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
            output = jobResult,
            xResolution = xResolution,
            zResolution = zResolution
        };

        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {   
                float y = jobResult[i] / amplitude;
                //Using animationCurve
                //vertices[i] = new Vector3(x, easeCurve.Evaluate(jobResult[i] / maxHeight) * maxHeight, z);
                
                //Using custom falloff function
                //vertices[i] = new Vector3(x, Mathf.Pow(1 - y * y, 3) * maxHeight, z);
                //vertices[i] = new Vector3(x, ((25 * Mathf.Pow(y, 4)) - (48 * Mathf.Pow(y, 5)) + (25 * Mathf.Pow(y, 6)) - Mathf.Pow(y, 10)) * maxHeight, z);
                vertices[i] = new Vector3(x * xResolution, easeCurve.Evaluate(((6 * Mathf.Pow(y, 5)) - (15 * Mathf.Pow(y, 4)) + (10 * Mathf.Pow(y, 3)))) * amplitude, z * zResolution);
                i++;
            }
        }

        jobResult.Dispose();
        return vertices;
    }
    
    public static Vector3[] GenerateTerrainLayers(float xOffset, float zOffset, int xSize, int zSize, NoiseTesting.NoiseLayer[] noiseLayers, float xResolution=1, float zResolution=1) {
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++) {
                vertices[i] = new Vector3(x * xResolution, 0, z * zResolution);
                i++;
            }
        }

        float normalization = 0;
        var jobResult = new NativeArray<float>(vertices.Length, Allocator.TempJob);
        for (int i = 0; i < noiseLayers.Length; i++) {

            if (noiseLayers[i].noiseType == "simplex") {
                var job = new SimplexNoiseJob() {
                    xSize = xSize + 1,
                    xOffset = xOffset,
                    zOffset = zOffset,
                    scale = 1 / noiseLayers[i].scale,
                    amplitude = noiseLayers[i].amplitude,
                    octaves = noiseLayers[i].octaves,
                    output = jobResult,
                    xResolution = xResolution,
                    zResolution = zResolution
                };
                var handle = job.Schedule(jobResult.Length, 32);
                handle.Complete();
            }

            else if (noiseLayers[i].noiseType == "cellular") {
                var job = new CellularNoiseJob() {
                    xSize = xSize + 1,
                    xOffset = xOffset,
                    zOffset = zOffset,
                    scale = 1 / noiseLayers[i].scale,
                    amplitude = noiseLayers[i].amplitude,
                    octaves = noiseLayers[i].octaves,
                    output = jobResult,
                    xResolution = xResolution,
                    zResolution = zResolution
                };
                var handle = job.Schedule(jobResult.Length, 32);
                handle.Complete();
            }

            for (int j = 0; j < vertices.Length; j++) {
                vertices[j].y += (i > 0 ? noiseLayers[i].primaryEase.Evaluate(vertices[j].y / normalization) : 1) * noiseLayers[i].easeCurve.Evaluate(jobResult[j] / Mathf.Abs(noiseLayers[i].amplitude)) * noiseLayers[i].amplitude;
            }

            normalization += noiseLayers[i].amplitude;
        }

        jobResult.Dispose();
        return vertices;
    }
    
    public static float[] GenerateLargeScaleHeight(float xOffset, float zOffset, int xSize, int zSize, float scale, float amplitude, AnimationCurve easeCurve, float xResolution=1, float zResolution=1)
    {
        var jobResult = new NativeArray<float>((xSize + 1) * (xSize + 1), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            xSize = xSize + 1,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = scale,
            amplitude = amplitude,
            octaves = 1,
            output = jobResult,
            xResolution = xResolution,
            zResolution = zResolution
        };

        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        float[] vertices = new float[(xSize + 1) * (zSize + 1)];
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {   
                float y = jobResult[i] / amplitude;
                vertices[i] = easeCurve.Evaluate(((6 * Mathf.Pow(y, 5)) - (15 * Mathf.Pow(y, 4)) + (10 * Mathf.Pow(y, 3)))) * amplitude;
                i++;
            }
        }

        jobResult.Dispose();
        return vertices;
    }

    public static float[] GenerateTemperatureMap(Vector3[] heightMap, float xOffset, float zOffset, int xSize, int zSize, float scale, AnimationCurve easeCurve, float xResolution=1, float zResolution=1)
    {
        
        float[] temperatureMap = new float[heightMap.Length];
        
        var jobResult = new NativeArray<float>((xSize + 1) * (zSize + 1), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            xSize = xSize + 1,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = scale,
            xResolution = xResolution,
            zResolution = zResolution,
            amplitude = 1,
            octaves = 1,
            output = jobResult
        };
        
        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        
        for (int i = 0; i < temperatureMap.Length; i++)
        {
            temperatureMap[i] = jobResult[i] / (1 + easeCurve.Evaluate(heightMap[i].y));
        }
        
        jobResult.Dispose();

        return temperatureMap;
    }

    public static float[] GenerateHumidityMap(Vector3[] heightMap, float[] temperatureMap, float xOffset, float zOffset, int xSize, int zSize, float scale, AnimationCurve easeCurve, float xResolution=1, float zResolution=1)
    {
        float[] humidityMap = new float[heightMap.Length];
        
        var jobResult = new NativeArray<float>((xSize + 1) * (xSize + 1), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            xSize = xSize + 1,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = scale,
            xResolution = xResolution,
            zResolution = zResolution,
            amplitude = 1,
            octaves = 1,
            output = jobResult
        };
        
        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();

        for (int i = 0; i < humidityMap.Length; i++)
        {
            humidityMap[i] = easeCurve.Evaluate(heightMap[i].y * temperatureMap[i]) * jobResult[i];
        }

        jobResult.Dispose();

        return humidityMap;
    }

    public static float[] GenerateWindMap(int width, int depth, float xOffset, float zOffset, float scale) {
        float[] windMap;
        var jobResult = new NativeArray<float>(width * depth, Allocator.TempJob);

        var job = new WindSimplexJob() {
            width = width,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = scale,
            output = jobResult
        };
        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        windMap = jobResult.ToArray();
        jobResult.Dispose();
        return windMap;
    }

}