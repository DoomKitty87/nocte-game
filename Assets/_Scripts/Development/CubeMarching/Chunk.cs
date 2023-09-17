using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
  public NoiseGenerator NoiseGenerator;
  public ComputeShader MarchingShader;
  public MeshFilter MeshFilter;
  
  float[] _weights;
  private Triangle[] triangles = new Triangle[0];
  private Vector3[] verts = new Vector3[0];
  private int[] tris = new int[0];
  
  struct Triangle {
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public static int SizeOf => sizeof(float) * 3 * 3;
  }
  
  ComputeBuffer _trianglesBuffer;
  ComputeBuffer _trianglesCountBuffer;
  ComputeBuffer _weightsBuffer;
  
  private void Awake() {
    CreateBuffers();
  }
  void Start() {
    _weights = NoiseGenerator.GetNoise();

    ConstructMesh();
  }

  private void Update() {
    _weights = NoiseGenerator.GetNoise();
    _weightsBuffer.SetData(_weights);
    _trianglesBuffer.SetCounterValue(0);

    MarchingShader.Dispatch(0, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads);
    if (ReadTriangleCount() != triangles.Length) Array.Resize(ref triangles, ReadTriangleCount());
    _trianglesBuffer.GetData(triangles);
    CreateMeshFromTriangles();
  }

  private void OnDestroy() {
    ReleaseBuffers();
  }

  void CreateBuffers() {
    _trianglesBuffer = new ComputeBuffer(5 * (GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk), Triangle.SizeOf, ComputeBufferType.Append);
    _trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    _weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk, sizeof(float));
  }

  void ReleaseBuffers() {
    _trianglesBuffer.Release();
    _trianglesCountBuffer.Release();
    _weightsBuffer.Release();
  }
  
  void ConstructMesh() {
    MarchingShader.SetBuffer(0, "_Triangles", _trianglesBuffer);
    MarchingShader.SetBuffer(0, "_Weights", _weightsBuffer);

    MarchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
    MarchingShader.SetFloat("_IsoLevel", .5f);

    _weightsBuffer.SetData(_weights);
    _trianglesBuffer.SetCounterValue(0);

    MarchingShader.Dispatch(0, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads);
    triangles = new Triangle[ReadTriangleCount()];
    _trianglesBuffer.GetData(triangles);
    MeshFilter.mesh = new Mesh();
    CreateMeshFromTriangles();
  }
  
  void CreateMeshFromTriangles() {
    if (triangles.Length * 3 != verts.Length) Array.Resize(ref verts, triangles.Length * 3);
    if (triangles.Length * 3 != tris.Length) Array.Resize(ref tris, triangles.Length * 3);
    for (int i = 0; i < triangles.Length; i++) {
      int startIndex = i * 3; 
      verts[startIndex] = triangles[i].a;
      verts[startIndex + 1] = triangles[i].b;
      verts[startIndex + 2] = triangles[i].c; 
      tris[startIndex] = startIndex;
      tris[startIndex + 1] = startIndex + 1;
      tris[startIndex + 2] = startIndex + 2;
    }

    MeshFilter.sharedMesh.triangles = new int[0];
    MeshFilter.sharedMesh.vertices = verts;
    MeshFilter.sharedMesh.triangles = tris;
    MeshFilter.sharedMesh.RecalculateNormals();
  }
  
  int ReadTriangleCount() {
    int[] triCount = { 0 };
    ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
    _trianglesCountBuffer.GetData(triCount);
    return triCount[0];
  }
    
}