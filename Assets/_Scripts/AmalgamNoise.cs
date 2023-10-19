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
      float sampleX = index % size;
      float sampleZ = index / size;

      // Compute all complementary values

      float sharpnessValue = sharpnessMean + sharpnessAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * sharpnessScale, (sampleZ * zResolution + zOffset) * sharpnessScale));

      float scaleValue = scaleMean + scaleAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * scaleScale, (sampleZ * zResolution + zOffset) * scaleScale));

      float amplitudeValue = amplitudeMean + amplitudeAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * amplitudeScale, (sampleZ * zResolution + zOffset) * amplitudeScale));

      float warpStrengthValue = warpStrengthMean + warpStrengthAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * warpStrengthScale, (sampleZ * zResolution + zOffset) * warpStrengthScale));
      
      float warpScaleValue = warpScaleMean + warpScaleAmplitude * snoise(new float2((sampleX * xResolution + xOffset) * warpScaleScale, (sampleZ * zResolution + zOffset) * warpScaleScale));
      
      if (scaleValue != 0) scaleValue = 1f / scaleValue;
      if (warpScaleValue != 0) warpScaleValue = 1f / warpScaleValue;

      // Compute height value

      float height = 0;
      float normalization = 0;

      for (int i = 0; i < octaves; i++) {
        float x = (sampleX * xResolution + xOffset) * scaleValue * Mathf.Pow(lacunarity, i);
        float y = (sampleZ * zResolution + zOffset) * scaleValue * Mathf.Pow(lacunarity, i);
        float warpValue = warpStrengthValue * snoise(new float2(x * warpScaleValue, y * warpScaleValue));
        float sample = snoise(new float2(x + warpValue, y + warpValue));
        float billow = Mathf.Abs(sample);
        float ridge = 1 - billow;
        sample = Mathf.Lerp(billow, ridge, sharpnessValue);
        sample *= Mathf.Pow(persistence, i) * amplitudeValue;
        height += sample;
        normalization += Mathf.Pow(persistence, i);
      }
      
      height /= normalization;
      output[index] = height;
    }

  }

  public static Vector3[] GenerateTerrain(int size, float xOffset, float zOffset, float xResolution, float zResolution, int octaves, float lacunarity, float persistence, float sharpnessScale, float sharpnessAmplitude, float sharpnessMean, float scaleScale, float scaleAmplitude, float scaleMean, float amplitudeScale, float amplitudeAmplitude, float amplitudeMean, float warpStrengthScale, float warpStrengthAmplitude, float warpStrengthMean, float warpScaleScale, float warpScaleAmplitude, float warpScaleMean) {
    size += 3;
    NativeArray<float> output = new NativeArray<float>(size * size, Allocator.TempJob);
    NoiseJob job = new NoiseJob {
      size = size,
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
      vertices[i] = new Vector3(i % size * xResolution, output[i], i / size * zResolution);
    }
    output.Dispose();
    return vertices;
  }
}