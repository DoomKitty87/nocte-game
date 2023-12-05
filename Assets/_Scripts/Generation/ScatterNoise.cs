using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using static Unity.Mathematics.noise;
using Unity.Mathematics;

public static class ScatterNoise
{

  [BurstCompile]
  private struct Point : IJobParallelFor
  {

    [ReadOnly] public float size;

    [WriteOnly] public NativeArray<float> output;

    public void Execute(int index) {
      output[index] = cellular(new float2((index % size) / size * 1000, (index / size) / size * 1000)).x;
    }

  }

  [BurstCompile]
  private struct OrganicScatter : IJobParallelFor
  {

    [ReadOnly] public float size;
    [ReadOnly] public float cutoff;
    [ReadOnly] public float xOffset;
    [ReadOnly] public float zOffset;
    [ReadOnly] public float xResolution;
    [ReadOnly] public float zResolution;
    [ReadOnly] public float scale;
    [ReadOnly] public float octaves;
    [ReadOnly] public float warpScale;
    [ReadOnly] public float warpStrength;

    [WriteOnly] public NativeArray<bool> output;

    public void Execute(int index) {
      float x = index % size * xResolution + xOffset;
      float z = ((int) index / size) * zResolution + zOffset;
      float warpValue = snoise(new float2(x * warpScale, z * warpScale)) * warpStrength;
      x += warpValue;
      z += warpValue;
      float value = 0;
      float normalization = 0;
      for (int i = 0; i < octaves; i++) {
        float scale = Mathf.Pow(2, i);
        float amplitude = Mathf.Pow(0.5f, i);
        value += snoise(new float2(x * scale, z * scale)) * amplitude;
        normalization += amplitude;
      }
      value /= normalization;
      value = (value + 1) / 2;
      if (value < cutoff) output[index] = true;
      else output[index] = false;
    }
  }

  public static Vector2[] GeneratePoints(int size, float cutoff) {
    NativeArray<float> output = new NativeArray<float>(size * size, Allocator.TempJob);
    Point job = new Point {
      size = size,
      output = output
    };
    JobHandle handle = job.Schedule(size * size, 64);
    handle.Complete();
    List<Vector2> points = new List<Vector2>();
    for (int i = 0; i < output.Length; i++) {
      if (output[i] < cutoff) points.Add(new Vector2(i % size, i / size));
    }
    output.Dispose();
    return points.ToArray();
  }

  public static int[] ScatterOrganic(int size, float cutoff, float xOffset, float zOffset, float xResolution, float zResolution, float scale, float octaves, float warpScale, float warpStrength) {
    NativeArray<bool> output = new NativeArray<bool>(size * size, Allocator.TempJob);
    OrganicScatter job = new OrganicScatter {
      size = size,
      cutoff = cutoff,
      xOffset = xOffset,
      zOffset = zOffset,
      xResolution = xResolution,
      zResolution = zResolution,
      scale = 1 / scale,
      octaves = octaves,
      warpScale = 1 / warpScale,
      warpStrength = warpStrength,
      output = output
    };
    JobHandle handle = job.Schedule(size * size, size);
    handle.Complete();
    int pointCount = 0;
    for (int i = 0; i < output.Length; i++) if (output[i]) pointCount++;
    int[] points = new int[pointCount];
    for (int i = 0, j = 0; i < output.Length; i++) {
      if (output[i]) {
        points[j] = i;
        j++;
      }
    }
    output.Dispose();
    return points;
  }

}