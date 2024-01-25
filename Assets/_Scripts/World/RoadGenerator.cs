using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public static class RoadGenerator
{

  public static Vector2[] PlanePointsFromLine(Vector2[] points, float width, float noiseAmplitude) {

    Vector2[] perpVectors = new Vector2[points.Length];

    for (int i = 0; i < points.Length - 1; i++) {
      Vector2 difference = (points[i + 1] - points[i]).normalized;
      float rads = Mathf.Atan2(difference.y, difference.x) + Mathf.PI / 2;
      Vector2 perp = new Vector2(Mathf.Cos(rads), Mathf.Sin(rads));
      perpVectors[i] += perp;
      perpVectors[i + 1] += perp;
    }

    Vector2[] outPoints = new Vector2[points.Length * 3];

    for (int i = 0; i < points.Length; i++) {
      perpVectors[i].Normalize();
      float noiseValue = noise.snoise(new float2(points[i].x, points[i].y) * 0.001f) * noiseAmplitude;
      outPoints[i * 3] = points[i] - perpVectors[i] * width + perpVectors[i] * noiseValue;
      outPoints[i * 3 + 1] = points[i] + perpVectors[i] * noiseValue;
      outPoints[i * 3 + 2] = points[i] + perpVectors[i] * width + perpVectors[i] * noiseValue;
    }

    return outPoints;
  }

  public static Mesh MeshFromPlane(Vector3[] points, float depth, float inset, float bevel) {

    int pointCount = points.Length / 3;
    Vector3[] vertices = new Vector3[pointCount * 6];
    int halfVerts = pointCount * 3;
    for (int i = 0; i < points.Length; i++) {
      vertices[i] = points[i] - Vector3.up * inset;
      vertices[i + halfVerts] = points[i] + Vector3.up * depth - Vector3.up * inset;
    }

    for (int i = 0; i < points.Length / 3; i++) {
      vertices[i * 3 + halfVerts] = Vector3.Lerp(vertices[i * 3 + halfVerts], vertices[i * 3 + 1 + halfVerts], bevel);
      vertices[i * 3 + 2 + halfVerts] = Vector3.Lerp(vertices[i * 3 + 2 + halfVerts], vertices[i * 3 + 1 + halfVerts], bevel);
    }
    int[] triangles = new int[(pointCount - 1) * 36 + 24];

    for (int i = pointCount * 3; i < pointCount * 6; i++) vertices[i] += Vector3.up * depth;
    for (int i = 0; i < pointCount - 1; i++) {
      triangles[i * 36] = i * 3 + 0;
      triangles[i * 36 + 1] = i * 3 + 3;
      triangles[i * 36 + 2] = i * 3 + 1;

      triangles[i * 36 + 3] = i * 3 + 3;
      triangles[i * 36 + 4] = i * 3 + 4;
      triangles[i * 36 + 5] = i * 3 + 1;

      triangles[i * 36 + 6] = i * 3 + 1;
      triangles[i * 36 + 7] = i * 3 + 4;
      triangles[i * 36 + 8] = i * 3 + 2;

      triangles[i * 36 + 9] = i * 3 + 4;
      triangles[i * 36 + 10] = i * 3 + 5;
      triangles[i * 36 + 11] = i * 3 + 2;

      triangles[i * 36 + 12] = i * 3 + halfVerts;
      triangles[i * 36 + 13] = i * 3 + 3;
      triangles[i * 36 + 14] = i * 3 + 0;

      triangles[i * 36 + 15] = i * 3 + halfVerts;
      triangles[i * 36 + 16] = i * 3 + 3 + halfVerts;
      triangles[i * 36 + 17] = i * 3 + 3;

      triangles[i * 36 + 18] = i * 3 + 2 + halfVerts;
      triangles[i * 36 + 19] = i * 3 + 2;
      triangles[i * 36 + 20] = i * 3 + 5;

      triangles[i * 36 + 21] = i * 3 + 5 + halfVerts;
      triangles[i * 36 + 22] = i * 3 + 2 + halfVerts;
      triangles[i * 36 + 23] = i * 3 + 5;

      triangles[i * 36 + 24] = i * 3 + 1 + halfVerts;
      triangles[i * 36 + 25] = i * 3 + 3 + halfVerts;
      triangles[i * 36 + 26] = i * 3 + 0 + halfVerts;
      
      triangles[i * 36 + 27] = i * 3 + 1 + halfVerts;
      triangles[i * 36 + 28] = i * 3 + 4 + halfVerts;
      triangles[i * 36 + 29] = i * 3 + 3 + halfVerts;

      triangles[i * 36 + 30] = i * 3 + 2 + halfVerts;
      triangles[i * 36 + 31] = i * 3 + 4 + halfVerts;
      triangles[i * 36 + 32] = i * 3 + 1 + halfVerts;

      triangles[i * 36 + 33] = i * 3 + 2 + halfVerts;
      triangles[i * 36 + 34] = i * 3 + 5 + halfVerts;
      triangles[i * 36 + 35] = i * 3 + 4 + halfVerts;
    }

    triangles[triangles.Length - 24] = halfVerts + 1;
    triangles[triangles.Length - 23] = halfVerts;
    triangles[triangles.Length - 22] = 0;

    triangles[triangles.Length - 21] = 1;
    triangles[triangles.Length - 20] = halfVerts + 1;
    triangles[triangles.Length - 19] = 0;

    triangles[triangles.Length - 18] = halfVerts + 2;
    triangles[triangles.Length - 17] = halfVerts + 1;
    triangles[triangles.Length - 16] = 1;

    triangles[triangles.Length - 15] = 2;
    triangles[triangles.Length - 14] = halfVerts + 2;
    triangles[triangles.Length - 13] = 1;

    triangles[triangles.Length - 12] = halfVerts - 3;
    triangles[triangles.Length - 11] = halfVerts - 3 + halfVerts;
    triangles[triangles.Length - 10] = halfVerts - 2 + halfVerts;

    triangles[triangles.Length - 9] = halfVerts - 3;
    triangles[triangles.Length - 8] = halfVerts - 2 + halfVerts;
    triangles[triangles.Length - 7] = halfVerts - 2;

    triangles[triangles.Length - 6] = halfVerts - 2;
    triangles[triangles.Length - 5] = halfVerts - 2 + halfVerts;
    triangles[triangles.Length - 4] = halfVerts - 1 + halfVerts;

    triangles[triangles.Length - 3] = halfVerts - 2;
    triangles[triangles.Length - 2] = halfVerts - 1 + halfVerts;
    triangles[triangles.Length - 1] = halfVerts - 1;

    Mesh msh = new Mesh();
    msh.vertices = vertices;
    msh.triangles = triangles;
    msh.RecalculateNormals();
    msh.RecalculateTangents();

    return msh;
  }

}