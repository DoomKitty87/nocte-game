using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class RoadGenerator
{

  public static Vector2[] PlanePointsFromLine(Vector2[] points, float width) {

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
      outPoints[i * 3] = points[i] - perpVectors[i] * width;
      outPoints[i * 3 + 1] = points[i];
      outPoints[i * 3 + 2] = points[i] + perpVectors[i] * width;
    }

    return outPoints;
  }

  public static Mesh MeshFromPlane(Vector3[] points, float depth, float inset) {
    for (int i = 0; i < points.Length; i++) points[i] -= Vector3.up * inset;

    int pointCount = points.Length / 3;
    Vector3[] vertices = new Vector3[pointCount * 6];
    int[] triangles = new int[(pointCount - 1) * 36];

    for (int i = pointCount * 3; i < pointCount * 6; i++) vertices[i] += Vector3.up * depth;
    int halfVerts = pointCount * 3;
    for (int i = 0; i < pointCount - 1; i++) {
      triangles[i * 36] = i * 3 + 1;
      triangles[i * 36 + 1] = i * 3 + 3;
      triangles[i * 36 + 2] = i * 3;

      triangles[i * 36 + 3] = i * 3 + 1;
      triangles[i * 36 + 4] = i * 3 + 4;
      triangles[i * 36 + 5] = i * 3 + 3;

      triangles[i * 36 + 6] = i * 3 + 2;
      triangles[i * 36 + 7] = i * 3 + 4;
      triangles[i * 36 + 8] = i * 3 + 1;

      triangles[i * 36 + 9] = i * 3 + 2;
      triangles[i * 36 + 10] = i * 3 + 5;
      triangles[i * 36 + 11] = i * 3 + 4;

      triangles[i * 36 + 12] = i * 3;
      triangles[i * 36 + 13] = i * 3 + 3;
      triangles[i * 36 + 14] = i * 3 + halfVerts;

      triangles[i * 36 + 15] = i * 3 + 3;
      triangles[i * 36 + 16] = i * 3 + 3 + halfVerts;
      triangles[i * 36 + 17] = i * 3 + halfVerts;

      triangles[i * 36 + 18] = i * 3 + 5;
      triangles[i * 36 + 19] = i * 3 + 2;
      triangles[i * 36 + 20] = i * 3 + 2 + halfVerts;

      triangles[i * 36 + 21] = i * 3 + 5;
      triangles[i * 36 + 22] = i * 3 + 2 + halfVerts;
      triangles[i * 36 + 23] = i * 3 + 5 + halfVerts;

      triangles[i * 36 + 24] = i * 3 + halfVerts;
      triangles[i * 36 + 25] = i * 3 + 3 + halfVerts;
      triangles[i * 36 + 26] = i * 3 + 1 + halfVerts;
      
      triangles[i * 36 + 27] = i * 3 + 3 + halfVerts;
      triangles[i * 36 + 28] = i * 3 + 4 + halfVerts;
      triangles[i * 36 + 29] = i * 3 + 1 + halfVerts;

      triangles[i * 36 + 30] = i * 3 + 1 + halfVerts;
      triangles[i * 36 + 31] = i * 3 + 4 + halfVerts;
      triangles[i * 36 + 32] = i * 3 + 2 + halfVerts;

      triangles[i * 36 + 33] = i * 3 + 4 + halfVerts;
      triangles[i * 36 + 34] = i * 3 + 5 + halfVerts;
      triangles[i * 36 + 35] = i * 3 + 2 + halfVerts;
    }

    Mesh msh = new Mesh();
    msh.vertices = vertices;
    msh.triangles = triangles;
    msh.RecalculateNormals();

    return msh;
  }

}