using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using Math = System.Math;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class WorldGenerator : MonoBehaviour
{

  #region Structs

  private struct WorldTile
  {
    public GameObject obj;
    public Mesh mesh;
    public MeshCollider meshCollider;
    public int x;
    public int z;
    public int waterVertIndex;
    public int waterVertCount;
    public int waterTriIndex;
    public int waterTriCount;
    public int currentLOD;

  }

  [System.Serializable]
  private struct LODLevel {
    public int distance;
    public int factor;
  }

  [System.Serializable]
  private struct AmalgamNoiseParams
  {

    [Tooltip("Iterations of noise layered for final result.")]
    public int octaves;
    [Tooltip("Scale change factor between octaves.")]
    public float lacunarity;        
    [Tooltip("Amplitude change factor between octaves.")]
    public float persistence;        
    [Tooltip("Scale of sharpness noise permutation.")]
    public float sharpnessScale;        
    [Tooltip("Amplitude of sharpness noise permutation.")]
    public float sharpnessAmplitude;        
    [Tooltip("Midpoint value of sharpness.")]
    public float sharpnessMean;        
    [Tooltip("Scale of scale noise permutation.")]
    public float scaleScale;        
    [Tooltip("Amplitude of scale noise permutation.")]
    public float scaleAmplitude;        
    [Tooltip("Midpoint value of scale.")]
    public float scaleMean;
    [Tooltip("Scale of amplitude noise permutation.")]
    public float amplitudeScale;        
    [Tooltip("Amplitude of amplitude noise permutation.")]
    public float amplitudeAmplitude;         
    [Tooltip("Midpoint value of amplitude.")]
    public float amplitudeMean;       
    [Tooltip("Scale of warp strength noise permutation.")]
    public float warpStrengthScale;         
    [Tooltip("Amplitude of warp strength noise permutation.")]
    public float warpStrengthAmplitude;       
    [Tooltip("Midpoint value of warp strength.")]
    public float warpStrengthMean;        
    [Tooltip("Scale of warp scale noise permutation.")]
    public float warpScaleScale;      
    [Tooltip("Amplitude of warp scale noise permutation.")]
    public float warpScaleAmplitude;
    [Tooltip("Midpoint value of warp scale.")]
    public float warpScaleMean;        

  }

  [System.Serializable]
  private struct RiverParams 
  {
    [Tooltip("Scale of river noise.")]
    public float scale;
    [Tooltip("Maximum depth of rivers.")]
    public float amplitude;
    [Tooltip("Octaves of river noise.")]
    public int octaves;
    [Tooltip("Scale factor between octaves of river noise.")]
    public float lacunarity;
    [Tooltip("Amplitude factor between octaves of river noise.")]
    public float persistence;
    [Tooltip("Warp scale of river noise.")]
    public float warpScale;
    [Tooltip("Warp strength of river noise.")]
    public float warpStrength;
    [Tooltip("Downwards offset of water level.")]
    public float waterLevel;
    public AnimationCurve noiseCurve;
    public AnimationCurve heightCurve;
    public AnimationCurve normalCurve;
    [Tooltip("Water rendering object.")]
    public GameObject obj;

  }

  #endregion
  
  #region Properties

  [Header("World Generation")]
  [Tooltip("Seed for world generation.")]
  public int _seed;
  [Tooltip("Size of world in tiles.")]
  [SerializeField] private int _tileCount;
  [Tooltip("Vertices of each tile in world (lowest LOD).")]
  [SerializeField] private int _size;
  [Tooltip("Vertex spacing of each tile in world (lowest LOD).")]
  [SerializeField] private float _resolution;
  [Tooltip("Maximum chunk updates per frame.")]
  [SerializeField] private float _maxUpdatesPerFrame;
  [Tooltip("Enable or disable colliders.")]
  [SerializeField] private bool _enableColliders;
  [Tooltip("Maximum distance from player with colliders.")]
  [SerializeField] private int _colliderRange;
  [Tooltip("Threshold to force colliders to update.")]
  [SerializeField] private int _forceBakeThreshold;
  [Tooltip("Material for world mesh.")]
  [SerializeField] private Material _material;
  [SerializeField] private AmalgamNoiseParams _noiseParameters;
  [Tooltip("Refresh world generation.")]

  [SerializeField] private bool _refreshButton = false;
  [Header("LOD Settings")]
  
  [SerializeField] private LODLevel[] _lodLevels;
  [Tooltip("LOD level outside of specified range.")]
  [SerializeField] private int _defaultLOD;

  [Header("Water Settings")]
  [Tooltip("Enable or disable rivers.")]
  [SerializeField] private bool _enableRivers;
  [SerializeField] private RiverParams _riverParameters;
  [Tooltip("Lake plane height.")]
  [SerializeField] private float _lakePlaneHeight;
  [Tooltip("Lake transform.")]
  [SerializeField] private Transform _lakeObject;

  [Header("Script Connections")]
  [SerializeField] private SecondaryStructures _structures;
  [SerializeField] private PlaceStructures _storyStructures;

  #endregion

  #region Internal Variables

  private WorldTile[] _tilePool;
  private int[,] _tilePositions;
  private List<Vector2> _generatedStructureTiles = new List<Vector2>();
  private List<int> _updateQueue = new List<int>();
  private List<int[]> _generateQueue = new List<int[]>();
  private List<int> _frameColliderBakeBuffer = new List<int>();
  private float updatesLeft;

  private float _playerX;
  private float _playerZ;
  private int _lastPlayerChunkX;
  private int _lastPlayerChunkZ;
  private int _playerXChunkScale;
  private int _playerZChunkScale;

  private float _maxPossibleHeight;
  private Mesh _waterMesh;
  private List<Vector3> _waterVertices = new List<Vector3>();
  private List<int> _waterTriangles = new List<int>();

  #endregion

  #region Public Fetch Functions

  public float GetHeightValue(Vector2 worldPosition) {
    float heightVal = AmalgamNoise.GenerateTerrain(0, 1, worldPosition.x + _seed, worldPosition.y + _seed, 0, 0, _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
      _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
      _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
      _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
      _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean)[0].y;
    heightVal -= _riverParameters.heightCurve.Evaluate(heightVal / _maxPossibleHeight) * (_riverParameters.noiseCurve.Evaluate(AmalgamNoise.GenerateRivers(
        0, 1, worldPosition.x + _seed % 216812, worldPosition.y + _seed % 216812, 0, 0, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength)[0]) * _riverParameters.amplitude);
    return heightVal;
  }

  public float GetRiverValue(Vector2 worldPosition) {
    return _riverParameters.heightCurve.Evaluate(worldPosition.y / _maxPossibleHeight) * _riverParameters.noiseCurve.Evaluate(AmalgamNoise.GenerateRivers(
      0, 1, worldPosition.x + _seed % 216812, worldPosition.y + _seed % 216812, 0, 0, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength)[0]);
  }

  public float GetSeedHash() {
    return _seed;
  }

  public (Vector3[][], Vector2[]) GetVertices(int distance) {
    int included = 0;
    for (int i = 0; i < _tileCount * _tileCount; i++) {
      if (Mathf.Sqrt(Mathf.Pow(_tilePool[i].x, 2) + Mathf.Pow(_tilePool[i].z, 2)) <= distance) included++;
    }
    Vector3[][] vertices = new Vector3[included][];
    Vector2[] positions = new Vector2[included];
    for (int i = 0, j = 0; i < _tileCount * _tileCount; i++) {
      if (Mathf.Sqrt(Mathf.Pow(_tilePool[i].x, 2) + Mathf.Pow(_tilePool[i].z, 2)) <= distance) {
        vertices[j] = _tilePool[i].mesh.vertices;
        positions[j] = new Vector2(_tilePool[i].obj.transform.position.x, _tilePool[i].obj.transform.position.z);
        j++;
      }
    }

    return (vertices, positions);
  }

  #endregion

  #region Unity Functions

  private void OnValidate() {
    if (_refreshButton) {
      _refreshButton = false;
      Regenerate();
    }
  }

  private void Awake() {
    _maxPossibleHeight = _noiseParameters.amplitudeMean + _noiseParameters.amplitudeAmplitude;
    _waterMesh = new Mesh();
    _waterMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    _riverParameters.obj.GetComponent<WaterSurface>().mesh = _waterMesh;
    _seed = int.Parse(Hash128.Compute(_seed).ToString().Substring(0, 6), System.Globalization.NumberStyles.HexNumber);
    _lakeObject.position = new Vector3(0, _lakePlaneHeight, 0);
  }

  private void Start()  {
    SetupPool();
  }

  private void Update() {
    if ((_frameColliderBakeBuffer.Count > 0 && _generateQueue.Count == 0) || _frameColliderBakeBuffer.Count > _forceBakeThreshold) {
      StartCoroutine(BakeColliders());
    }
    while (updatesLeft > 0) {
      if (_updateQueue.Count > 0 && _generateQueue.Count == 0) {
        UpdateTile(_updateQueue[0]);
        _updateQueue.RemoveAt(0);
        if (_updateQueue.Count == 0) UpdateWaterMesh();
        updatesLeft--;
      } else break;
    }
    for (int i = 0; i < _maxUpdatesPerFrame * 20; i++) {
      if (_generateQueue.Count > 0) {
        GenerateTile(_generateQueue[0][0], _generateQueue[0][1], _generateQueue[0][2]);
        _generateQueue.RemoveAt(0);
        if (Time.timeScale == 0f) Time.timeScale = 1f;
      } else break;
    }
    updatesLeft += _maxUpdatesPerFrame;
    updatesLeft = Mathf.Min(updatesLeft, Mathf.Max(_maxUpdatesPerFrame, 1));
  }

  #endregion

  #region Chunk Management Functions

  public void Regenerate() {
    for (int i = 0; i < _tilePool.Length; i++) {
      Destroy(_tilePool[i].obj);
    }
    SetupPool();
  }

  private void SetupPool() {
    Time.timeScale = 0f;
    _tilePool = new WorldTile[_tileCount * _tileCount];
    _tilePositions = new int[_tileCount, _tileCount];
    for (int x = -(_tileCount - 1) / 2, i = 0; x <= (_tileCount - 1) / 2; x++)
    {
      for (int z = -(_tileCount - 1) / 2; z <= (_tileCount - 1) / 2; z++) {
        QueueTileGen(x, z, i);
        i++;
      }
    }
    _generateQueue.Sort((c1, c2) => (Mathf.Abs(c1[0]) + Mathf.Abs(c1[1])).CompareTo(Mathf.Abs(c2[0]) + Mathf.Abs(c2[1])));
  }

  private void QueueTileUpdate(int index) {
    _updateQueue.Add(index);
  }

  private void QueueTileGen(int x, int z, int index) {
    _generateQueue.Add(new int[] {
      x, z, index
    }); 
  }

  public void UpdatePlayerLoadedChunks(Vector3 playerPos) {
    if (_generateQueue.Count > 0) return;
    int playerXChunkScale = Mathf.FloorToInt(playerPos.x / (_size * _resolution));
    int playerZChunkScale = Mathf.FloorToInt(playerPos.z / (_size * _resolution));

    _playerXChunkScale = playerXChunkScale;
    _playerZChunkScale = playerZChunkScale;

    if (playerXChunkScale - _lastPlayerChunkX > 1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX - 1;
    if (playerZChunkScale - _lastPlayerChunkZ > 1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ - 1;
    if (playerXChunkScale - _lastPlayerChunkX < -1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX + 1;
    if (playerZChunkScale - _lastPlayerChunkZ < -1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ + 1;

    int deltaX = playerXChunkScale - _lastPlayerChunkX;
    int deltaZ = playerZChunkScale - _lastPlayerChunkZ;
    
    if (deltaX < 0) {
      int[] tempValues = new int[_tileCount];
      for (int i = 0; i < _tileCount; i++) {
        tempValues[i] = _tilePositions[_tileCount - 1, i];
      }
      
      for (int x = _tileCount - 1; x > 0; x--) {
        for (int z = 0; z < _tileCount; z++) {
          _tilePositions[x, z] = _tilePositions[x - 1, z];
        }
      }
      
      for (int i = 0; i < _tileCount; i++) {
        _tilePositions[0, i] = tempValues[i];
      }
      
      for (int i = 0; i < _tileCount; i++) {
        _tilePool[_tilePositions[0, i]].x = playerXChunkScale - (_tileCount - 1) / 2;
        _tilePool[_tilePositions[0, i]].z = i + playerZChunkScale - (_tileCount - 1) / 2;
        QueueTileUpdate(_tilePositions[0, i]);
      }
    }
    else if (deltaX > 0) {
      int[] tempValues = new int[_tileCount];
      for (int i = 0; i < _tileCount; i++) {
        tempValues[i] = _tilePositions[0, i];
      }
      
      for (int x = 0; x < _tileCount - 1; x++) {
        for (int z = 0; z < _tileCount; z++) {
          _tilePositions[x, z] = _tilePositions[x + 1, z];
        }
      }
      
      for (int i = 0; i < _tileCount; i++) {
        _tilePositions[_tileCount - 1, i] = tempValues[i];
      }
      
      for (int i = 0; i < _tileCount; i++) {
        _tilePool[_tilePositions[_tileCount - 1, i]].x = playerXChunkScale + (_tileCount - 1) / 2;
        _tilePool[_tilePositions[_tileCount - 1, i]].z = i + playerZChunkScale - (_tileCount - 1) / 2;
        QueueTileUpdate(_tilePositions[_tileCount - 1, i]);
      }
    }

    if (deltaZ < 0) {
      int[] tempValues = new int[_tileCount];
      for (int i = 0; i < _tileCount; i++) {
        tempValues[i] = _tilePositions[i, _tileCount - 1];
      }
      
      for (int x = 0; x < _tileCount; x++) {
        for (int z = _tileCount - 1; z > 0; z--) {
          _tilePositions[x, z] = _tilePositions[x, z - 1];
        }
      }
      
      for (int i = 0; i < _tileCount; i++) {
        _tilePositions[i, 0] = tempValues[i];
      }
      
      for (int i = 0; i < _tileCount; i++) {
        _tilePool[_tilePositions[i, 0]].x = i + playerXChunkScale - (_tileCount - 1) / 2;
        _tilePool[_tilePositions[i, 0]].z = playerZChunkScale - (_tileCount - 1) / 2;
        QueueTileUpdate(_tilePositions[i, 0]);
      }
    }
    else if (deltaZ > 0) {
      int[] tempValues = new int[_tileCount];
      for (int i = 0; i < _tileCount; i++) {
        tempValues[i] = _tilePositions[i, 0];
      }
      
      for (int x = 0; x < _tileCount; x++) {
        for (int z = 0; z < _tileCount - 1; z++) {
          _tilePositions[x, z] = _tilePositions[x, z + 1];
        }
      }
      
      for (int i = 0; i < _tileCount; i++) {
        _tilePositions[i, _tileCount - 1] = tempValues[i];
      }
      for (int i = 0; i < _tileCount; i++) {
        _tilePool[_tilePositions[i, _tileCount - 1]].x = i + playerXChunkScale - (_tileCount - 1) / 2;
        _tilePool[_tilePositions[i, _tileCount - 1]].z = playerZChunkScale + (_tileCount - 1) / 2;
        QueueTileUpdate(_tilePositions[i, _tileCount - 1]);
      }
    }

    if (deltaZ != 0 || deltaX != 0) {
      _structures.CheckStructures(new Vector2(playerPos.x, playerPos.z));
      _storyStructures.CheckStructures(new Vector2(playerPos.x, playerPos.z));
      _updateQueue.Sort((c1, c2) => (Mathf.Abs(_tilePool[c1].x - _playerXChunkScale) + Mathf.Abs(_tilePool[c1].z - _playerZChunkScale)).CompareTo(Mathf.Abs(_tilePool[c2].x - _playerXChunkScale) + Mathf.Abs(_tilePool[c2].z - _playerZChunkScale)));
      for (int x = 0; x < _tileCount; x++) {
        for (int z = 0; z < _tileCount; z++) { 
          float maxDistance = Mathf.Max(Mathf.Abs(_tilePool[_tilePositions[x, z]].x - playerXChunkScale), Mathf.Abs(_tilePool[_tilePositions[x, z]].z - playerZChunkScale));
          if (GetLOD(new Vector2(_tilePool[_tilePositions[x, z]].x, _tilePool[_tilePositions[x, z]].z), new Vector2(playerXChunkScale, playerZChunkScale)) != _tilePool[_tilePositions[x, z]].currentLOD) {
            _tilePool[_tilePositions[x, z]].currentLOD = GetLOD(new Vector2(_tilePool[_tilePositions[x, z]].x, _tilePool[_tilePositions[x, z]].z), new Vector2(playerXChunkScale, playerZChunkScale));
            QueueTileUpdate(_tilePositions[x, z]);
            continue;
          }
          if (_enableColliders && maxDistance < _colliderRange) { 
            if (_tilePool[_tilePositions[x, z]].meshCollider == null || !_tilePool[_tilePositions[x, z]].meshCollider.enabled) UpdateCollider(_tilePositions[x, z]);
          }
        }
      }
    }
    
    _lastPlayerChunkX = playerXChunkScale;
    _lastPlayerChunkZ = playerZChunkScale;
  }

  #endregion

  #region Tile Generation Functions

  private int GetLOD(Vector2 playerChunkCoords, Vector2 chunkCoords) {
    int lod = _defaultLOD;
    
    for (int i = 0; i < _lodLevels.Length; i++) {
      if (Mathf.Max(Mathf.Abs(playerChunkCoords.x - chunkCoords.x), Mathf.Abs(playerChunkCoords.y - chunkCoords.y)) < _lodLevels[i].distance) {
      lod = _lodLevels[i].factor;
      break;
      }
    }

    return lod;
  }

  private void GenerateTile(int x, int z, int index) {
    GameObject go = new GameObject("Tile");
    go.transform.parent = transform;
    MeshFilter mf = go.AddComponent<MeshFilter>();
    MeshRenderer mr = go.AddComponent<MeshRenderer>();
    mr.material = _material;
    mf.mesh = new Mesh();
    Mesh msh = mf.mesh;
    WorldTile tile = new WorldTile();
    int lodFactor = GetLOD(new Vector2(_playerXChunkScale, _playerZChunkScale), new Vector2(x, z));
    tile.currentLOD = lodFactor;
    lodFactor = (int) Mathf.Pow(2, lodFactor);
    if (_size * lodFactor * _size * lodFactor > 65000) {
        msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }
    
    Vector3[] result = AmalgamNoise.GenerateTerrain(_size, lodFactor, x * _size * _resolution + _seed, z * _size * _resolution + _seed, _resolution / lodFactor, _resolution / lodFactor,
        _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
        _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
        _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
        _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
        _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean);
    msh.vertices = result;
    
    go.transform.position = new Vector3(x * _size * _resolution, 0, z * _size * _resolution);
    go.isStatic = true;
    
    // If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
    go.tag = "Ground";
    go.layer = 6;
    _structures.GenerateChunkStructures(new Vector2(x * _size * _resolution, z * _size * _resolution), new Vector2((x + 1) * _size * _resolution, (z + 1) * _size * _resolution));
    _generatedStructureTiles.Add(new Vector2(x, z));
    tile.mesh = msh;
    tile.obj = go;
    tile.x = x;
    tile.z = z;
    _tilePool[index] = tile;
    _tilePositions[index / _tileCount, index % _tileCount] = index;

    WindTriangles(msh, index);
    UpdateMesh(msh, index);
    float maxDistance = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
    if (_enableColliders && maxDistance <= _colliderRange) UpdateCollider(index);

    if (index == (_tileCount * _tileCount) - 1) UpdateWaterMesh();
  }

  private void UpdateTile(int index) {
    int x = _tilePool[index].x;
    int z = _tilePool[index].z;
    int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
    if (_size * lodFactor * _size * lodFactor > 65000) {
        _tilePool[index].mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }
    float maxDistance = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x - _playerXChunkScale), 2) + Mathf.Pow(Mathf.Abs(z - _playerZChunkScale), 2));
    Vector3[] result = AmalgamNoise.GenerateTerrain(_size, lodFactor, x * _size * _resolution + _seed, z * _size * _resolution + _seed, _resolution / lodFactor, _resolution / lodFactor,
        _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
        _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
        _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
        _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
        _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean);
    _tilePool[index].mesh.triangles = null;
    _tilePool[index].mesh.vertices = result;

    WindTriangles(_tilePool[index].mesh, index);

    _tilePool[index].obj.transform.position = new Vector3(x * _size * _resolution, 0, z * _size * _resolution);
    if (!_generatedStructureTiles.Contains(new Vector2(x, z))) {
        _structures.GenerateChunkStructures(new Vector2(x * _size * _resolution, z * _size * _resolution), new Vector2((x + 1) * _size * _resolution, (z + 1) * _size * _resolution));
        _generatedStructureTiles.Add(new Vector2(x, z));
    }
    UpdateMesh(_tilePool[index].mesh, index);
    maxDistance = Mathf.Max(Mathf.Abs(x - _playerXChunkScale), Mathf.Abs(z - _playerZChunkScale));
    if (_enableColliders && maxDistance <= _colliderRange) UpdateCollider(index);
  }

  #endregion

  #region Water Management Functions

  private void UpdateWaterMesh() {
    _waterMesh.triangles = null;
    _waterMesh.vertices = _waterVertices.ToArray();
    _waterMesh.triangles = _waterTriangles.ToArray();
  }

  private void RiverPass(Mesh targetMesh, int index) {
    if (_tilePool[index].waterVertCount > 0) {
      _waterVertices.RemoveRange(_tilePool[index].waterVertIndex, _tilePool[index].waterVertCount);
      _waterTriangles.RemoveRange(_tilePool[index].waterTriIndex, _tilePool[index].waterTriCount);
      for (int i = 0; i < _waterTriangles.Count; i++) {
        if (_waterTriangles[i] >= _tilePool[index].waterVertIndex) _waterTriangles[i] -= _tilePool[index].waterVertCount;
      }
      for (int i = 0; i < _tileCount * _tileCount; i++) {
        if (_tilePool[i].waterVertIndex > _tilePool[index].waterVertIndex) {
          _tilePool[i].waterVertIndex -= _tilePool[index].waterVertCount;
        }
        if (_tilePool[i].waterTriIndex > _tilePool[index].waterTriIndex) {
          _tilePool[i].waterTriIndex -= _tilePool[index].waterTriCount;
        }
      }
    }
    _tilePool[index].waterVertCount = 0;
    float maxDistance = Mathf.Max(Mathf.Abs(_tilePool[index].x - _playerXChunkScale), Mathf.Abs(_tilePool[index].z - _playerZChunkScale));
    Vector3[] vertices = targetMesh.vertices;
    Vector3[] normals = targetMesh.normals;
    int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
    float[] heightMods = AmalgamNoise.GenerateRivers(_size, lodFactor, _tilePool[index].x * _size * _resolution + _seed % 216812,
        _tilePool[index].z * _size * _resolution + _seed % 216812, _resolution / lodFactor, _resolution / lodFactor, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength);
    Vector3[] waterVerts = new Vector3[vertices.Length];
    bool[] ignoreVerts = new bool[vertices.Length];
    int ignored = 0;
    for (int i = 0; i < heightMods.Length; i++) {
      heightMods[i] = _riverParameters.noiseCurve.Evaluate(heightMods[i]) * _riverParameters.heightCurve.Evaluate(vertices[i].y / _maxPossibleHeight) * _riverParameters.normalCurve.Evaluate(Mathf.Abs(normals[i].y));
      waterVerts[i] = vertices[i] - new Vector3(0, _riverParameters.waterLevel, 0);
      if (heightMods[i] == 0) {
        waterVerts[i] -= new Vector3(0, _riverParameters.amplitude / 10, 0);
        ignoreVerts[i] = true;
        ignored++;
        continue;
      }
      vertices[i] -= new Vector3(0, heightMods[i] * _riverParameters.amplitude, 0);
      waterVerts[i] += _tilePool[index].obj.transform.position;
    }

    int sideLength = (int) Mathf.Sqrt(vertices.Length);
    int[] triangles = new int[(sideLength - 1) * (sideLength - 1) * 6];
    for (int i = 0, j = 0; i < waterVerts.Length; i++) {
      if (i / sideLength >= sideLength - 1) continue;
      if (i % sideLength >= sideLength - 1) continue;
      if (i / sideLength == 0) continue;
      if (i % sideLength == 0) continue;
      triangles[j * 6] = i;
      triangles[j * 6 + 1] = i + sideLength;
      triangles[j * 6 + 2] = i + 1;
      triangles[j * 6 + 3] = i + sideLength;
      triangles[j * 6 + 4] = i + sideLength + 1;
      triangles[j * 6 + 5] = i + 1;
      j++;
    }
    targetMesh.vertices = vertices;
    int waterVertsLength = _waterVertices.Count;
    int waterTriLength = _waterTriangles.Count;
    int[] realIndices = new int[waterVerts.Length];
    Vector3[] verts = new Vector3[waterVerts.Length - ignored];
    for (int i = 0, j = 0; i < waterVerts.Length; i++) {
      if (!ignoreVerts[i]) {
        verts[j] = waterVerts[i];
        realIndices[i] = j;
        j++;
      }
    }
    int triCount = 0;
    for (int i = 0; i < triangles.Length; i+= 3) {
      if (!ignoreVerts[triangles[i]] && !ignoreVerts[triangles[i + 1]] && !ignoreVerts[triangles[i + 2]]) {
        triCount += 3;
      }
    }
    int[] tris = new int[triCount];
    for (int i = 0, j = 0; i < triangles.Length; i+= 3) {
      if (!ignoreVerts[triangles[i]] && !ignoreVerts[triangles[i + 1]] && !ignoreVerts[triangles[i + 2]]) {
        tris[j] = realIndices[triangles[i]] + waterVertsLength;
        tris[j + 1] = realIndices[triangles[i + 1]] + waterVertsLength;
        tris[j + 2] = realIndices[triangles[i + 2]] + waterVertsLength;
        j += 3;
      }
    }
    _waterVertices.AddRange(verts);
    _waterTriangles.AddRange(tris);
    _tilePool[index].waterVertIndex = waterVertsLength;
    _tilePool[index].waterVertCount = waterVerts.Length - ignored;
    _tilePool[index].waterTriIndex = waterTriLength;
    _tilePool[index].waterTriCount = _waterTriangles.Count - waterTriLength;
  }

  #endregion

  #region Mesh Operation Functions

  private void WindTriangles(Mesh targetMesh, int index) {
    int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
    int sideLength = (int) Mathf.Sqrt(targetMesh.vertices.Length) - 1;
    int[] triangles = new int[sideLength * sideLength * 6];
    int vert = 0;
    int tris = 0;
    for (int z = 0; z < sideLength; z++)
    {
      for (int x = 0; x < sideLength; x++)
      {
        triangles[tris] = vert;
        triangles[tris + 1] = vert + sideLength + 1;
        triangles[tris + 2] = vert + 1;
        triangles[tris + 3] = vert + 1;
        triangles[tris + 4] = vert + sideLength + 1;
        triangles[tris + 5] = vert + sideLength + 2;
        vert++;
        tris += 6;
      }
      vert++;
    }

    targetMesh.triangles = triangles;
  }

  private int[] CullTriangles(Mesh targetMesh, int index) {
    int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
    int[] triangles = targetMesh.triangles;
    int sideLength = (int) Mathf.Sqrt(targetMesh.vertices.Length) - 1;
    int[] culled = new int[(sideLength * sideLength - (sideLength * 2 + (sideLength - 2) * 2)) * 6];
    for (int i = 0, j = 0; i < sideLength * sideLength; i++) {
      if (i / sideLength == sideLength - 1) continue;
      if (i % sideLength == sideLength - 1) continue;
      if (i / sideLength == 0) continue;
      if (i % sideLength == 0) continue;
      culled[j] = triangles[i * 6];
      culled[j + 1] = triangles[i * 6 + 1];
      culled[j + 2] = triangles[i * 6 + 2];
      culled[j + 3] = triangles[i * 6 + 3];
      culled[j + 4] = triangles[i * 6 + 4];
      culled[j + 5] = triangles[i * 6 + 5];
      j += 6;
    }

    return culled;
  }

  private struct BakeColliderJob : IJobParallelFor
  {

    public NativeArray<int> meshIds;

    public void Execute(int index)
    {

      Physics.BakeMesh(meshIds[index], false);

    }

  }

  private (JobHandle, NativeArray<int>) ExecuteBake(int[] indices) {
    Mesh[] meshes = new Mesh[indices.Length];
    for (int i = 0; i < indices.Length; i++) {
      meshes[i] = _tilePool[indices[i]].mesh;
    }

    NativeArray<int> meshIds = new NativeArray<int>(meshes.Length, Allocator.TempJob);
    for (int i = 0; i < meshes.Length; i++) {
      meshIds[i] = meshes[i].GetInstanceID();
    }

    BakeColliderJob job = new BakeColliderJob {
      meshIds = meshIds
    };
    
    JobHandle handle = job.Schedule(meshIds.Length, 1);
    return (handle, meshIds);
  }

  private System.Collections.IEnumerator BakeColliders() {
    int[] indices = _frameColliderBakeBuffer.ToArray();
    (JobHandle, NativeArray<int>) jobData = ExecuteBake(indices);
    while (!jobData.Item1.IsCompleted) {
      yield return null;
    }
    jobData.Item1.Complete();
    jobData.Item2.Dispose();
    for (int i = 0; i < indices.Length; i++) {
      if (!_tilePool[indices[i]].meshCollider) _tilePool[indices[i]].meshCollider = _tilePool[indices[i]].obj.AddComponent<MeshCollider>();
      _tilePool[indices[i]].meshCollider.enabled = true;
      _tilePool[indices[i]].meshCollider.sharedMesh = _tilePool[indices[i]].mesh;
    }
    _frameColliderBakeBuffer.Clear();
  }

  private void UpdateCollider(int index) {
    _frameColliderBakeBuffer.Add(index);
  }

  private void UpdateMesh(Mesh targetMesh, int index) {
    targetMesh.normals = CalculateNormalsJobs(targetMesh);
    //RiverPass(targetMesh, index);
    targetMesh.triangles = CullTriangles(targetMesh, index);
    targetMesh.RecalculateBounds();
  }

  [BurstCompile]
  private struct NormalJob : IJobParallelFor
  {

    [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<int> triangles;
    [NativeDisableParallelForRestriction] public NativeArray<float3> normals;

    public void Execute(int index) {
      int vertexIndexA = triangles[index * 3];
      int vertexIndexB = triangles[index * 3 + 1];
      int vertexIndexC = triangles[index * 3 + 2];
      float3 pointA = vertices[vertexIndexA];
      float3 normal = math.cross(vertices[vertexIndexB] - pointA, vertices[vertexIndexC] - pointA);
      normals[vertexIndexA] += normal;
      normals[vertexIndexB] += normal;
      normals[vertexIndexC] += normal;
    }

  }

  private Vector3[] CalculateNormalsJobs(Mesh targetMesh) {
    Vector3[] verts = targetMesh.vertices;
    int[] tris = targetMesh.triangles;
    NativeArray<float3> vertices = new NativeArray<float3>(verts.Length, Allocator.TempJob);
    NativeArray<int> triangles = new NativeArray<int>(tris.Length, Allocator.TempJob);
    NativeArray<float3> normals = new NativeArray<float3>(verts.Length, Allocator.TempJob);
    for (int i = 0; i < verts.Length; i++) vertices[i] = verts[i];
    for (int i = 0; i < tris.Length; i++) triangles[i] = tris[i];
    NormalJob job = new NormalJob {
      vertices = vertices,
      triangles = triangles,
      normals = normals
    };
    JobHandle handle = job.Schedule(tris.Length / 3, 64);
    handle.Complete();
    Vector3[] normalOut = new Vector3[verts.Length];
    normalOut = normals.Reinterpret<Vector3>().ToArray();
    for (int i = 0; i < normalOut.Length; i++) normalOut[i].Normalize();
    vertices.Dispose();
    triangles.Dispose();
    normals.Dispose();
    return normalOut;
  }

  #endregion

}