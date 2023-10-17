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

        [ReadOnly] public int xSize;
        [ReadOnly] public float xOffset;
        [ReadOnly] public float zOffset;
        [ReadOnly] public float scale;
        [ReadOnly] public float amplitude;
        [ReadOnly] public int octaves;
        [ReadOnly] public float xResolution;
        [ReadOnly] public float zResolution;

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
                noise += Mathf.Abs(octaveNoise);
                normalization += 1 / Mathf.Pow(2, i);
            }

            noise = noise / normalization;
            output[index] = noise;
        }

    }
    
    [BurstCompile]
    private struct SimplexNoiseJobScale : IJobParallelFor
    {

        [ReadOnly] public int xSize;
        [ReadOnly] public float xOffset;
        [ReadOnly] public float zOffset;
        [ReadOnly] public float scaleX;
        [ReadOnly] public float scaleZ;
        [ReadOnly] public int octaves;
        [ReadOnly] public float xResolution;
        [ReadOnly] public float zResolution;
        [ReadOnly] public bool turbulent;
        [ReadOnly] public float lacunarity;
        [ReadOnly] public float persistence;
        [ReadOnly] public float warpStrength;
        [ReadOnly] public float warpSize;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % xSize;
            float sampleZ = index / xSize;
            float noise = 0;
            float normalization = 0;
            for (int i = 0; i < octaves; i++)
            {   
                float x = (sampleX * xResolution + xOffset) * (scaleX * Mathf.Pow(lacunarity, i));
                float y = (sampleZ * zResolution + zOffset) * (scaleZ * Mathf.Pow(lacunarity, i));
                float octaveNoise = snoise(new float3(x, y, warpStrength * snoise(new float2(x * warpSize, y * warpSize)))) * Mathf.Pow(persistence, i);
                noise += (turbulent ? Mathf.Abs(octaveNoise) : ((octaveNoise + 1) / 2));
                normalization += Mathf.Pow(persistence, i);
            }

            noise /= normalization;
            output[index] = noise;
        }

    }
    
    [BurstCompile]
    private struct CellularNoiseJob : IJobParallelFor
    {

        [ReadOnly] public int xSize;
        [ReadOnly] public float xOffset;
        [ReadOnly] public float zOffset;
        [ReadOnly] public float scale;
        [ReadOnly] public float amplitude;
        [ReadOnly] public int octaves;
        [ReadOnly] public float xResolution;
        [ReadOnly] public float zResolution;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % xSize;
            float sampleZ = index / xSize;
            float noise = 0;
            float normalization = 0;
            for (int i = 0; i < octaves; i++)
            {
                float octaveNoise = (cellular(new float2((sampleX * xResolution + xOffset) * (scale * Mathf.Pow(2, i)), (sampleZ * zResolution + zOffset) * (scale * Mathf.Pow(2, i)))).x * amplitude * (1 / Mathf.Pow(2, i)));
                noise += Mathf.Abs(octaveNoise);
                normalization += 1 / Mathf.Pow(2, i);
            }

            noise /= normalization;
            output[index] = noise;
        }

    }
    
    [BurstCompile]
    private struct CellularNoiseJobScale : IJobParallelFor
    {

        [ReadOnly] public int xSize;
        [ReadOnly] public float xOffset;
        [ReadOnly] public float zOffset;
        [ReadOnly] public float scaleX;
        [ReadOnly] public float scaleZ;
        [ReadOnly] public int octaves;
        [ReadOnly] public float xResolution;
        [ReadOnly] public float zResolution;
        [ReadOnly] public bool turbulent;
        [ReadOnly] public float lacunarity;
        [ReadOnly] public float persistence;
        [ReadOnly] public float warpStrength;
        [ReadOnly] public float warpSize;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index)
        {
            float sampleX = index % xSize;
            float sampleZ = index / xSize;
            float noise = 0;
            float normalization = 0;
            for (int i = 0; i < octaves; i++)
            {
                float x = (sampleX * xResolution + xOffset) * (scaleX * Mathf.Pow(lacunarity, i));
                float y = (sampleZ * zResolution + zOffset) * (scaleZ * Mathf.Pow(lacunarity, i));
                float octaveNoise = (cellular(new float3(x, y, warpStrength * cellular(new float2(x * warpSize, y * warpSize)).x)).x * Mathf.Pow(persistence, i));
                noise += (turbulent ? Mathf.Abs(octaveNoise) : ((octaveNoise + 1) / 2));
                normalization += Mathf.Pow(persistence, i);
            }

            noise /= normalization;
            output[index] = noise;
        }

    }


    [BurstCompile]
    private struct WindSimplexJob : IJobParallelFor
    {

        [ReadOnly] public int width;
        [ReadOnly] public float xOffset;
        [ReadOnly] public float zOffset;
        [ReadOnly] public float scale;

        [WriteOnly] public NativeArray<float> output;
        
        public void Execute(int index)
        {
            float sampleX = index % width;
            float sampleZ = index / width;
            float noise = snoise(new float2((sampleX + xOffset) * scale, (sampleZ + zOffset) * scale));
            output[index] = noise;
        }

    }

    [BurstCompile]
    private struct SamplePointSimplexJob : IJobParallelFor
    {

        [ReadOnly] public NativeArray<float> samplesX;
        [ReadOnly] public NativeArray<float> samplesZ;
        [ReadOnly] public float xOffset;
        [ReadOnly] public float zOffset;
        [ReadOnly] public float scale;

        [WriteOnly] public NativeArray<float> output;

        public void Execute(int index) 
        {
            output[index] = snoise(new float2((samplesX[index] + xOffset) * scale, (samplesZ[index] + zOffset) * scale));
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

    [BurstCompile]
    public static (Vector3[], int[]) GenerateTerrainBiomes(float xOffset, float zOffset, int xSize, int zSize, WorldGenerator.Biome[] biomes, float biomeScale, float xResolution = 1, float zResolution = 1) {
        Vector3[] vertices = new Vector3[(xSize + 3) * (zSize + 3)];
        int[] biomeData = new int[(xSize + 3) * (zSize + 3)];
        for (int z = 0, i = 0; z <= zSize + 2; z++)
        {
            for (int x = 0; x <= xSize + 2; x++) {
                vertices[i] = new Vector3(x * xResolution, 0, z * zResolution);
                i++;
            }
        }
        var biomeResult = new NativeArray<float>(vertices.Length, Allocator.TempJob);
        bool[] usedBiomes = new bool[biomes.Length];
        var biomeJob = new SimplexNoiseJobScale() {
            xSize = xSize + 3,
            xOffset = xOffset,
            zOffset = zOffset,
            scaleX = biomeScale,
            scaleZ = biomeScale,
            octaves = 1,
            output = biomeResult,
            xResolution = xResolution,
            zResolution = zResolution,
            turbulent = false,
            lacunarity = 2,
            persistence = 0.5f,
            warpStrength = 0,
            warpSize = 1
        };
        var biomeHandle = biomeJob.Schedule(biomeResult.Length, 32);
        biomeHandle.Complete();
        for (int i = 0; i < vertices.Length; i++) {
            float biomeNoise = biomeResult[i] * (biomes.Length - 1);
            biomeData[i] = Mathf.RoundToInt(biomeNoise);
            if (usedBiomes[Mathf.FloorToInt(biomeNoise)] == false) usedBiomes[Mathf.FloorToInt(biomeNoise)] = true;
            if (usedBiomes[Mathf.CeilToInt(biomeNoise)] == false) usedBiomes[Mathf.CeilToInt(biomeNoise)] = true;
        }

        float[] verticesBiomes = new float[biomes.Length * vertices.Length];
        var jobResult = new NativeArray<float>(vertices.Length, Allocator.TempJob);
        for (int b = 0; b < biomes.Length; b++) {
            if (!usedBiomes[b]) continue;
            float normalization = 0;
            float lowestAmp = 0;
            for (int i = 0; i < biomes[b].noiseLayers.Length; i++) {

                if (biomes[b].noiseLayers[i].noiseType == "simplex") {
                    var job = new SimplexNoiseJobScale() {
                        xSize = xSize + 3,
                        xOffset = xOffset,
                        zOffset = zOffset,
                        scaleX = 1 / biomes[b].noiseLayers[i].scaleX,
                        scaleZ = 1 / biomes[b].noiseLayers[i].scaleZ,
                        octaves = biomes[b].noiseLayers[i].octaves,
                        output = jobResult,
                        xResolution = xResolution,
                        zResolution = zResolution,
                        turbulent = biomes[b].noiseLayers[i].turbulent,
                        lacunarity = biomes[b].noiseLayers[i].lacunarity,
                        persistence = biomes[b].noiseLayers[i].persistence,
                        warpStrength = biomes[b].noiseLayers[i].warpStrength,
                        warpSize = biomes[b].noiseLayers[i].warpSize
                    };
                    var handle = job.Schedule(jobResult.Length, 32);
                    handle.Complete();
                }

                else if (biomes[b].noiseLayers[i].noiseType == "cellular") {
                    var job = new CellularNoiseJobScale() {
                        xSize = xSize + 3,
                        xOffset = xOffset,
                        zOffset = zOffset,
                        scaleX = 1 / biomes[b].noiseLayers[i].scaleX,
                        scaleZ = 1 / biomes[b].noiseLayers[i].scaleZ,
                        octaves = biomes[b].noiseLayers[i].octaves,
                        output = jobResult,
                        xResolution = xResolution,
                        zResolution = zResolution,
                        turbulent = biomes[b].noiseLayers[i].turbulent,
                        lacunarity = biomes[b].noiseLayers[i].lacunarity,
                        persistence = biomes[b].noiseLayers[i].persistence,
                        warpStrength = biomes[b].noiseLayers[i].warpStrength,
                        warpSize = biomes[b].noiseLayers[i].warpSize
                    };
                    var handle = job.Schedule(jobResult.Length, 32);
                    handle.Complete();
                }

                for (int j = 0; j < vertices.Length; j++) {
                    verticesBiomes[b * vertices.Length + j] += (i > 0 ? biomes[b].noiseLayers[i].primaryEase.Evaluate(verticesBiomes[b * vertices.Length + j] / normalization) : 1) * biomes[b].noiseLayers[i].easeCurve.Evaluate(jobResult[j]) * biomes[b].noiseLayers[i].amplitude;
                }

                normalization += biomes[b].noiseLayers[i].amplitude;
                if (biomes[b].noiseLayers[i].amplitude < lowestAmp) lowestAmp = biomes[b].noiseLayers[i].amplitude;
            }

            for (int j = 0; j < vertices.Length; j++) {
                verticesBiomes[b * vertices.Length + j] -= lowestAmp;
            }
        }
        
        for (int i = 0; i < vertices.Length; i++) {
            float biomeNoise = biomeResult[i] * (biomes.Length - 1);
            vertices[i].y = Mathf.Lerp(verticesBiomes[Mathf.FloorToInt(biomeNoise) * vertices.Length + i], verticesBiomes[Mathf.CeilToInt(biomeNoise) * vertices.Length + i], biomeNoise - Mathf.FloorToInt(biomeNoise));
        }
        jobResult.Dispose();
        biomeResult.Dispose();
        return (vertices, biomeData);
    }

    [BurstCompile]
    public static float[] GenerateRockNoise(int sideLength, float xOffset, float zOffset, float xScale, float zScale, int octaves, float xResolution, float zResolution, bool turbulent) {

      float[] heightMap = new float[sideLength * sideLength];

      var jobResult = new NativeArray<float>(heightMap.Length, Allocator.TempJob);
      var job = new SimplexNoiseJobScale() {
        xSize = sideLength,
        xOffset = xOffset,
        zOffset = zOffset,
        scaleX = 1 / xScale,
        scaleZ = 1 / zScale,
        octaves = octaves,
        output = jobResult,
        xResolution = xResolution,
        zResolution = zResolution,
        turbulent = turbulent,
        lacunarity = 2,
        persistence = 0.5f
      };

      var handle = job.Schedule(jobResult.Length, 32);
      handle.Complete();
      for (int i = 0; i < jobResult.Length; i++) heightMap[i] = jobResult[i];
      jobResult.Dispose();

      return heightMap;
    }
    
    public static Vector3[] GenerateTerrainLayers(float xOffset, float zOffset, int xSize, int zSize, WorldGenerator.NoiseLayer[] noiseLayers, float xResolution=1, float zResolution=1) {
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
        float lowestAmp = 0;
        for (int i = 0; i < noiseLayers.Length; i++) {

            if (noiseLayers[i].noiseType == "simplex") {
                var job = new SimplexNoiseJob() {
                    xSize = xSize + 1,
                    xOffset = xOffset,
                    zOffset = zOffset,
                    scale = 1 / noiseLayers[i].scaleX,
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
                    scale = 1 / noiseLayers[i].scaleX,
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
            if (noiseLayers[i].amplitude < lowestAmp) lowestAmp = noiseLayers[i].amplitude;
        }

        for (int j = 0; j < vertices.Length; j++) {
            vertices[j].y -= lowestAmp;
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
    
    [BurstCompile]
    public static float[] GenerateTemperatureMap(Vector3[] heightMap, float xOffset, float zOffset, int xSize, int zSize, float scale, AnimationCurve easeCurve, float xResolution=1, float zResolution=1)
    {
        
        float[] temperatureMap = new float[heightMap.Length];
        
        var jobResult = new NativeArray<float>((xSize + 3) * (zSize + 3), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            xSize = xSize + 3,
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

    [BurstCompile]
    public static float[] GenerateHumidityMap(Vector3[] heightMap, float[] temperatureMap, float xOffset, float zOffset, int xSize, int zSize, float scale, AnimationCurve easeCurve, float xResolution=1, float zResolution=1)
    {
        float[] humidityMap = new float[heightMap.Length];
        
        var jobResult = new NativeArray<float>((xSize + 3) * (xSize + 3), Allocator.TempJob);

        var job = new SimplexNoiseJob()
        {
            xSize = xSize + 3,
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
    
    [BurstCompile]
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

    [BurstCompile]
    public static float[] GenerateCavePass(Vector3[] samplePoints, float xOffset, float zOffset, float scale) {
        float[] weightMap = new float[samplePoints.Length];

        var jobResult = new NativeArray<float>(samplePoints.Length, Allocator.TempJob);
        var samplePointsX = new NativeArray<float>(samplePoints.Length, Allocator.TempJob);
        var samplePointsZ =  new NativeArray<float>(samplePoints.Length, Allocator.TempJob);
        for (int i = 0; i < samplePoints.Length; i++) {
            samplePointsX[i] = samplePoints[i].x;
            samplePointsZ[i] = samplePoints[i].z;
        }
        var job = new SamplePointSimplexJob() {
            samplesX = samplePointsX,
            samplesZ = samplePointsZ,
            xOffset = xOffset,
            zOffset = zOffset,
            scale = 1 / scale,
            output = jobResult
        };
        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        for (int i = 0; i < weightMap.Length; i++) weightMap[i] = jobResult[i];
        jobResult.Dispose();
        samplePointsX.Dispose();
        samplePointsZ.Dispose();

        return weightMap;
    }

}