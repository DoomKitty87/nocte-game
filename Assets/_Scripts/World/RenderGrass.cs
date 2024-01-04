using UnityEngine;

public class RenderGrass : MonoBehaviour
{
    #region Exposed Variables

    [HideInInspector] public int _numberOfChunks;
    [HideInInspector] public Vector3[][] _vertices;
    [HideInInspector] public int[][] _tris;
    
    [SerializeField] public bool _enableGrass;
    [SerializeField] public bool _regenerateGrass;
    
    [Header("Variables")]
    [SerializeField, Range(0.1f, 0.75f)] private float _minBladeHeight = 0.2f;
    [SerializeField, Range(0.1f, 0.75f)] private float _maxBladeHeight = 1.0f;
    [SerializeField, Range(0.0f, 0.5f)] private float _scale = 1.0f;
    [SerializeField, Min(1), Delayed] private int _numberOfBladesPerTri = 1;

    // NOTE: All of the following are automatically loaded from "Resources" file
    // Left as SerializeField for ease of access
    [Header("Resources")]
    [SerializeField, Tooltip("Automatically loaded")] private ComputeShader _computeShader;
    [SerializeField, Tooltip("Automatically loaded")] private Mesh _grassMesh;
    [SerializeField, Tooltip("Automatically loaded")] private Material _material;
    
    #endregion
    
    #region Local Variables
    
    private bool _resourcesLoaded;
    private bool _grassIsEnabled;
    
    private GraphicsBuffer[] _terrainTriangleBuffer;
    private GraphicsBuffer[] _terrainVertexBuffer;
    
    private GraphicsBuffer[] _transformMatrixBuffer;
    
    private GraphicsBuffer[] _grassTriangleBuffer;
    private GraphicsBuffer[] _grassVertexBuffer;
    private GraphicsBuffer[] _grassUVBuffer;
    
    private Bounds[] _bounds;
    private MaterialPropertyBlock[] _materialPropertyBlock;

    private RenderParams[] _rp;
    
    private int _kernel;
    private uint[] _threadGroupSize;
    private int[] _terrainTriangleCount;
    
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

        for (int i = 0; i < _numberOfChunks; i++) {
            // Get data from terrain mesh
            Vector3[] terrainVertices = _vertices[i];
            _terrainVertexBuffer[i] = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                terrainVertices.Length,
                sizeof(float) * 3
            );
            _terrainVertexBuffer[i].SetData(terrainVertices);

            int[] terrainTriangles = _tris[i];
            _terrainTriangleBuffer[i] = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                terrainTriangles.Length,
                sizeof(int)
            );
            _terrainTriangleBuffer[i].SetData(terrainTriangles);

            _terrainTriangleCount[i] = terrainTriangles.Length / 3;

            // Passes data to compute buffer
            _computeShader.SetBuffer(_kernel, TerrainPositions, _terrainVertexBuffer[i]);
            _computeShader.SetBuffer(_kernel, TerrainTriangles, _terrainTriangleBuffer[i]);

            // Data for RenderPrimitivesIndexed
            Vector3[] grassVertices = _grassMesh.vertices;
            _grassVertexBuffer[i] = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                grassVertices.Length,
                sizeof(float) * 3
            );
            _grassVertexBuffer[i].SetData(grassVertices);

            int[] grassTriangles = _grassMesh.triangles;
            _grassTriangleBuffer[i] = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                grassTriangles.Length,
                sizeof(int)
            );
            _grassTriangleBuffer[i].SetData(grassTriangles);

            Vector2[] grassUVs = _grassMesh.uv;
            _grassUVBuffer[i] = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                grassUVs.Length,
                sizeof(float) * 2
            );
            _grassUVBuffer[i].SetData(grassUVs);

            // Buffer for transformation matrices
            _transformMatrixBuffer[i] = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                _terrainTriangleCount[i] * _numberOfBladesPerTri,
                sizeof(float) * 16
            );
            _computeShader.SetBuffer(_kernel, TransformMatrices, _transformMatrixBuffer[i]);

            // Bounds setup
            _bounds[i] = _bounds[i];
            _bounds[i].center += transform.position;
            _bounds[i].Expand(_maxBladeHeight);

            // Creates MaterialPropertyBlock
            _materialPropertyBlock[i] = new MaterialPropertyBlock();
            _materialPropertyBlock[i].SetBuffer(TransformMatrices, _transformMatrixBuffer[i]);
            _materialPropertyBlock[i].SetBuffer(Positions, _grassVertexBuffer[i]);
            _materialPropertyBlock[i].SetBuffer(UVs, _grassUVBuffer[i]);

            // Binds RenderParams
            _rp[i] = new RenderParams(_material) {
                worldBounds = _bounds[i],
                matProps = _materialPropertyBlock[i]
            };

            RunComputeShader(i);
        }
    }

    private void RunComputeShader(int chunkIndex) {
        // Bind variables to compute shader.
        _computeShader.SetMatrix(TerrainObjectToWorld, transform.localToWorldMatrix);
        _computeShader.SetInt(TerrainTriangleCount, _terrainTriangleCount[chunkIndex]);
        _computeShader.SetFloat(MinBladeHeight, _minBladeHeight);
        _computeShader.SetFloat(MaxBladeHeight, _maxBladeHeight);
        _computeShader.SetFloat(Scale, _scale);
        _computeShader.SetFloat(NumberOfBladesPerTri, _numberOfBladesPerTri);
        
        // Run the compute shader.
        _computeShader.GetKernelThreadGroupSizes(_kernel, out _threadGroupSize[chunkIndex], out _, out _);
        int threadGroups = Mathf.CeilToInt(_terrainTriangleCount[chunkIndex] * _numberOfBladesPerTri / _threadGroupSize[chunkIndex]);
        _computeShader.Dispatch(_kernel, threadGroups, 1, 1);
    }
    
    private void CleanUpGrass() {
        for (int i = 0; i < _numberOfChunks; i++) {
            _terrainTriangleBuffer[i]?.Release();
            _terrainVertexBuffer[i]?.Release();
            _transformMatrixBuffer[i]?.Release();

            _grassTriangleBuffer[i]?.Release();
            _grassVertexBuffer[i]?.Release();
            _grassUVBuffer[i]?.Release();
        }
    }

    private void OnDestroy()
    {
        CleanUpGrass();
    }
    
    #endregion
    
    #region Render Grass
    
    private void Update() {
        
        if (!_enableGrass) return;
        
        if (_regenerateGrass) {
            CleanUpGrass();
            GenerateGrass();
            _regenerateGrass = false;
        }

        for (int i = 0; i < _numberOfChunks; i++) {
            Graphics.RenderPrimitivesIndexed(
                _rp[i],
                MeshTopology.Triangles,
                _grassTriangleBuffer[i],
                _grassTriangleBuffer[i].count,
                instanceCount: _terrainTriangleCount[i] * _numberOfBladesPerTri
            );
        }
    }
    
    #endregion
}
