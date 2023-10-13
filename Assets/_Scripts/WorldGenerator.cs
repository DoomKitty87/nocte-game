using System;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using UnityEngine.ProBuilder;
using Math = System.Math;

public class WorldGenerator : MonoBehaviour
{
    private struct WorldTile
    {
        public GameObject obj;
        public Mesh mesh;
        public MeshCollider meshCollider;
        public float[] temperatureMap;
        public float[] humidityMap;
        public int x;
        public int z;
        public int grassIndexStart;
        public int grassCount;
        public int[] biomeData;

    }
    
    [System.Serializable]
    public struct NoiseLayer
    {

        public string name;
        public string noiseType;
        public float amplitude;
        public int octaves;
        public AnimationCurve easeCurve;
        public AnimationCurve primaryEase;
        public float scaleX;
        public float scaleZ;
        public bool turbulent;

    }

    [System.Serializable]
    public struct ScatterLayer
    {

        public string name;
        public GameObject prefab;

    }

    [System.Serializable]
    public struct ScatterSettings
    {
        
        public float density;
        
    }

    [System.Serializable]
    public struct Biome
    {

        public string name;
        public NoiseLayer[] noiseLayers;
        public ScatterLayer[] scatterLayers;

    }
    
    [System.Serializable]
    public struct GrassLOD
    {
        
        public Mesh mesh;
        public int distance;
        public int density;

    }
    
    private struct GrassData
    {

        public Vector4 position;
        public Vector2 uv;
        public float displacement;

    }
    
    public int _xSize = 64;
    public int _zSize = 64;

    public float _xResolution = 0.25f;
    public float _zResolution = 0.25f;
    
    public int _xTiles = 101;
    public int _zTiles = 101;
    public float _scale = 1000;
    public float _biomeScale = 0.0001f;
    
    public float _amplitude = 50;
    public int _octaves = 10;

    public float _temperatureScale = 2;
    public float _humidityScale = 1.5f;

    public int _seed;
    public int _colliderRange;
    
    public Material _material;
    public Gradient _colorGradient;
    public bool _useColorGradient;
    public AnimationCurve _easeCurve;
    
    public int _maxUpdatesPerFrame = 5;

    public bool _hasColliders;
    
    public Material _material2;
    public Mesh _mesh;

    public int _maxGrassDistChunks = 10;
    public int _grassDensity = 5;

    public AnimationCurve _rockPassCurve;
    public AnimationCurve _rockPassNoiseCurve;
    public float _rockPassAmplitude = 1;
    public float _rockPassScale = 2;

    public float _cavePassScale = 100;
    public float _cavePassAmplitude = 10;
    public AnimationCurve _cavePassCurve;

    [SerializeField] private GrassLOD[] _grassLODLevels;
    [SerializeField] private Biome[] _biomes;
    [SerializeField] private ScatterSettings[] _scatterSettings;

    public int _scatterRange = 2;
    // These are separated because density is used in calculations before biome is calculated

    private WorldTile[] _tilePool;
    private int[,] _tilePositions;

    private int _lastPlayerChunkX;
    private int _lastPlayerChunkZ;
    private int _playerChunkXWorld;
    private int _playerChunkZWorld;
    
    private List<int> _updateQueue = new List<int>();
    private List<int[]> _generateQueue = new List<int[]>();

    private Texture2D _wind;
    
    GraphicsBuffer[] _commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[][] _commandData;
    private ComputeBuffer[] _positionsBuffer;
    const int _commandCount = 2;
    private List<GrassData>[] _grassData;
    private RenderParams[] _rp;

    private int[] _grassLODLookupDensityArray;
    private int[] _grassLODLookupBufferArray;
    private int[] _grassLODChunkCache;
    
    private float _playerX, _playerZ;
    private int _playerXChunkScale, _playerZChunkScale;
    
    private Vector2 _windPos;

    private void Awake() {
        MakeGrassBuffers();
        FillGrassArray();
    }

    private float HashVector3ToFloat(Vector3 inputVector, int otherSeed)
    {
        int seed = _seed;

        float psuedoRandomValue = ((Mathf.Sin((float)(inputVector.x * 234.24 * (_seed * otherSeed / 1000))) +
                                   Mathf.Tan((float)(inputVector.y * 937.24 * (_seed * otherSeed / 950)) -
                                             Mathf.Cos((float)(inputVector.z * 734.52 * (_seed * otherSeed / 1050))))) * 100000) % 1;
        
        return psuedoRandomValue;
    }

    private void MakeGrassBuffers() {
        int numberOfBuffers = _grassLODLevels.Length;
        _grassData = new List<GrassData>[numberOfBuffers];
        _positionsBuffer = new ComputeBuffer[numberOfBuffers];
        _rp = new RenderParams[numberOfBuffers];
        _commandBuf = new GraphicsBuffer[numberOfBuffers];
        _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[numberOfBuffers][];
    }
    
