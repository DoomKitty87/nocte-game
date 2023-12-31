using System;
using UnityEngine;
using UnityEngine.Assertions;

public class RenderGrass : MonoBehaviour
{
    #region Exposed Variables

    public bool _enableGrass = false;
    
    [Header("Set up")] 
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private Mesh _terrainMesh;
    
    [Header("Grass")]
    [Tooltip("Mesh to be rendered.")]
    [SerializeField] private Mesh _grassMesh;
    [Tooltip("Material to be applied to mesh. Must be custom material with correct properties.")]
    [SerializeField] private Material _material;

    public float _minBladeHeight = 0.2f;
    public float _maxBladeHeight = 1.0f;
    public float _minOffset = -5f;
    public float _maxOffset = 5f;
    public float _scale = 1.0f;
    [Min(1)] public int _numberOfBladesPerTri = 1;
    
    #endregion
    
    #region Local Variables

    private GraphicsBuffer _terrainTriangleBuffer;
    private GraphicsBuffer _terrainVertexBuffer;
    
    private GraphicsBuffer _transformMatrixBuffer;
    
    private GraphicsBuffer _grassTriangleBuffer;
    private GraphicsBuffer _grassVertexBuffer;
    private GraphicsBuffer _grassUVBuffer;
    
    private Bounds _bounds;
    private MaterialPropertyBlock _MPB;

    private int _kernel;
    private uint _threadGroupSize;
    private int _terrainTriangleCount = 0;
    
    #endregion
    
    #region Setup

    public void EnableGrass() {
        _terrainMesh = GetComponent<MeshFilter>().sharedMesh;
        _computeShader = (ComputeShader) Resources.Load("Grass/ProceduralGrassCompute");
        _material = (Material)Resources.Load("Grass/Custom_ProceduralGrass");
        _grassMesh = ((GameObject)Resources.Load("Grass/GrassBlade(Clone)")).GetComponentInChildren<MeshFilter>().
            sharedMesh;
        
        _kernel = _computeShader.FindKernel("CalculateBladePositions");

        // Gets vertices of terrain object.
        Vector3[] terrainVertices = _terrainMesh.vertices;
        _terrainVertexBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            terrainVertices.Length,
            sizeof(float) * 3
        );
        _terrainVertexBuffer.SetData(terrainVertices);
        
        // Gets tris of terrain object.
        int[] terrainTriangles = _terrainMesh.triangles;
        _terrainTriangleBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            terrainTriangles.Length,
            sizeof(int)
        );
        _terrainTriangleBuffer.SetData(terrainTriangles);

        _terrainTriangleCount = terrainTriangles.Length / 3;
        
        // Passes data to compute buffer.
        _computeShader.SetBuffer(_kernel, "_TerrainPositions", _terrainVertexBuffer);
        _computeShader.SetBuffer(_kernel, "_TerrainTriangles", _terrainTriangleBuffer);
        
        // Data for RenderPrimitivesIndexed.
        Vector3[] grassVertices = _grassMesh.vertices;
        _grassVertexBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, 
            grassVertices.Length, 
            sizeof(float) * 3
        );
        _grassVertexBuffer.SetData(grassVertices);

        int[] grassTriangles = _grassMesh.triangles;
        _grassTriangleBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            grassTriangles.Length,
            sizeof(int)
        );
        _grassTriangleBuffer.SetData(grassTriangles);

        Vector2[] grassUVs = _grassMesh.uv;
        _grassUVBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            grassUVs.Length,
            sizeof(float) * 2
        );
        _grassUVBuffer.SetData(grassUVs);
        
        // Set up buffers for transformation matrices.
        _transformMatrixBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            _terrainTriangleCount * _numberOfBladesPerTri,
            sizeof(float) * 16
        );
        _computeShader.SetBuffer(_kernel, "_TransformMatrices", _transformMatrixBuffer);

        // Set bounds.
        _bounds = _terrainMesh.bounds;
        _bounds.center += transform.position;
        _bounds.Expand(_maxBladeHeight);

        // Bind buffers to a MaterialPropertyBlock.
        _MPB = new MaterialPropertyBlock();
        _MPB.SetBuffer("_TransformMatricews", _transformMatrixBuffer);
        _MPB.SetBuffer("_Positions", _grassVertexBuffer);
        _MPB.SetBuffer("_UVs", _grassUVBuffer);
        
        RunComputeShader();
    }

    private void RunComputeShader() {
        // Bind variables to compute shader.
        _computeShader.SetMatrix("_TerrainObjectToWorld", transform.localToWorldMatrix);
        _computeShader.SetInt("_TerrainTriangleCount", _terrainTriangleCount);
        _computeShader.SetFloat("_MinBladeHeight", _minBladeHeight);
        _computeShader.SetFloat("_MaxBladeHeight", _maxBladeHeight);
        _computeShader.SetFloat("_MinOffset", _minOffset);
        _computeShader.SetFloat("_MaxOffset", _maxOffset);
        _computeShader.SetFloat("_Scale", _scale);
        _computeShader.SetFloat("_NumberOfBladesPerTri", _numberOfBladesPerTri);
        
        // Run the compute shader.
        _computeShader.GetKernelThreadGroupSizes(_kernel, out _threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt((_terrainTriangleCount * _numberOfBladesPerTri) / _threadGroupSize);
        _computeShader.Dispatch(_kernel, threadGroups, 1, 1);
    }
    
    void OnDestroy()
    {
        CleanUpGrass();
    }

    public void CleanUpGrass() {
        _terrainTriangleBuffer?.Release();
        _terrainVertexBuffer?.Release();
        _transformMatrixBuffer?.Release();

        _grassTriangleBuffer?.Release();
        _grassVertexBuffer?.Release();
        _grassUVBuffer?.Release();
    }
    #endregion

    private bool _grassIsEnabled;
    
    private void Update() {
        if (_enableGrass && !_grassIsEnabled) {
            EnableGrass();
            _grassIsEnabled = true;
        } else if (_enableGrass && _grassIsEnabled) {
            RenderParams rp = new RenderParams(_material);
            rp.worldBounds = _bounds;
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetBuffer("_TransformMatrices", _transformMatrixBuffer);
            rp.matProps.SetBuffer("_Positions", _grassVertexBuffer);
            rp.matProps.SetBuffer("_UVs", _grassUVBuffer);

            Graphics.RenderPrimitivesIndexed(
                rp,
                MeshTopology.Triangles,
                _grassTriangleBuffer,
                _grassTriangleBuffer.count,
                instanceCount: _terrainTriangleCount * _numberOfBladesPerTri
            );
        } else if (!_enableGrass && _grassIsEnabled) {
            CleanUpGrass();
            _grassIsEnabled = false;
        }
    }
}
