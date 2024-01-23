using UnityEngine;

public class RenderSingleGrass : MonoBehaviour
{
    #region Exposed Variables

    [SerializeField] private bool _enableGrass;
    [SerializeField] bool _regenerateGrass;
    
    [Header("Variables")]
    [SerializeField, Range(0.1f, 0.75f)] private float _minBladeHeight = 0.2f;
    [SerializeField, Range(0.1f, 0.75f)] private float _maxBladeHeight = 1.0f;
    [SerializeField, Range(0.0f, 0.5f)] private float _scale = 1.0f;
    [SerializeField, Min(1), Delayed] private int _numberOfBladesPerTri = 1;

    // NOTE: All of the following are automatically loaded from "Resources" file
    // Left as SerializeField for ease of access
    [Header("Resources")]
    [SerializeField, Tooltip("Automatically loaded")] private ComputeShader _computeShader;
    [SerializeField, Tooltip("Automatically loaded")] private Mesh _terrainMesh;
    [SerializeField, Tooltip("Automatically loaded")] private Mesh _grassMesh;
    [SerializeField, Tooltip("Automatically loaded")] private Material _material;
    
    #endregion
    
    #region Local Variables

    private bool _resourcesLoaded;
    private bool _grassIsEnabled;
    
    private GraphicsBuffer _terrainTriangleBuffer;
    private GraphicsBuffer _terrainVertexBuffer;
    
    private GraphicsBuffer _transformMatrixBuffer;
    
    private GraphicsBuffer _grassTriangleBuffer;
    private GraphicsBuffer _grassVertexBuffer;
    private GraphicsBuffer _grassUVBuffer;
    
    private Bounds _bounds;
    private MaterialPropertyBlock _materialPropertyBlock;

    private int _kernel;
    private uint _threadGroupSize;
    private int _terrainTriangleCount;
    
    private RenderParams _rp;
    
    // Cached property index
    private static readonly int TerrainPositions = Shader.PropertyToID("_TerrainPositions");
    private static readonly int TerrainTriangles = Shader.PropertyToID("_TerrainTriangles");
    private static readonly int TransformMatrices = Shader.PropertyToID("_TransformMatrices");
    private static readonly int Positions = Shader.PropertyToID("_Positions");
    private static readonly int UVs = Shader.PropertyToID("_UVs");
    private static readonly int TerrainObjectToWorld = Shader.PropertyToID("_TerrainObjectToWorld");
    
    private static readonly int TerrainTriangleCount = Shader.PropertyToID("_TerrainTriangleCount");
    private static readonly int MinBladeHeight = Shader.PropertyToID("_MinBladeHeight");
    private static readonly int MaxBladeHeight = Shader.PropertyToID("_MaxBladeHeight");
    private static readonly int Scale = Shader.PropertyToID("_Scale");
    private static readonly int NumberOfBladesPerTri = Shader.PropertyToID("_NumberOfBladesPerTri");

    #endregion
    
    #region Initialize Grass

    private void LoadResources() {
        // Load from resources
        _terrainMesh = GetComponent<MeshFilter>().sharedMesh;
        _computeShader = (ComputeShader) Resources.Load("Grass/ProceduralGrassCompute");
        _material = (Material)Resources.Load("Grass/Custom_ProceduralGrass");
        _grassMesh = ((GameObject)Resources.Load("Grass/GrassBlade")).GetComponentInChildren<MeshFilter>().
            sharedMesh; // Is there a better way to load meshes?
        
        _kernel = _computeShader.FindKernel("CalculateBladePositions");
    }
    
    private void GenerateGrass() {

        if (!_resourcesLoaded) {
            LoadResources();
            _resourcesLoaded = true;
        }

        // Get data from terrain mesh
        Vector3[] terrainVertices = _terrainMesh.vertices;
        _terrainVertexBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            terrainVertices.Length,
            sizeof(float) * 3
        );
        _terrainVertexBuffer.SetData(terrainVertices);
        
        int[] terrainTriangles = _terrainMesh.triangles;
        _terrainTriangleBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            terrainTriangles.Length,
            sizeof(int)
        );
        _terrainTriangleBuffer.SetData(terrainTriangles);

        _terrainTriangleCount = terrainTriangles.Length / 3;
        
        // Passes data to compute buffer
        _computeShader.SetBuffer(_kernel, TerrainPositions, _terrainVertexBuffer);
        _computeShader.SetBuffer(_kernel, TerrainTriangles, _terrainTriangleBuffer);
        
        // Data for RenderPrimitivesIndexed
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
        
        // Buffer for transformation matrices
        _transformMatrixBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            _terrainTriangleCount * _numberOfBladesPerTri,
            sizeof(float) * 16
        );
        _computeShader.SetBuffer(_kernel, TransformMatrices, _transformMatrixBuffer);

        // Bounds setup
        _bounds = _terrainMesh.bounds;
        _bounds.center += transform.position;
        _bounds.Expand(_maxBladeHeight);

        // Creates MaterialPropertyBlock
        _materialPropertyBlock = new MaterialPropertyBlock();
        _materialPropertyBlock.SetBuffer(TransformMatrices, _transformMatrixBuffer);
        _materialPropertyBlock.SetBuffer(Positions, _grassVertexBuffer);
        _materialPropertyBlock.SetBuffer(UVs, _grassUVBuffer);
        
        // Binds RenderParams
        _rp = new RenderParams(_material) {
            worldBounds = _bounds,
            matProps = _materialPropertyBlock
        };
        
        RunComputeShader();
    }

    private void RunComputeShader() {
        // Bind variables to compute shader.
        _computeShader.SetMatrix(TerrainObjectToWorld, transform.localToWorldMatrix);
        _computeShader.SetInt(TerrainTriangleCount, _terrainTriangleCount);
        _computeShader.SetFloat(MinBladeHeight, _minBladeHeight);
        _computeShader.SetFloat(MaxBladeHeight, _maxBladeHeight);
        _computeShader.SetFloat(Scale, _scale);
        _computeShader.SetFloat(NumberOfBladesPerTri, _numberOfBladesPerTri);
        
        // Run the compute shader.
        _computeShader.GetKernelThreadGroupSizes(_kernel, out _threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt(_terrainTriangleCount * _numberOfBladesPerTri / _threadGroupSize);
        _computeShader.Dispatch(_kernel, threadGroups, 1, 1);
        
        Debug.Log(_terrainTriangleCount);
    }
    
    private void CleanUpGrass() {
        _terrainTriangleBuffer?.Release();
        _terrainVertexBuffer?.Release();
        _transformMatrixBuffer?.Release();

        _grassTriangleBuffer?.Release();
        _grassVertexBuffer?.Release();
        _grassUVBuffer?.Release();
    }

    private void OnDestroy()
    {
        CleanUpGrass();
    }
    
    #endregion
    
    #region Render Grass
    
    private void Update() {
        
        if (_regenerateGrass) {
            CleanUpGrass();
            GenerateGrass();
            _regenerateGrass = false;
        }
        
        if (!_enableGrass) return;
        
        // Is this slow?
        try {
            Graphics.RenderPrimitivesIndexed(
                _rp,
                MeshTopology.Triangles,
                _grassTriangleBuffer,
                _grassTriangleBuffer.count, 
                instanceCount: _terrainTriangleCount * _numberOfBladesPerTri
            );
        }
        catch {
            Debug.Log("Not Rendering Grass This Frame Due To GPU Instance Error, regenerating grass");
            GenerateGrass();
        }
    }
    
    #endregion
}