    private void FillGrassArray() {
        int numberOfChunks = _xTiles * _zTiles;
        _grassLODChunkCache = new int[numberOfChunks];
        for (int i = 0; i < _grassLODChunkCache.Length; i++) {
            _grassLODChunkCache[i] = -1;
        }
        
        int numberOfElements = _grassLODLevels[^1].distance;
        _grassLODLookupDensityArray = new int[numberOfElements];
        _grassLODLookupBufferArray = new int[numberOfElements];
        for (int i = _grassLODLevels.Length - 1; i >= 0; i--) {
            for (int j = 0; j < _grassLODLevels[i].distance; j++) {
                _grassLODLookupDensityArray[j] = _grassLODLevels[i].density;
                _grassLODLookupBufferArray[j] = i;
            }
        }
    }
    
    public void UpdatePlayerLoadedChunks(Vector3 playerPos) {
        if (_generateQueue.Count > 0) return;
        _material.SetVector("_PlayerPosition", playerPos);
        int playerXChunkScale = Mathf.FloorToInt(playerPos.x / (_xSize * _xResolution));
        int playerZChunkScale = Mathf.FloorToInt(playerPos.z / (_zSize * _zResolution));

        _playerChunkXWorld = playerXChunkScale;
        _playerChunkZWorld = playerZChunkScale;

        if (playerXChunkScale - _lastPlayerChunkX > 1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX - 1;
        if (playerZChunkScale - _lastPlayerChunkZ > 1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ - 1;
        if (playerXChunkScale - _lastPlayerChunkX < -1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX + 1;
        if (playerZChunkScale - _lastPlayerChunkZ < -1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ + 1;

        int deltaX = playerXChunkScale - _lastPlayerChunkX;
        int deltaZ = playerZChunkScale - _lastPlayerChunkZ;
        
        if (deltaX < 0) {
            int[] tempValues = new int[_zTiles];
            for (int i = 0; i < _zTiles; i++) {
                tempValues[i] = _tilePositions[_xTiles - 1, i];
            }
            
            for (int x = _xTiles - 1; x > 0; x--) {
                for (int z = 0; z < _zTiles; z++) {
                    _tilePositions[x, z] = _tilePositions[x - 1, z];
                }
            }
            
            for (int i = 0; i < _zTiles; i++) {
                _tilePositions[0, i] = tempValues[i];
            }
            
            for (int i = 0; i < _zTiles; i++) {
                _tilePool[_tilePositions[0, i]].x = playerXChunkScale - (_xTiles - 1) / 2;
                _tilePool[_tilePositions[0, i]].z = i + playerZChunkScale - (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[0, i]);
            }
        }
        else if (deltaX > 0) {
            int[] tempValues = new int[_zTiles];
            for (int i = 0; i < _zTiles; i++) {
                tempValues[i] = _tilePositions[0, i];
            }
            
            for (int x = 0; x < _xTiles - 1; x++) {
                for (int z = 0; z < _zTiles; z++) {
                    _tilePositions[x, z] = _tilePositions[x + 1, z];
                }
            }
            
            for (int i = 0; i < _zTiles; i++) {
                _tilePositions[_xTiles - 1, i] = tempValues[i];
            }
            
            for (int i = 0; i < _zTiles; i++) {
                _tilePool[_tilePositions[_xTiles - 1, i]].x = playerXChunkScale + (_xTiles - 1) / 2;
                _tilePool[_tilePositions[_xTiles - 1, i]].z = i + playerZChunkScale - (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[_xTiles - 1, i]);
            }
        }

        if (deltaZ < 0) {
            int[] tempValues = new int[_xTiles];
            for (int i = 0; i < _xTiles; i++) {
                tempValues[i] = _tilePositions[i, _zTiles - 1];
            }
            
            for (int x = 0; x < _xTiles; x++) {
                for (int z = _zTiles - 1; z > 0; z--) {
                    _tilePositions[x, z] = _tilePositions[x, z - 1];
                }
            }
            
            for (int i = 0; i < _xTiles; i++) {
                _tilePositions[i, 0] = tempValues[i];
            }
            
            for (int i = 0; i < _xTiles; i++) {
                _tilePool[_tilePositions[i, 0]].x = i + playerXChunkScale - (_xTiles - 1) / 2;
                _tilePool[_tilePositions[i, 0]].z = playerZChunkScale - (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[i, 0]);
            }
        }
        else if (deltaZ > 0) {
            int[] tempValues = new int[_xTiles];
            for (int i = 0; i < _xTiles; i++) {
                tempValues[i] = _tilePositions[i, 0];
            }
            
            for (int x = 0; x < _xTiles; x++) {
                for (int z = 0; z < _zTiles - 1; z++) {
                    _tilePositions[x, z] = _tilePositions[x, z + 1];
                }
            }
            
            for (int i = 0; i < _xTiles; i++) {
                _tilePositions[i, _zTiles - 1] = tempValues[i];
            }
            for (int i = 0; i < _xTiles; i++) {
                _tilePool[_tilePositions[i, _zTiles - 1]].x = i + playerXChunkScale - (_xTiles - 1) / 2;
                _tilePool[_tilePositions[i, _zTiles - 1]].z = playerZChunkScale + (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[i, _zTiles - 1]);
            }
        }

        if (deltaZ != 0 || deltaX != 0) {
            for (int x = 0; x < _xTiles; x++) {
                for (int z = 0; z < _zTiles; z++) { 
                    float maxDistance = Mathf.Max(Mathf.Abs(_tilePool[_tilePositions[x, z]].x - playerXChunkScale), Mathf.Abs(_tilePool[_tilePositions[x, z]].z - playerZChunkScale));
                    if (_hasColliders && maxDistance < _colliderRange) { 
                        UpdateCollider(_tilePositions[x, z]);
                    } else if (_hasColliders) {
                        if (_tilePool[_tilePositions[x, z]].meshCollider) _tilePool[_tilePositions[x, z]].meshCollider.enabled = false;
                    }

                    GenerateGrassBasedOffLODs(x, z, Mathf.FloorToInt(maxDistance));
                }
            }
        }
        
        _lastPlayerChunkX = playerXChunkScale;
        _lastPlayerChunkZ = playerZChunkScale;
        for (int i = 0; i < _rp.Length; i++) {
            _rp[i].matProps.SetVector("_PlayerPosition", playerPos);
        }
    }
    
    private void GenerateGrass(int index, int density, int bufferID) {
        List<GrassData> buffer = _grassData[bufferID];
        if (_tilePool[index].grassCount > 0) return;
        Vector3[] vertexData = _tilePool[index].mesh.vertices;
		int[] triangles = _tilePool[index].mesh.triangles;
		Vector3[] normals = new Vector3[triangles.Length / 3];

		triangles[^1] = 0; // Fixes bug with last triangle vertex being incorrect

		// Calculate normals of triangles
		for (int i = 0; i < normals.Length; i++) {
			normals[i] = Vector3.Cross(vertexData[triangles[i * 3 + 1]] - vertexData[triangles[i * 3]], vertexData[triangles[i * 3 + 2]] - vertexData[triangles[i * 3]]).normalized;
		}

		_tilePool[index].grassIndexStart = buffer.Count;
        
        // TODO:
        // Increase performance by converting to compute shader
        for (int i = 0; i < triangles.Length / 3; i++) {
            if (normals[i].y < 0.7f) continue;
            for (int j = 0; j < density; j++) {
                double r1 = UnityEngine.Random.Range(0f, 1f); // HashVector3ToFloat(vertexData[triangles[i]], j);
                double r2 = UnityEngine.Random.Range(0f, 1f); // HashVector3ToFloat(vertexData[triangles[i]], j + 1);
                GrassData gd = new GrassData();
                // Randomly pick points between the vertices of the triangle
                Vector3 randomPosition = 
                    (float)(1 - Math.Sqrt(r1)) * vertexData[triangles[i * 3]] 
                    + (float)(Math.Sqrt(r1) * (1 - r2)) * vertexData[triangles[i * 3 + 1]]
                    + (float)(r2 * Math.Sqrt(r1)) * vertexData[triangles[i * 3 + 2]];
                gd.position = new Vector4(
					randomPosition.x + (_tilePool[index].x * _xSize * _xResolution),
				    randomPosition.y, 
                    randomPosition.z + (_tilePool[index].z * _zSize * _zResolution), 
                    0);
                gd.uv = new Vector2(gd.position.x / (_xSize * _xResolution * _xTiles), gd.position.z / (_zSize * _zResolution * _zTiles));

                buffer.Add(gd);
            }
        }
        
        _tilePool[index].grassCount = buffer.Count - _tilePool[index].grassIndexStart;
    }
    
    private void GenerateGrassBasedOffLODs(int x, int z, int maxDistance) {
        
        int currentChunkSetting;
        if (maxDistance >= _grassLODLevels[^1].distance) currentChunkSetting = -1;
        else currentChunkSetting = _grassLODLookupBufferArray[maxDistance];
        
        if (currentChunkSetting != _grassLODChunkCache[_tilePositions[x, z]]) {
           if (_grassLODChunkCache[_tilePositions[x, z]] != -1) {
                 _grassData[0].RemoveRange(
                    _tilePool[_tilePositions[x, z]].grassIndexStart,
                    _tilePool[_tilePositions[x, z]].grassCount);
                for (int j = 0; j < _xTiles * _zTiles; j++) {
                    if (_tilePool[j].grassIndexStart > _tilePool[_tilePositions[x, z]].grassIndexStart)
                        _tilePool[j].grassIndexStart -= _tilePool[_tilePositions[x, z]].grassCount;
                }

               _tilePool[_tilePositions[x, z]].grassCount = 0;
               _tilePool[_tilePositions[x, z]].grassIndexStart = 0;
           }

           if (currentChunkSetting != -1) {
                GenerateGrass(_tilePositions[x, z], _grassLODLookupDensityArray[maxDistance], 0);
            }

            _grassLODChunkCache[_tilePositions[x, z]] = currentChunkSetting;
        }
    }
    
    private void Start() {
        _seed = int.Parse(Hash128.Compute(_seed).ToString().Substring(0, 6), System.Globalization.NumberStyles.HexNumber);
        _scale = 1 / _scale;
        
        SetupGrass();
        SetupPool();
    }

    private void Update() {
        for (int i = 0; i < _maxUpdatesPerFrame; i++) {
            if (_updateQueue.Count > 0 && _generateQueue.Count == 0) {
                UpdateTile(_updateQueue[0]);
                _updateQueue.RemoveAt(0);
            } else break;
        }
        for (int i = 0; i < _maxUpdatesPerFrame * 10; i++) {
            if (_generateQueue.Count > 0) {
                GenerateTile(_generateQueue[0][0], _generateQueue[0][1], _generateQueue[0][2]);
                _generateQueue.RemoveAt(0);
                Time.timeScale = 1f;
            } else break;
        }
        UpdateWind();
        for (int i = 0; i < _positionsBuffer.Length; i++) {
            Graphics.RenderMeshIndirect(_rp[i], _grassLODLevels[i].mesh, _commandBuf[i], _commandCount);
        }
    }
    
    private void UpdateWind() {
        _windPos += new Vector2(Time.deltaTime, Time.deltaTime);
        float[] windData = NoiseMaps.GenerateWindMap(128, 128, _windPos.x, _windPos.y, 0.1f);
        for (int i = 0; i < windData.Length; i++) {
            _wind.SetPixel(i % 128, i / 128, new Color(windData[i], windData[i], windData[i], 1));
        }
        _wind.Apply();
    }
    
    private void OnDestroy() {
        for (int i = 0; i < _positionsBuffer.Length; i++) {
            _commandBuf[i]?.Release();
            _positionsBuffer[i]?.Release();
            _positionsBuffer[i] = null;

        }
        _commandBuf = null;
        _positionsBuffer = null;
    }
    
    private void SetupPool() {
        Time.timeScale = 0f;
        _tilePool = new WorldTile[_xTiles * _zTiles];
        _tilePositions = new int[_xTiles, _zTiles];
        for (int x = -(_xTiles - 1) / 2, i = 0; x <= (_xTiles - 1) / 2; x++)
        {
            for (int z = -(_zTiles - 1) / 2; z <= (_zTiles - 1) / 2; z++) {
                QueueTileGen(x, z, i);
                i++;
            }
        }
    }

    private void SetupGrass() {
        for (int i = 0; i < _grassData.Length; i++) {
            _commandBuf[i] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, _commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            _commandData[i] = new GraphicsBuffer.IndirectDrawIndexedArgs[_commandCount];
            _grassData[i] = new List<GrassData>();
            _positionsBuffer[i] = new ComputeBuffer((_xSize + 2) * (_zSize + 2) * _xTiles * _zTiles, sizeof(float) * 7, ComputeBufferType.Default);
            _rp[i] = new RenderParams(_material2);
            _rp[i].matProps = new MaterialPropertyBlock();
            _wind = new Texture2D(128, 128);
            _rp[i].matProps.SetTexture("_Wind", _wind);
            _rp[i].matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(0, 0, 0)));
            _rp[i].matProps.SetBuffer("_PositionsBuffer", _positionsBuffer[i]);
        }
    }

    private void GenerateTile(int x, int z, int index)
    {
        GameObject go = new GameObject("Tile");
        go.transform.parent = transform;
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = _material;
        mf.mesh = new Mesh();
        Mesh msh = mf.mesh;
        int seed = _seed;
        var result = NoiseMaps.GenerateTerrainBiomes(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _biomes, _biomeScale, _xResolution, _zResolution);
        Vector3[] vertexData = result.Item1;
        msh.vertices = vertexData;
        WindTriangles(msh);
        
        float[] temperatureMap = NoiseMaps.GenerateTemperatureMap(vertexData, x * _xSize * _xResolution + (seed * 2), z * _zSize * _zResolution + (seed * 2), _xSize, _zSize, _scale / _temperatureScale, _easeCurve, _xResolution, _zResolution);
        float[] humidityMap = NoiseMaps.GenerateHumidityMap(vertexData, temperatureMap, x * _xSize * _xResolution + (seed * 0.5f), z * _zSize * _zResolution + (seed * 0.5f), _xSize, _zSize, _scale / _humidityScale, _easeCurve, _xResolution, _zResolution);
        go.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
        go.isStatic = true;
        
        // If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
        go.tag = "Ground";
        
        WorldTile tile = new WorldTile();
        
        tile.obj = go;
        tile.mesh = msh;
        tile.temperatureMap = temperatureMap;
        tile.humidityMap = humidityMap;
        tile.x = x;
        tile.z = z;
        tile.biomeData = result.Item2;
        _tilePool[index] = tile;
        UpdateMesh(msh, index);
        if (_hasColliders && Math.Abs(x) < _colliderRange && Math.Abs(z) < _colliderRange) {
            tile.meshCollider = go.AddComponent<MeshCollider>();
            tile.meshCollider.sharedMesh = msh;
        }
        //if (!_useColorGradient) CalculateUVs(msh);
        //else CalculateColors(index);
        _tilePositions[index / _zTiles, index % _zTiles] = index;
        if (index == (_xTiles * _zTiles) - 1) {
            UpdateGrassBuffers();
        }

        if (Math.Abs(x) < _scatterRange && Math.Abs(z) < _scatterRange) {
            ScatterTile(index);
        }
    }

    private void QueueTileUpdate(int index) {
        _updateQueue.Add(index);
        _updateQueue.Sort((c1, c2) => (Mathf.Abs(_tilePool[c1].x - _playerChunkXWorld) + Mathf.Abs(_tilePool[c1].z - _playerChunkZWorld)).CompareTo(Mathf.Abs(_tilePool[c2].x - _playerChunkXWorld) + Mathf.Abs(_tilePool[c2].z - _playerChunkZWorld)));
    }

    private void QueueTileGen(int x, int z, int index) {
        _generateQueue.Add(new int[] {
            x, z, index
        }); 
        _generateQueue.Sort((c1, c2) => (Mathf.Abs(c1[0]) + Mathf.Abs(c1[1])).CompareTo(Mathf.Abs(c2[0]) + Mathf.Abs(c2[1])));
    }
    
    //Regenerate given tile based on an LOD parameter.
    private void UpdateTile(int index) {
        int x = _tilePool[index].x;
        int z = _tilePool[index].z;
        _tilePool[index].mesh.Clear();
        int seed = _seed;
        var result = NoiseMaps.GenerateTerrainBiomes(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _biomes, _biomeScale, _xResolution, _zResolution);
        _tilePool[index].mesh.vertices = result.Item1;
        _tilePool[index].biomeData = result.Item2;
        _tilePool[index].temperatureMap = NoiseMaps.GenerateTemperatureMap(_tilePool[index].mesh.vertices, x * _xSize * _xResolution + (seed * 2), z * _zSize * _zResolution + (seed * 2), _xSize, _zSize, _scale / _temperatureScale, _easeCurve, _xResolution, _zResolution);
        _tilePool[index].humidityMap = NoiseMaps.GenerateHumidityMap(_tilePool[index].mesh.vertices, _tilePool[index].temperatureMap, x * _xSize * _xResolution + (seed * 0.5f), z * _zSize * _zResolution + (seed * 0.5f), _xSize, _zSize, _scale / _humidityScale, _easeCurve, _xResolution, _zResolution);

        WindTriangles(_tilePool[index].mesh);
        UpdateMesh(_tilePool[index].mesh, index);
        //if (!_useColorGradient) CalculateUVs(_tilePool[index].mesh);
        //else CalculateColors(index);
        _tilePool[index].obj.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
        int maxDistance = Mathf.Max(Mathf.Abs(x - _playerXChunkScale), Mathf.Abs(z - _playerZChunkScale));

        if (maxDistance < 2) UpdateCollider(index);
        else if (_tilePool[index].meshCollider) _tilePool[index].obj.GetComponent<MeshCollider>().enabled = false;

        if (maxDistance < _scatterRange) {
            ScatterTile(index);
        }
        if (_updateQueue.Count > 1) return;
        UpdateGrassBuffers();
    }


    
    private void UpdateGrassBuffers() {
        for (int i = 0; i < _grassData.Length; i++) {
            _rp[i].worldBounds = new Bounds(new Vector3(_playerX, 0, _playerZ),
                _xTiles * _xSize * _xResolution * Vector3.one); // use tighter bounds for better FOV culling
            _commandData[i][0].indexCountPerInstance = _mesh.GetIndexCount(0);
            _commandData[i][0].instanceCount = (uint)_grassData[i].Count;
            _commandData[i][1].indexCountPerInstance = _mesh.GetIndexCount(0);
            _commandData[i][1].instanceCount = (uint)_grassData[i].Count;
            _commandBuf[i].SetData(_commandData[i]);
            _positionsBuffer[i].SetData(_grassData[i]);
        }
    }

    private void WindTriangles(Mesh targetMesh) {
        int[] triangles = new int[(_xSize + 2) * (_zSize + 2) * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < _zSize + 2; z++)
        {
            for (int x = 0; x < _xSize + 2; x++)
            {
                triangles[tris] = vert + 0;
                triangles[tris + 1] = vert + _xSize + 3;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + _xSize + 3;
                triangles[tris + 5] = vert + _xSize + 4;
                vert++;
                tris += 6;
            }
            vert++;
        }

        targetMesh.triangles = triangles;
    }

    private void CalculateUVs(Mesh targetMesh) {
        int xSize = _xSize + 3;
        int zSize = _zSize + 3;

        Vector2[] uvs = new Vector2[xSize * zSize];

        Vector3[] vertices = targetMesh.vertices;
        for (int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x / (xSize * _xResolution), vertices[i].z / (zSize * _zResolution));
        }

        targetMesh.uv = uvs;
    }

    private void CalculateColors(int index) {
        Color[] colors = new Color[_tilePool[index].temperatureMap.Length];
        for (int i = 0; i < _tilePool[index].temperatureMap.Length; i++) {
            colors[i] = _colorGradient.Evaluate(_tilePool[index].temperatureMap[i]);
        }

        _tilePool[index].mesh.colors = colors;
    }

    private Vector3[] CalculateNormals(Mesh targetMesh, int index) {
        Vector3[] vertices = targetMesh.vertices;
        int[] triangles = targetMesh.triangles;
        Vector3[] normals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        int sideLength = (int) Mathf.Sqrt(vertices.Length);

        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 pointA = vertices[vertexIndexA];
            Vector3 pointB = vertices[vertexIndexB];
            Vector3 pointC = vertices[vertexIndexC];
            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            Vector3 normal = Vector3.Cross(sideAB, sideAC).normalized;
            normals[vertexIndexA] += normal;
            normals[vertexIndexB] += normal;
            normals[vertexIndexC] += normal;
        }
        

        for (int i = 0; i < normals.Length; i++) {
            normals[i].Normalize();
        }

        return normals;
    }

