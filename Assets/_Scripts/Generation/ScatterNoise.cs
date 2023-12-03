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

}