using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using static Unity.Mathematics.noise;
using Unity.Mathematics;

public static class AmalgamNoise
{

  [BurstCompile]
  private struct NoiseJob : IJobParallelFor
  {

    [ReadOnly] public int size;
    [ReadOnly] public int overlap;
    [ReadOnly] public float xOffset;
    [ReadOnly] public float zOffset;
    [ReadOnly] public float xResolution;
    [ReadOnly] public float zResolution;

    // Values set for the overall terrain

    [ReadOnly] public int octaves;
    [ReadOnly] public float lacunarity;
    [ReadOnly] public float persistence;
    
    // Values set for each component controlled by its own noise layer
    // Includes sharpness, scale, amplitude, warp strength, and warp scale
    // Each one gets scale, amplitude, and mean values

    [ReadOnly] public float sharpnessScale;
    [ReadOnly] public float sharpnessAmplitude;
    [ReadOnly] public float sharpnessMean;

    [ReadOnly] public float scaleScale;
    [ReadOnly] public float scaleAmplitude;
    [ReadOnly] public float scaleMean;

    [ReadOnly] public float amplitudeScale;
    [ReadOnly] public float amplitudeAmplitude;
    [ReadOnly] public float amplitudeMean;

    [ReadOnly] public float warpStrengthScale;
    [ReadOnly] public float warpStrengthAmplitude;
    [ReadOnly] public float warpStrengthMean;

    [ReadOnly] public float warpScaleScale;
    [ReadOnly] public float warpScaleAmplitude;
    [ReadOnly] public float warpScaleMean;

    [WriteOnly] public NativeArray<float> output;