    private int[] CullTriangles(Mesh targetMesh) {
        int[] triangles = targetMesh.triangles;
        int sideLength = (int) Mathf.Sqrt(triangles.Length / 6) - 2;
        int[] culled = new int[(int) Mathf.Pow(sideLength, 2) * 6];

        for (int i = 0, j = 0; i < triangles.Length; i += 6) {
            int triangleIndex = i / 6;
            if (triangleIndex / (sideLength + 2) == sideLength + 1) continue;
            if (triangleIndex % (sideLength + 2) == sideLength + 1) continue;
            if (triangleIndex < sideLength + 2) continue;
            if (triangleIndex % (sideLength + 2) == 0) continue;
            culled[j] = triangles[i];
            culled[j + 1] = triangles[i + 1];
            culled[j + 2] = triangles[i + 2];
            culled[j + 3] = triangles[i + 3];
            culled[j + 4] = triangles[i + 4];
            culled[j + 5] = triangles[i + 5];
            j += 6;
        }

        return culled;
    }

    private void UpdateCollider(int index) {
        if (!_tilePool[index].meshCollider) _tilePool[index].meshCollider = _tilePool[index].obj.AddComponent<MeshCollider>();
        else if (_tilePool[index].meshCollider.enabled) return;
        _tilePool[index].meshCollider.enabled = true;
        _tilePool[index].meshCollider.sharedMesh = _tilePool[index].mesh;
    }

