using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using static Unity.Mathematics.noise;
using Unity.Mathematics;
using System;
using static WorldGenInfo.AmalgamNoiseParams;

public static class AmalgamNoise
{

  [BurstCompile]
  public struct NoiseJob : IJobParallelFor
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

    [ReadOnly] public float amplitudePower;

    [WriteOnly] public NativeArray<float> output;

    public void Execute(int index) {
      int sampleX = (int) (index % size - overlap);
      int sampleZ = (int) (index / size - overlap);

      float sharpnessValue = sharpnessMean + sharpnessAmplitude * Mathf.Pow(snoise(new float2((sampleX * xResolution + xOffset) * sharpnessScale, (sampleZ * zResolution + zOffset) * sharpnessScale)), 3);
      float secondarySharpness = sharpnessAmplitude / 5 * snoise(new float2((sampleX * xResolution + xOffset) * sharpnessScale * 5, (sampleZ * zResolution + zOffset) * sharpnessScale * 5));
      sharpnessValue += secondarySharpness;
      float scaleValue = scaleMean + scaleAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * scaleScale, (sampleZ * zResolution + zOffset) * scaleScale));
      float amplitudeValue = snoise(new float2((sampleX * xResolution + xOffset) * amplitudeScale, (sampleZ * zResolution + zOffset) * amplitudeScale));
      if (amplitudeValue < 0) amplitudeValue = Mathf.Abs(amplitudeValue) * 0.3f;
      amplitudeValue = Mathf.Pow(amplitudeValue, amplitudePower);
      float amplitudeValue0 = amplitudeValue;
      //sharpnessValue += (Mathf.Max(0, amplitudeValue0 - 0.6f));
      //sharpnessValue = Mathf.Min(sharpnessValue, 1);
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
        // sample = sample * sample * (3.0f - 2.0f * sample);
        float samplehigh = sample * (2 - sample);
        float samplelow = sample * sample * (0.5f + (0.5f * sample));
        sample = Mathf.Lerp(samplelow, samplehigh, amplitudeValue0 + 0.5f);
        float billow = Mathf.Abs(sample);
        float ridge = 1 - billow;
        sample = Mathf.Lerp(billow, ridge, sharpnessValue);
        sample *= octaveAmp;
        height += sample;
        normalization += octaveAmp;
      }
      
      height /= normalization;
      height = height * height * (3.0f - 2.0f * height);
      height *= amplitudeValue;
      output[index] = height;
    }

  }

  [BurstCompile]
  private struct NoiseJobPoint : IJobParallelFor
  {
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

    [ReadOnly] public float amplitudePower;

    [ReadOnly] public NativeArray<float> inputX;
    [ReadOnly] public NativeArray<float> inputZ;

    [WriteOnly] public NativeArray<float> output;

    public void Execute(int index) {
      float sampleX = inputX[index];
      float sampleZ = inputZ[index];

      float sharpnessValue = sharpnessMean + sharpnessAmplitude * Mathf.Pow(snoise(new float2((sampleX) * sharpnessScale, (sampleZ) * sharpnessScale)), 3);
      float secondarySharpness = sharpnessAmplitude / 5 * snoise(new float2((sampleX) * sharpnessScale * 5, (sampleZ) * sharpnessScale * 5));
      sharpnessValue += secondarySharpness;
      float scaleValue = scaleMean + scaleAmplitude * snoise(new float2((sampleX) * scaleScale, (sampleZ) * scaleScale));
      float amplitudeValue = snoise(new float2((sampleX) * amplitudeScale, (sampleZ) * amplitudeScale));
      if (amplitudeValue < 0) amplitudeValue = Mathf.Abs(amplitudeValue) * 0.3f;
      amplitudeValue = Mathf.Pow(amplitudeValue, amplitudePower);
      float amplitudeValue0 = amplitudeValue;
      amplitudeValue = amplitudeMean + amplitudeAmplitude * amplitudeValue;
      float warpStrengthValue = warpStrengthMean + warpStrengthAmplitude * snoise(new float2((sampleX) * warpStrengthScale, (sampleZ) * warpStrengthScale));
      float warpScaleValue = warpScaleMean + warpScaleAmplitude * snoise(new float2((sampleX) * warpScaleScale, (sampleZ) * warpScaleScale));
      if (scaleValue != 0) scaleValue = 1f / scaleValue;
      if (warpScaleValue != 0) warpScaleValue = 1f / warpScaleValue;

      float height = 0;
      float normalization = 0;

      for (int i = 0; i < octaves; i++) {
        float octaveScale = Mathf.Pow(lacunarity, i);
        float octaveAmp = Mathf.Pow(persistence, i);
        float x = (sampleX) * scaleValue * octaveScale;
        float y = (sampleZ) * scaleValue * octaveScale;
        float warpValue = warpStrengthValue * snoise(new float2(x * warpScaleValue, y * warpScaleValue));
        float sample = snoise(new float2(x + warpValue, y + warpValue));
        float samplehigh = sample * (2 - sample);
        float samplelow = sample * sample * (0.5f + (0.5f * sample));
        sample = Mathf.Lerp(samplelow, samplehigh, amplitudeValue0 + 0.5f);
        float billow = Mathf.Abs(sample);
        float ridge = 1 - billow;
        sample = Mathf.Lerp(billow, ridge, sharpnessValue);
        sample *= octaveAmp;
        height += sample;
        normalization += octaveAmp;
      }
      
      height /= normalization;
      height = height * height * (3.0f - 2.0f * height);
      height *= amplitudeValue;
      output[index] = height;
    }
  }

  [BurstCompile]
  public struct RiverJob : IJobParallelFor
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
        value += snoise(new float2(x, z)) * octaveAmp;
        normalization += octaveAmp;
      }
      
      value /= normalization;
      output[index] = Mathf.Abs(value);
    }
    
  }

  [BurstCompile]
  private struct RiverJobPoint : IJobParallelFor
  {

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
    [ReadOnly] public NativeArray<float> inputX;
    [ReadOnly] public NativeArray<float> inputZ;

    public void Execute(int index) {
      float sampleX = inputX[index];
      float sampleZ = inputZ[index];
      float value = 0;
      float normalization = 0;

      for (int i = 0; i < octaves; i++) {
        float octaveScale = Mathf.Pow(lacunarity, i);
        float octaveAmp = Mathf.Pow(persistence, i);
        float x = (sampleX) * scale * octaveScale;
        float z = (sampleZ) * scale * octaveScale;
        float warpValue = snoise(new float2(x * warpScale, z * warpScale)) * warpStrength;
        x += warpValue;
        z += warpValue;
        value += snoise(new float2(x, z)) * octaveAmp;
        normalization += octaveAmp;
      }
      
      value /= normalization;
      output[index] = Mathf.Abs(value);
    }
  }

  public static Vector3[] GenerateTerrain(int size, int lodFactor, float xOffset, float zOffset, float xResolution, float zResolution, int octaves, float lacunarity, float persistence, float sharpnessScale, float sharpnessAmplitude, float sharpnessMean, float scaleScale, float scaleAmplitude, float scaleMean, float amplitudeScale, float amplitudeAmplitude, float amplitudeMean, float warpStrengthScale, float warpStrengthAmplitude, float warpStrengthMean, float warpScaleScale, float warpScaleAmplitude, float warpScaleMean, float amplitudePower) {
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
      amplitudePower = amplitudePower,
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

  public static float GetRiverValue(float xPosition, float zPosition, float scale, int octaves, float lacunarity, float persistence, float warpScale, float warpStrength) {
    scale = 1f / scale;
    warpScale = 1f / warpScale;
    float value = 0;
    float normalization = 0;

    for (int i = 0; i < octaves; i++) {
      float octaveScale = Mathf.Pow(lacunarity, i);
      float octaveAmp = Mathf.Pow(persistence, i);
      float x = xPosition * scale * octaveScale;
      float z = zPosition * scale * octaveScale;
      float warpValue = snoise(new float2(x * warpScale, z * warpScale)) * warpStrength;
      x += warpValue;
      z += warpValue;
      value += snoise(new float2(x, z)) * octaveAmp;
      normalization += octaveAmp;
    }
    
    value /= normalization;
    return Mathf.Abs(value);
  }

  public static float[] GetRiverValues(float[] xPositions, float[] zPositions, float scale, int octaves, float lacunarity, float persistence, float warpScale, float warpStrength) {
    NativeArray<float> output = new NativeArray<float>(xPositions.Length, Allocator.TempJob);
    NativeArray<float> xInputs = new NativeArray<float>(xPositions.Length, Allocator.TempJob);
    NativeArray<float> zInputs = new NativeArray<float>(xPositions.Length, Allocator.TempJob);
    for (int i = 0; i < xPositions.Length; i++) xInputs[i] = xPositions[i];
    for (int i = 0; i < xPositions.Length; i++) zInputs[i] = zPositions[i];
    RiverJobPoint job = new RiverJobPoint {
      inputX = xInputs,
      inputZ = zInputs,
      octaves = octaves,
      lacunarity = lacunarity,
      persistence = persistence,
      scale = 1f / scale,
      warpScale = 1f / warpScale,
      warpStrength = warpStrength,
      output = output
    };
    JobHandle handle = job.Schedule(xPositions.Length, 16);
    handle.Complete();
    float[] values = new float[xPositions.Length];
    for (int i = 0; i < values.Length; i++) values[i] = output[i];
    return values;
  }

  public static float GetPosition(float xPosition, float zPosition) {
    float sharpnessScale = 1f / NOISEPARAMS_sharpnessScale;
    float scaleScale = 1f / NOISEPARAMS_scaleScale;
    float amplitudeScale = 1f / NOISEPARAMS_amplitudeScale;
    float warpStrengthScale = 1f / NOISEPARAMS_warpStrengthScale;
    float warpScaleScale = 1f / NOISEPARAMS_warpScaleScale;
    float sharpnessValue = NOISEPARAMS_sharpnessMean + NOISEPARAMS_sharpnessAmplitude * Mathf.Pow(snoise(new float2(xPosition * sharpnessScale, zPosition * sharpnessScale)), 3);
    float secondarySharpness = NOISEPARAMS_sharpnessAmplitude / 5 * snoise(new float2(xPosition * sharpnessScale * 5, zPosition * sharpnessScale * 5));
    sharpnessValue += secondarySharpness;
    float scaleValue = NOISEPARAMS_scaleMean + NOISEPARAMS_scaleAmplitude * snoise(new float2(xPosition * scaleScale, zPosition * scaleScale));
    float amplitudeValue = snoise(new float2(xPosition * amplitudeScale, zPosition * amplitudeScale));
    if (amplitudeValue < 0) amplitudeValue = Mathf.Abs(amplitudeValue) * 0.3f;
    amplitudeValue = Mathf.Pow(amplitudeValue, NOISEPARAMS_amplitudePower);
    float amplitudeValue0 = amplitudeValue;
    amplitudeValue = NOISEPARAMS_amplitudeMean + NOISEPARAMS_amplitudeAmplitude * amplitudeValue;
    float warpStrengthValue = NOISEPARAMS_warpStrengthMean + NOISEPARAMS_warpStrengthAmplitude * snoise(new float2(xPosition * warpStrengthScale, zPosition * warpStrengthScale));
    float warpScaleValue = NOISEPARAMS_warpScaleMean + NOISEPARAMS_warpScaleAmplitude * snoise(new float2(xPosition * warpScaleScale, zPosition * warpScaleScale));
    if (scaleValue != 0) scaleValue = 1f / scaleValue;
    if (warpScaleValue != 0) warpScaleValue = 1f / warpScaleValue;

    float height = 0;
    float normalization = 0;

    for (int i = 0; i < NOISEPARAMS_octaves; i++) {
      float octaveScale = Mathf.Pow(NOISEPARAMS_lacunarity, i);
      float octaveAmp = Mathf.Pow(NOISEPARAMS_persistence, i);
      float x = xPosition * scaleValue * octaveScale;
      float y = zPosition * scaleValue * octaveScale;
      float warpValue = warpStrengthValue * snoise(new float2(x * warpScaleValue, y * warpScaleValue));
      float sample = snoise(new float2(x + warpValue, y + warpValue));
      float samplehigh = sample * (2 - sample);
      float samplelow = sample * sample * (0.5f + (0.5f * sample));
      sample = Mathf.Lerp(samplelow, samplehigh, amplitudeValue0 + 0.5f);
      float billow = Mathf.Abs(sample);
      float ridge = 1 - billow;
      sample = Mathf.Lerp(billow, ridge, sharpnessValue);
      sample *= octaveAmp;
      height += sample;
      normalization += octaveAmp;
    }
    
    height /= normalization;
    height = height * height * (3.0f - 2.0f * height);
    height *= amplitudeValue;
    return height;
  }
    
  public static float[] GetPositionParallel(float[] xPositions, float[] zPositions) {
    NativeArray<float> output = new NativeArray<float>(xPositions.Length, Allocator.TempJob);
    NativeArray<float> xInputs = new NativeArray<float>(xPositions.Length, Allocator.TempJob);
    NativeArray<float> zInputs = new NativeArray<float>(xPositions.Length, Allocator.TempJob);
    for (int i = 0; i < xPositions.Length; i++) xInputs[i] = xPositions[i];
    for (int i = 0; i < xPositions.Length; i++) zInputs[i] = zPositions[i];
    
    
    
    NoiseJobPoint job = new NoiseJobPoint {
      inputX = xInputs,
      inputZ = zInputs,
      octaves = NOISEPARAMS_octaves,
      lacunarity = NOISEPARAMS_lacunarity,
      persistence = NOISEPARAMS_persistence,
      sharpnessScale = 1f / NOISEPARAMS_sharpnessScale,
      sharpnessAmplitude = NOISEPARAMS_sharpnessAmplitude,
      sharpnessMean = NOISEPARAMS_sharpnessMean,
      scaleScale = 1f / NOISEPARAMS_scaleScale,
      scaleAmplitude = NOISEPARAMS_scaleAmplitude,
      scaleMean = NOISEPARAMS_scaleMean,
      amplitudeScale = 1f / NOISEPARAMS_amplitudeScale,
      amplitudeAmplitude = NOISEPARAMS_amplitudeAmplitude,
      amplitudeMean = NOISEPARAMS_amplitudeMean,
      warpStrengthScale = 1f / NOISEPARAMS_warpStrengthScale,
      warpStrengthAmplitude = NOISEPARAMS_warpStrengthAmplitude,
      warpStrengthMean = NOISEPARAMS_warpStrengthMean,
      warpScaleScale = 1f / NOISEPARAMS_warpScaleScale,
      warpScaleAmplitude = NOISEPARAMS_warpScaleAmplitude,
      warpScaleMean = NOISEPARAMS_warpScaleMean,
      amplitudePower = NOISEPARAMS_amplitudePower,
      output = output
    };
    JobHandle handle = job.Schedule(xPositions.Length, 16);
    handle.Complete();
    float[] values = new float[xPositions.Length];
    for (int i = 0; i < values.Length; i++) values[i] = output[i];
    output.Dispose();
    xInputs.Dispose();
    zInputs.Dispose();
    return values;
  }
}