    public void Execute(int index) {
      int sampleX = (int) (index % size - overlap);
      int sampleZ = (int) (index / size - overlap);

      float sharpnessValue = sharpnessMean + sharpnessAmplitude * Mathf.Pow(snoise(new float2((sampleX * xResolution + xOffset) * sharpnessScale, (sampleZ * zResolution + zOffset) * sharpnessScale)), 3);
      float secondarySharpness = sharpnessAmplitude / 5 * snoise(new float2((sampleX * xResolution + xOffset) * sharpnessScale * 5, (sampleZ * zResolution + zOffset) * sharpnessScale * 5));
      sharpnessValue += secondarySharpness;
      float scaleValue = scaleMean + scaleAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * scaleScale, (sampleZ * zResolution + zOffset) * scaleScale));
      float amplitudeValue = snoise(new float2((sampleX * xResolution + xOffset) * amplitudeScale, (sampleZ * zResolution + zOffset) * amplitudeScale));
      amplitudeValue = Mathf.Pow(Mathf.Max(amplitudeValue, 0), 2);
      amplitudeValue = amplitudeMean + amplitudeAmplitude * amplitudeValue;
      float warpStrengthValue = warpStrengthMean + warpStrengthAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * warpStrengthScale, (sampleZ * zResolution + zOffset) * warpStrengthScale));
      float warpScaleValue = warpScaleMean + warpScaleAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * warpScaleScale, (sampleZ * zResolution + zOffset) * warpScaleScale));
      if (scaleValue != 0) scaleValue = 1f / scaleValue;
      if (warpScaleValue != 0) warpScaleValue = 1f / warpScaleValue;

      float height = 0;
      float normalization = 0;

      for (int i = 0; i < octaves; i++) {
        float octaveScale = Mathf.Pow(lacunarity, i);
        float octaveAmp = Mathf.Pow(persistence, i);
        // Compute all complementary values
        //float sharpnessValue = sharpnessMean + octaveAmp * sharpnessAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * sharpnessScale * octaveScale, (sampleZ * zResolution + zOffset) * sharpnessScale * octaveScale));
        //float scaleValue = scaleMean + octaveAmp * scaleAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * scaleScale * octaveScale, (sampleZ * zResolution + zOffset) * scaleScale * octaveScale));
        //float amplitudeValue = amplitudeMean + octaveAmp * amplitudeAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * amplitudeScale * octaveScale, (sampleZ * zResolution + zOffset) * amplitudeScale * octaveScale));
        //float warpStrengthValue = warpStrengthMean + octaveAmp * warpStrengthAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * warpStrengthScale * octaveScale, (sampleZ * zResolution + zOffset) * warpStrengthScale * octaveScale));
        //float warpScaleValue = warpScaleMean + octaveAmp * warpScaleAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * warpScaleScale * octaveScale, (sampleZ * zResolution + zOffset) * warpScaleScale * octaveScale));
        //if (scaleValue != 0) scaleValue = 1f / scaleValue;
        //if (warpScaleValue != 0) warpScaleValue = 1f / warpScaleValue;
        // Compute height value
        float x = (sampleX * xResolution + xOffset) * scaleValue * octaveScale;
        float y = (sampleZ * zResolution + zOffset) * scaleValue * octaveScale;
        float warpValue = warpStrengthValue * snoise(new float2(x * warpScaleValue, y * warpScaleValue));
        float sample = snoise(new float2(x + warpValue, y + warpValue));
        sample = sample * sample * (3.0f - 2.0f * sample);
        float billow = Mathf.Abs(sample);
        float ridge = 1 - billow;
        sample = Mathf.Lerp(billow, ridge, sharpnessValue);
        sample *= octaveAmp * amplitudeValue;
        height += sample;
        normalization += octaveAmp;
      }
      
      height /= normalization;
      output[index] = height;
    }

  }

  [BurstCompile]
  private struct RiverJob : IJobParallelFor
  {

    [ReadOnly] public float size;
    [ReadOnly] public int octaves;
    [ReadOnly] public float lacunarity;
    [ReadOnly] public float persistence;
    [ReadOnly] public float xOffset;
    [ReadOnly] public float zOffset;
    [ReadOnly] public float xResolution;
    [ReadOnly] public float zResolution;
    [ReadOnly] public float scale;
    [ReadOnly] public float warpScale;
    [ReadOnly] public float warpStrength;

    [WriteOnly] public NativeArray<float> output;

    public void Execute(int index) {
      int sampleX = (int) index % (int) size;
      int sampleZ = (int) index / (int) size;

      float value = 0;
      float normalization = 0;

      for (int i = 0; i < octaves; i++) {
        float octaveScale = Mathf.Pow(lacunarity, i);
        float octaveAmp = Mathf.Pow(persistence, i);
        float x = (sampleX * xResolution + xOffset) * scale * octaveScale;
        float z = (sampleZ * zResolution + zOffset) * scale * octaveScale;
        float warpValue = snoise(new float2(x * warpScale, z * warpScale)) * warpStrength;
        x += warpValue;
        z += warpValue;
        value += snoise(new float2(x, z) * octaveAmp);
        normalization += octaveAmp;
      }
      
      value /= normalization;
      output[index] = Mathf.Abs(value);
    }
    
  }

  public static Vector3[] GenerateTerrain(int size, int lodFactor, float xOffset, float zOffset, float xResolution, float zResolution, int octaves, float lacunarity, float persistence, float sharpnessScale, float sharpnessAmplitude, float sharpnessMean, float scaleScale, float scaleAmplitude, float scaleMean, float amplitudeScale, float amplitudeAmplitude, float amplitudeMean, float warpStrengthScale, float warpStrengthAmplitude, float warpStrengthMean, float warpScaleScale, float warpScaleAmplitude, float warpScaleMean) {
    size = size * lodFactor + 5;
    NativeArray<float> output = new NativeArray<float>(size * size, Allocator.TempJob);
    NoiseJob job = new NoiseJob {
      size = size,
      overlap = 1,
      xOffset = xOffset,
      zOffset = zOffset,
      xResolution = xResolution,
      zResolution = zResolution,
      octaves = octaves,
      lacunarity = lacunarity,
      persistence = persistence,
      sharpnessScale = 1f / sharpnessScale,
      sharpnessAmplitude = sharpnessAmplitude,
      sharpnessMean = sharpnessMean,
      scaleScale = 1f / scaleScale,
      scaleAmplitude = scaleAmplitude,
      scaleMean = scaleMean,
      amplitudeScale = 1f / amplitudeScale,
      amplitudeAmplitude = amplitudeAmplitude,
      amplitudeMean = amplitudeMean,
      warpStrengthScale = 1f / warpStrengthScale,
      warpStrengthAmplitude = warpStrengthAmplitude,
      warpStrengthMean = warpStrengthMean,
      warpScaleScale = 1f / warpScaleScale,
      warpScaleAmplitude = warpScaleAmplitude,
      warpScaleMean = warpScaleMean,
      output = output
    };
    JobHandle handle = job.Schedule(size * size, 64);
    handle.Complete();
    Vector3[] vertices = new Vector3[size * size];
    for (int i = 0; i < size * size; i++) {
      vertices[i] = new Vector3((i % size - 1) * xResolution, output[i], (i / size - 1) * zResolution);
      //if (lodEdge && (i % size == 1 || i % size == size - 2 || i / size == 1 || i / size == size - 2 || i % size == 0 || i % size == size - 1 || i / size == 0 || i / size == size - 1)) vertices[i] -= new Vector3(0, lodFactor == 1 ? 25 : 5, 0);
    }
    output.Dispose();
    return vertices;
  }

  public static float[] GenerateRivers(int size, int lodFactor, float xOffset, float zOffset, float xResolution, float zResolution, float scale, int octaves, float lacunarity, float persistence, float warpScale, float warpStrength) {
    size = size * lodFactor + 5;
    float[] heightMap = new float[size * size];
    NativeArray<float> output = new NativeArray<float>(size * size, Allocator.TempJob);
    RiverJob job = new RiverJob {
      size = size,
      octaves = octaves,
      lacunarity = lacunarity,
      persistence = persistence,
      xOffset = xOffset,
      zOffset = zOffset,
      xResolution = xResolution,
      zResolution = zResolution,
      scale = 1f / scale,
      warpScale = 1f / warpScale,
      warpStrength = warpStrength,
      output = output
    };
    JobHandle handle = job.Schedule(size * size, size);
    handle.Complete();
    for (int i = 0; i < size * size; i++) {
      heightMap[i] = output[i];
    }
    output.Dispose();
    return heightMap;
  }
}