    private Vector3 CalculateAbsVertex(Vector3 vertex, int index) {
        Vector3 vertexAbs = new Vector3(vertex.x * _xResolution + _tilePool[index].x * _xSize * _xResolution, vertex.y, vertex.z * _zResolution + _tilePool[index].z * _zSize * _zResolution);
        return vertexAbs;
    }

    private void ScatterTile(int index) {
        for (int i = _tilePool[index].obj.transform.childCount - 1; i >= 0; i++) Destroy(_tilePool[index].obj.transform.GetChild(i).gameObject);
        for (int i = 0; i < _scatterSettings.Length; i++) {
            ScatterObjects(index, i);
        }
    }
    
    private void ScatterObjects(int index, int layer) {
        Mesh targetMesh = _tilePool[index].mesh;
        Vector3[] vertices = targetMesh.vertices;
        List<Vector2> points = PoissonDisk.GeneratePoints(1 / _scatterSettings[layer].density, new Vector2(_xSize, _zSize));
        for (int i = 0; i < points.Count; i++) {
            Vector3 vertex = vertices[(int)points[i].x + (int)points[i].y * _xSize];
            GameObject go = Instantiate(_biomes[_tilePool[index].biomeData[(int)points[i].x + (int)points[i].y * _xSize]].scatterLayers[layer].prefab, new Vector3(vertex.x + (_tilePool[index].x * _xSize * _xResolution), vertex.y, vertex.z + (_tilePool[index].z * _zSize * _zResolution)), UnityEngine.Quaternion.identity);
            go.transform.parent = _tilePool[index].obj.transform;
        }
    }

    private void RockPass(Mesh targetMesh, int index) {
      Vector3[] vertices = targetMesh.vertices;
      Vector3[] normals = targetMesh.normals;

      float[] rockVal = NoiseMaps.GenerateRockNoise((int) Mathf.Sqrt(vertices.Length), _tilePool[index].x * _xSize * _xResolution + _seed, _tilePool[index].z * _zSize * _zResolution + _seed, _rockPassScale, _rockPassScale, 1, _xResolution, _zResolution, true);
      //This doesn't work with caves right now. Just need to make it per vertex noise instead of grid generated.
      for (int i = 0; i < vertices.Length; i++) {
        vertices[i] += _rockPassAmplitude * _rockPassCurve.Evaluate(Mathf.Abs(normals[i].y)) *_rockPassNoiseCurve.Evaluate(rockVal[i]) * normals[i];
      }

      targetMesh.vertices = vertices;
    }

    private int[,] SubdivideFaces(Mesh targetMesh, int[] toSubdivide) {
        Vector3[] verticesOriginal = targetMesh.vertices;
        int[] trianglesOriginal = targetMesh.triangles;

        int originalVertLength = verticesOriginal.Length;
        int originalTriangleLength = trianglesOriginal.Length;

        Vector3[] vertices = new Vector3[verticesOriginal.Length + toSubdivide.Length];
        int[] triangles = new int[trianglesOriginal.Length + 6 * toSubdivide.Length];

        for (int i = 0; i < trianglesOriginal.Length; i++) {
            triangles[i] = trianglesOriginal[i];
        }

        for (int i = 0; i < verticesOriginal.Length; i++) {
            vertices[i] = verticesOriginal[i];
        }

        int[,] newVertices = new int[toSubdivide.Length,4];

        for (int i = 0; i < toSubdivide.Length; i++) {
            // An entry in the array is a triangle in triangles, so needs to be fetched with triangles[toSubdivide[i] * 3]
            int pointAIndx = triangles[toSubdivide[i]];
            int pointBIndx = triangles[toSubdivide[i] + 1];
            int pointCIndx = triangles[toSubdivide[i] + 2];
            Vector3 pointA = vertices[pointAIndx];
            Vector3 pointB = vertices[pointBIndx];
            Vector3 pointC = vertices[pointCIndx];
           
            Vector3 newVertex = (pointA + pointB + pointC) / 3;

            vertices[originalVertLength + i] = newVertex;
            newVertices[i,0] = originalVertLength + i;
            newVertices[i,1] = pointAIndx;
            newVertices[i,2] = pointBIndx;
            newVertices[i,3] = pointCIndx;
            triangles[toSubdivide[i] ] = pointAIndx;
            triangles[toSubdivide[i] + 1] = pointBIndx;
            triangles[toSubdivide[i] + 2] = originalVertLength + i;
            triangles[originalTriangleLength + i * 6] = pointBIndx;
            triangles[originalTriangleLength + i * 6 + 1] = pointCIndx;
            triangles[originalTriangleLength + i * 6 + 2] = originalVertLength + i;
            triangles[originalTriangleLength + i * 6 + 3] = pointCIndx;
            triangles[originalTriangleLength + i * 6 + 4] = pointAIndx;
            triangles[originalTriangleLength + i * 6 + 5] = originalVertLength + i;
        }

        targetMesh.vertices = vertices;
        targetMesh.triangles = triangles;
        return newVertices;
    }

    private void CavePass(Mesh targetMesh, int index) {
        //Decide which faces to subdivide
        int[] triangles = targetMesh.triangles;
        Vector3[] triangleSamples = new Vector3[triangles.Length / 3];
        int[] sampledTris = new int[triangles.Length / 3];
        Vector3[] faceNormals = new Vector3[sampledTris.Length];
        Vector3[] vertices = targetMesh.vertices;
        for (int i = 0, j = 0; i < triangles.Length / 6; i++) {
            Vector3 pointA = vertices[triangles[i * 6]];
            Vector3 pointB = vertices[triangles[i * 6 + 1]];
            Vector3 pointC = vertices[triangles[i * 6 + 2]];
            triangleSamples[j] = (pointA + pointB + pointC) / 3;
            faceNormals[j] = Vector3.Cross(pointB - pointA, pointC - pointA);
            sampledTris[j] = i * 6;
            j++;
            pointA = vertices[triangles[i * 6 + 3]];
            pointB = vertices[triangles[i * 6 + 4]];
            pointC = vertices[triangles[i * 6 + 5]];
            triangleSamples[j] = (pointA + pointB + pointC) / 3;
            faceNormals[j] = Vector3.Cross(pointB - pointA, pointC - pointA).normalized;
            sampledTris[j] = i * 6 + 3;
            j++;
        }
        float[] caveMap = NoiseMaps.GenerateCavePass(triangleSamples, _tilePool[index].x * _xResolution * _xSize + _seed, _tilePool[index].z * _zResolution * _zSize + _seed, _cavePassScale);

        List<int> toSubdivide = new List<int>();
        for (int i = 0; i < caveMap.Length; i++) {
            if (caveMap[i] > 0.5f) {
                toSubdivide.Add(sampledTris[i]);
            }
        }
        int[,] subdVerts = SubdivideFaces(targetMesh, toSubdivide.ToArray());
        vertices = targetMesh.vertices;
        Vector3[] normalsOriginal = targetMesh.normals;
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normalsOriginal.Length; i++) normals[i] = normalsOriginal[i];
        Vector3[] effectValues = new Vector3[vertices.Length];
        int[] normalizations = new int[vertices.Length];
        for (int i = 0; i < subdVerts.GetLength(0); i++) {
            if (vertices[subdVerts[i,0]].x <= _xResolution * 2 || vertices[subdVerts[i,0]].x >= _xResolution * (_xSize - 1) || vertices[subdVerts[i,0]].z <= _zResolution * 2 || vertices[subdVerts[i,0]].z >= _zResolution * (_zSize - 1)) continue;
            //if (faceNormals[i].y > 0.6f) continue;
            Vector3 subValue = new Vector3(0, _cavePassCurve.Evaluate(caveMap[toSubdivide[i] / 3]) * _cavePassAmplitude, 0);
            effectValues[subdVerts[i,0]] += subValue;
            effectValues[subdVerts[i,1]] += subValue;
            effectValues[subdVerts[i,2]] += subValue;
            effectValues[subdVerts[i,3]] += subValue;
            normalizations[subdVerts[i,1]]++;
            normalizations[subdVerts[i,2]]++;
            normalizations[subdVerts[i,3]]++;
            normals[subdVerts[i,0]] = faceNormals[i];
            normals[subdVerts[i,1]] += faceNormals[i];
            normals[subdVerts[i,2]] += faceNormals[i];
            normals[subdVerts[i,3]] += faceNormals[i];
        }
        for (int i = 0; i < vertices.Length; i++) {
            if (normalizations[i] > 0) {
                effectValues[i] /= normalizations[i];
                normals[i].Normalize();
            }                
            vertices[i] -= effectValues[i];
        }
        /* This did not work
        for (int i = 0; i < vertices.Length; i++) {
            List<int> newTriangles = new List<int>();
            int triangleCount = 0;
            for (int j = 0; j < triangles.Length; j++) {
              if (triangles[j] == i) triangleCount++;
            }
            //Debug.Log(triangleCount);
            if (triangleCount == 6) {
              for (int j = 0; j < triangles.Length / 3; j++) {
                if (triangles[j * 3] != i && triangles[j * 3 + 1] != i && triangles[j * 3 + 2] != i) {
                  newTriangles.Add(triangles[j * 3]);
                  newTriangles.Add(triangles[j * 3 + 1]);
                  newTriangles.Add(triangles[j * 3 + 2]);
                }
              }
            }
            else newTriangles.AddRange(triangles);
            triangles = newTriangles.ToArray();
        } */
        targetMesh.vertices = vertices;
        targetMesh.normals = normals;
        targetMesh.RecalculateNormals();
    }

    private void UpdateMesh(Mesh targetMesh, int index) {
        targetMesh.normals = CalculateNormals(targetMesh, index);
        targetMesh.triangles = CullTriangles(targetMesh);
        RockPass(targetMesh, index);
        // CavePass(targetMesh, index); Cave pass not the best really
        targetMesh.RecalculateBounds();
    }

}