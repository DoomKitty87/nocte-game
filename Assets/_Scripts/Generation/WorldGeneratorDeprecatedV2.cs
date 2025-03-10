// using System;
// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.Rendering.HighDefinition;
// using Matrix4x4 = UnityEngine.Matrix4x4;
// using Vector2 = UnityEngine.Vector2;
// using Vector3 = UnityEngine.Vector3;
// using Vector4 = UnityEngine.Vector4;
// using Math = System.Math;
// using Unity.Mathematics;
// using Unity.Jobs;
// using Unity.Burst;
// using Unity.Collections;
// using UnityEngine.Rendering;
// using IEnumerator = System.Collections.IEnumerator;
// using static AmalgamNoise;

// public class WorldGenerator : MonoBehaviour
// {

//   #region Structs

//   private struct WorldTile
//   {
//     public GameObject obj;
//     public Mesh mesh;
//     public MeshCollider meshCollider;
//     public int x;
//     public int z;
//     public Vector3[] waterVertices;
//     public int[] waterTriangles;
//     public int currentLOD;

//   }

//   [System.Serializable]
//   private struct LODLevel {
//     public int distance;
//     public int factor;
//   }

//   [System.Serializable]
//   private struct AmalgamNoiseParams
//   {

//     [Tooltip("Iterations of noise layered for final result.")]
//     public int octaves;
//     [Tooltip("Scale change factor between octaves.")]
//     public float lacunarity;        
//     [Tooltip("Amplitude change factor between octaves.")]
//     public float persistence;        
//     [Tooltip("Scale of sharpness noise permutation.")]
//     public float sharpnessScale;        
//     [Tooltip("Amplitude of sharpness noise permutation.")]
//     public float sharpnessAmplitude;        
//     [Tooltip("Midpoint value of sharpness.")]
//     public float sharpnessMean;        
//     [Tooltip("Scale of scale noise permutation.")]
//     public float scaleScale;        
//     [Tooltip("Amplitude of scale noise permutation.")]
//     public float scaleAmplitude;        
//     [Tooltip("Midpoint value of scale.")]
//     public float scaleMean;
//     [Tooltip("Scale of amplitude noise permutation.")]
//     public float amplitudeScale;        
//     [Tooltip("Amplitude of amplitude noise permutation.")]
//     public float amplitudeAmplitude;         
//     [Tooltip("Midpoint value of amplitude.")]
//     public float amplitudeMean;       
//     [Tooltip("Scale of warp strength noise permutation.")]
//     public float warpStrengthScale;         
//     [Tooltip("Amplitude of warp strength noise permutation.")]
//     public float warpStrengthAmplitude;       
//     [Tooltip("Midpoint value of warp strength.")]
//     public float warpStrengthMean;        
//     [Tooltip("Scale of warp scale noise permutation.")]
//     public float warpScaleScale;      
//     [Tooltip("Amplitude of warp scale noise permutation.")]
//     public float warpScaleAmplitude;
//     [Tooltip("Midpoint value of warp scale.")]
//     public float warpScaleMean;
//     public float amplitudePower;

//     [Header("Parameters for seed-based perturbation.")]
//     public float scaleMeanAmplitude;
//     public float sharpnessScaleAmplitude; 
//     public float sharpnessAmplitudeAmplitude;
//     public float sharpnessMeanAmplitude;
//     public float amplitudeScaleAmplitude;
//     public float amplitudeAmplitudeAmplitude;
//     public float amplitudeMeanAmplitude;
//     public float warpStrengthAmplitudeAmplitude; 
//     public float warpStrengthMeanAmplitude;
//     public float warpScaleMeanAmplitude;
//     public float amplitudePowerAmplitude;

//     public void Perturb(int seed) {
//       scaleMean += (Mathf.PerlinNoise(seed % 296.13f, seed % 906.13f)) * scaleMeanAmplitude;
//       sharpnessScale += (Mathf.PerlinNoise(seed % 751.92f, seed % 601.93f)) * sharpnessScaleAmplitude;
//       sharpnessAmplitude += (Mathf.PerlinNoise(seed % 968.01f, seed % 981.24f) - 0.5f) * sharpnessAmplitudeAmplitude;
//       sharpnessMean += (Mathf.PerlinNoise(seed % 214.25f, seed % 591.85f)) * sharpnessMeanAmplitude;
//       amplitudeScale += (Mathf.PerlinNoise(seed % 172.82f, seed % 918.96f)) * amplitudeScaleAmplitude;
//       amplitudeAmplitude += (Mathf.PerlinNoise(seed % 619.34f, seed % 729.14f) - 0.5f) * amplitudeAmplitudeAmplitude;
//       amplitudeMean += (Mathf.PerlinNoise(seed % 481.83f, seed % 389.06f)) * amplitudeMeanAmplitude;
//       warpStrengthAmplitude += (Mathf.PerlinNoise(seed % 195.12f, seed % 702.18f) - 0.5f) * warpStrengthAmplitudeAmplitude;
//       warpStrengthMean += (Mathf.PerlinNoise(seed % 810.53f, seed % 109.52f) - 0.5f) * warpStrengthMeanAmplitude;
//       warpScaleMeanAmplitude += (Mathf.PerlinNoise(seed % 639.14f, seed % 561.92f)) * warpScaleMeanAmplitude;
//       amplitudePower += (Mathf.PerlinNoise(seed % 591.03f, seed % 329.51f) - 0.5f) * amplitudePowerAmplitude;
//     }

//   }

//   [System.Serializable]
//   private struct RiverParams 
//   {
//     [Tooltip("Scale of river noise.")]
//     public float scale;
//     [Tooltip("Maximum depth of rivers.")]
//     public float amplitude;
//     [Tooltip("Octaves of river noise.")]
//     public int octaves;
//     [Tooltip("Scale factor between octaves of river noise.")]
//     public float lacunarity;
//     [Tooltip("Amplitude factor between octaves of river noise.")]
//     public float persistence;
//     [Tooltip("Warp scale of river noise.")]
//     public float warpScale;
//     [Tooltip("Warp strength of river noise.")]
//     public float warpStrength;
//     [Tooltip("Downwards offset of water level.")]
//     public float waterLevel;
//     public AnimationCurve noiseCurve;
//     public AnimationCurve heightCurve;
//     public AnimationCurve normalCurve;
//     [Tooltip("Water rendering object.")]
//     public GameObject obj;

//   }

//   #endregion
  
//   #region Properties

//   [Header("World Generation")]
//   [Tooltip("Seed for world generation.")]
//   public int _seed;
//   [Tooltip("Size of world in tiles.")]
//   [SerializeField] private int _tileCount;
//   [Tooltip("Vertices of each tile in world (lowest LOD).")]
//   [SerializeField] private int _size;
//   [Tooltip("Vertex spacing of each tile in world (lowest LOD).")]
//   [SerializeField] private float _resolution;
//   [Tooltip("Maximum chunk updates per frame.")]
//   [SerializeField] private float _maxUpdatesPerFrame;
//   [Tooltip("Enable or disable colliders.")]
//   [SerializeField] private bool _enableColliders;
//   [Tooltip("Maximum distance from player with colliders.")]
//   [SerializeField] private int _colliderRange;
//   [Tooltip("Threshold to force colliders to update.")]
//   [SerializeField] private int _forceBakeThreshold;
//   [Tooltip("Material for world mesh.")]
//   [SerializeField] private Material _material;
//   [SerializeField] private AmalgamNoiseParams _noiseParameters;
//   [Tooltip("Refresh world generation.")]

//   [SerializeField] private bool _refreshButton = false;
//   [Header("LOD Settings")]
  
//   [SerializeField] private LODLevel[] _lodLevels;
//   [Tooltip("LOD level outside of specified range.")]
//   [SerializeField] private int _defaultLOD;

//   [Header("Water Settings")]
//   [Tooltip("Enable or disable rivers.")]
//   [SerializeField] private bool _enableRivers;
//   [SerializeField] private RiverParams _riverParameters;
//   [Tooltip("Lake plane height.")]
//   [SerializeField] private float _lakePlaneHeight;

//   [Header("Script Connections")]
//   [SerializeField] private SecondaryStructures _structures;
//   [SerializeField] private PlaceStructures _storyStructures;

//   #endregion

//   #region Internal Variables

//   private WorldTile[] _tilePool;
//   private int[,] _tilePositions;
//   private List<Vector2> _generatedStructureTiles = new List<Vector2>();
//   private List<int> _updateQueue = new List<int>();
//   private List<int[]> _generateQueue = new List<int[]>();
//   private List<int> _frameColliderBakeBuffer = new List<int>();
//   private float updatesLeft;
//   private bool _bakingColliders;

//   private float _playerX;
//   private float _playerZ;
//   private int _lastPlayerChunkX;
//   private int _lastPlayerChunkZ;
//   private int _playerXChunkScale;
//   private int _playerZChunkScale;

//   private float _maxPossibleHeight;
//   private Mesh _waterMesh;

//   private bool _doneGenerating = false;
//   private int _currentlyUpdating = 0;
//   private int _currentlyGenerating = 0;

//   public delegate void OnGenerationComplete();
  
//   public static event OnGenerationComplete GenerationComplete;

//   #endregion

//   #region Public Fetch Functions

//   public float GetHeightValue(Vector2 worldPosition) {
//     float heightVal = AmalgamNoise.GetPosition(worldPosition.x + _seed, worldPosition.y + _seed, _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
//       _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
//       _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
//       _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
//       _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean, _noiseParameters.amplitudePower);
//     heightVal -= _riverParameters.heightCurve.Evaluate(heightVal / _maxPossibleHeight) * (_riverParameters.noiseCurve.Evaluate(AmalgamNoise.GetRiverValue(
//         worldPosition.x + _seed % 216812, worldPosition.y + _seed % 216812, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength)) * _riverParameters.amplitude);
//     return heightVal;
//   }

//   public float GetRiverValue(Vector2 worldPosition) {
//     return _riverParameters.heightCurve.Evaluate(worldPosition.y / _maxPossibleHeight) * _riverParameters.noiseCurve.Evaluate(AmalgamNoise.GetRiverValue(
//       worldPosition.x + _seed % 216812, worldPosition.y + _seed % 216812, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength));
//   }

//   public float GetWaterHeight(Vector2 worldPosition) {
//     float heightVal = AmalgamNoise.GetPosition(worldPosition.x + _seed, worldPosition.y + _seed, _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
//       _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
//       _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
//       _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
//       _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean, _noiseParameters.amplitudePower);
//     float waterFactor = _riverParameters.heightCurve.Evaluate(heightVal / _maxPossibleHeight) * _riverParameters.noiseCurve.Evaluate(AmalgamNoise.GetRiverValue(
//         worldPosition.x + _seed % 216812, worldPosition.y + _seed % 216812, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength));
//     return waterFactor == 0 ? -1 : (heightVal - _riverParameters.waterLevel);
//   }

//   public float GetSeedHash() {
//     return _seed;
//   }

//   public (Vector3[][], Vector2[]) GetVertices(int distance) {
//     int included = 0;
//     for (int i = 0; i < _tileCount * _tileCount; i++) {
//       if (Mathf.Sqrt(Mathf.Pow(_tilePool[i].x, 2) + Mathf.Pow(_tilePool[i].z, 2)) <= distance) included++;
//     }
//     Vector3[][] vertices = new Vector3[included][];
//     Vector2[] positions = new Vector2[included];
//     for (int i = 0, j = 0; i < _tileCount * _tileCount; i++) {
//       if (Mathf.Sqrt(Mathf.Pow(_tilePool[i].x, 2) + Mathf.Pow(_tilePool[i].z, 2)) <= distance) {
//         vertices[j] = _tilePool[i].mesh.vertices;
//         positions[j] = new Vector2(_tilePool[i].obj.transform.position.x, _tilePool[i].obj.transform.position.z);
//         j++;
//       }
//     }

//     return (vertices, positions);
//   }

//   #endregion

//   #region Unity Functions

//   private void OnValidate() {
//     if (_refreshButton) {
//       _refreshButton = false;
//       Regenerate();
//     }
//   }

//   private void Awake() {
//     _maxPossibleHeight = _noiseParameters.amplitudeMean + _noiseParameters.amplitudeAmplitude;
//     _waterMesh = new Mesh();
//     _waterMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//     _riverParameters.obj.GetComponent<MeshFilter>().mesh = _waterMesh;
//     WorldGenInfo._seed = _seed;
//     WorldGenInfo._maxUpdatesPerFrame = _maxUpdatesPerFrame;
//     WorldGenInfo._lakePlaneHeight = _lakePlaneHeight - _riverParameters.waterLevel;
//     _seed = int.Parse(Hash128.Compute(_seed).ToString().Substring(0, 6), System.Globalization.NumberStyles.HexNumber);
//     // Debug.Log(_seed);
//     // Seed-based terrain parameter changes
//     _noiseParameters.Perturb(_seed);
//   }

//   private void Start()  {
//     SetupPool();
//   }

//   private void Update() {
//     if ((_frameColliderBakeBuffer.Count > 0 && _generateQueue.Count == 0) || _frameColliderBakeBuffer.Count > _forceBakeThreshold) {
//       if (!_bakingColliders) StartCoroutine(BakeColliders());
//     }
//     while (updatesLeft > 0) {
//       if (_updateQueue.Count > 0 && _doneGenerating) {
//         StartCoroutine(UpdateTileJobs(_updateQueue[0]));
//         //UpdateTile(_updateQueue[0]);
//         _updateQueue.RemoveAt(0);
//         updatesLeft--;
//       } else break;
//     }
//     for (int i = 0; i < _maxUpdatesPerFrame * 250; i++) {
//       if (_generateQueue.Count > 0) {
//         StartCoroutine(GenerateTileJobs(_generateQueue[0][0], _generateQueue[0][1], _generateQueue[0][2]));
//         //GenerateTile(_generateQueue[0][0], _generateQueue[0][1], _generateQueue[0][2]);
//         _generateQueue.RemoveAt(0);
//         if (Time.timeScale == 0f) Time.timeScale = 1f;
//       } else break;
//     }
//     updatesLeft += WorldGenInfo._maxUpdatesPerFrame;
//     updatesLeft = Mathf.Min(updatesLeft, Mathf.Max(WorldGenInfo._maxUpdatesPerFrame, 1));
//   }

//   #endregion

//   #region Chunk Management Functions

//   public void Regenerate() {
//     for (int i = 0; i < _tilePool.Length; i++) {
//       Destroy(_tilePool[i].obj);
//     }
//     SetupPool();
//   }

//   private void SetupPool() {
//     Time.timeScale = 0f;
//     _tilePool = new WorldTile[_tileCount * _tileCount];
//     _tilePositions = new int[_tileCount, _tileCount];
//     for (int x = -(_tileCount - 1) / 2, i = 0; x <= (_tileCount - 1) / 2; x++)
//     {
//       for (int z = -(_tileCount - 1) / 2; z <= (_tileCount - 1) / 2; z++) {
//         QueueTileGen(x, z, i);
//         i++;
//       }
//     }
//     _generateQueue.Sort((c1, c2) => (Mathf.Abs(c1[0]) + Mathf.Abs(c1[1])).CompareTo(Mathf.Abs(c2[0]) + Mathf.Abs(c2[1])));
//   }

//   private void QueueTileUpdate(int index) {
//     _updateQueue.Add(index);
//   }

//   private void QueueTileGen(int x, int z, int index) {
//     _generateQueue.Add(new int[] {
//       x, z, index
//     }); 
//   }

//   public void UpdatePlayerLoadedChunks(Vector3 playerPos) {
//     if (_generateQueue.Count > 0) return;
//     int playerXChunkScale = Mathf.FloorToInt(playerPos.x / (_size * _resolution));
//     int playerZChunkScale = Mathf.FloorToInt(playerPos.z / (_size * _resolution));

//     _playerXChunkScale = playerXChunkScale;
//     _playerZChunkScale = playerZChunkScale;
//     _playerX = playerPos.x;
//     _playerZ = playerPos.z;

//     if (playerXChunkScale - _lastPlayerChunkX > 1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX - 1;
//     if (playerZChunkScale - _lastPlayerChunkZ > 1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ - 1;
//     if (playerXChunkScale - _lastPlayerChunkX < -1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX + 1;
//     if (playerZChunkScale - _lastPlayerChunkZ < -1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ + 1;

//     int deltaX = playerXChunkScale - _lastPlayerChunkX;
//     int deltaZ = playerZChunkScale - _lastPlayerChunkZ;
    
//     if (deltaX < 0) {
//       int[] tempValues = new int[_tileCount];
//       for (int i = 0; i < _tileCount; i++) {
//         tempValues[i] = _tilePositions[_tileCount - 1, i];
//       }
      
//       for (int x = _tileCount - 1; x > 0; x--) {
//         for (int z = 0; z < _tileCount; z++) {
//           _tilePositions[x, z] = _tilePositions[x - 1, z];
//         }
//       }
      
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePositions[0, i] = tempValues[i];
//       }
      
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePool[_tilePositions[0, i]].x = playerXChunkScale - (_tileCount - 1) / 2;
//         _tilePool[_tilePositions[0, i]].z = i + playerZChunkScale - (_tileCount - 1) / 2;
//         QueueTileUpdate(_tilePositions[0, i]);
//       }
//     }
//     else if (deltaX > 0) {
//       int[] tempValues = new int[_tileCount];
//       for (int i = 0; i < _tileCount; i++) {
//         tempValues[i] = _tilePositions[0, i];
//       }
      
//       for (int x = 0; x < _tileCount - 1; x++) {
//         for (int z = 0; z < _tileCount; z++) {
//           _tilePositions[x, z] = _tilePositions[x + 1, z];
//         }
//       }
      
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePositions[_tileCount - 1, i] = tempValues[i];
//       }
      
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePool[_tilePositions[_tileCount - 1, i]].x = playerXChunkScale + (_tileCount - 1) / 2;
//         _tilePool[_tilePositions[_tileCount - 1, i]].z = i + playerZChunkScale - (_tileCount - 1) / 2;
//         QueueTileUpdate(_tilePositions[_tileCount - 1, i]);
//       }
//     }

//     if (deltaZ < 0) {
//       int[] tempValues = new int[_tileCount];
//       for (int i = 0; i < _tileCount; i++) {
//         tempValues[i] = _tilePositions[i, _tileCount - 1];
//       }
      
//       for (int x = 0; x < _tileCount; x++) {
//         for (int z = _tileCount - 1; z > 0; z--) {
//           _tilePositions[x, z] = _tilePositions[x, z - 1];
//         }
//       }
      
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePositions[i, 0] = tempValues[i];
//       }
      
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePool[_tilePositions[i, 0]].x = i + playerXChunkScale - (_tileCount - 1) / 2;
//         _tilePool[_tilePositions[i, 0]].z = playerZChunkScale - (_tileCount - 1) / 2;
//         QueueTileUpdate(_tilePositions[i, 0]);
//       }
//     }
//     else if (deltaZ > 0) {
//       int[] tempValues = new int[_tileCount];
//       for (int i = 0; i < _tileCount; i++) {
//         tempValues[i] = _tilePositions[i, 0];
//       }
      
//       for (int x = 0; x < _tileCount; x++) {
//         for (int z = 0; z < _tileCount - 1; z++) {
//           _tilePositions[x, z] = _tilePositions[x, z + 1];
//         }
//       }
      
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePositions[i, _tileCount - 1] = tempValues[i];
//       }
//       for (int i = 0; i < _tileCount; i++) {
//         _tilePool[_tilePositions[i, _tileCount - 1]].x = i + playerXChunkScale - (_tileCount - 1) / 2;
//         _tilePool[_tilePositions[i, _tileCount - 1]].z = playerZChunkScale + (_tileCount - 1) / 2;
//         QueueTileUpdate(_tilePositions[i, _tileCount - 1]);
//       }
//     }

//     if (deltaZ != 0 || deltaX != 0) {
//       _structures.CheckStructures(new Vector2(playerPos.x, playerPos.z));
//       _storyStructures.CheckStructures(new Vector2(playerPos.x, playerPos.z));
//       _updateQueue.Sort((c1, c2) => (Mathf.Abs(_tilePool[c1].x - _playerXChunkScale) + Mathf.Abs(_tilePool[c1].z - _playerZChunkScale)).CompareTo(Mathf.Abs(_tilePool[c2].x - _playerXChunkScale) + Mathf.Abs(_tilePool[c2].z - _playerZChunkScale)));
//       for (int x = 0; x < _tileCount; x++) {
//         for (int z = 0; z < _tileCount; z++) { 
//           float maxDistance = Mathf.Max(Mathf.Abs(_tilePool[_tilePositions[x, z]].x - playerXChunkScale), Mathf.Abs(_tilePool[_tilePositions[x, z]].z - playerZChunkScale));
//           if (GetLOD(new Vector2(_tilePool[_tilePositions[x, z]].x, _tilePool[_tilePositions[x, z]].z), new Vector2(playerXChunkScale, playerZChunkScale)) != _tilePool[_tilePositions[x, z]].currentLOD) {
//             _tilePool[_tilePositions[x, z]].currentLOD = GetLOD(new Vector2(_tilePool[_tilePositions[x, z]].x, _tilePool[_tilePositions[x, z]].z), new Vector2(playerXChunkScale, playerZChunkScale));
//             QueueTileUpdate(_tilePositions[x, z]);
//             continue;
//           }
//           if (_enableColliders && maxDistance < _colliderRange) { 
//             if (_tilePool[_tilePositions[x, z]].meshCollider == null || !_tilePool[_tilePositions[x, z]].meshCollider.enabled) UpdateCollider(_tilePositions[x, z]);
//           }
//         }
//       }
//     }

//     _lastPlayerChunkX = playerXChunkScale;
//     _lastPlayerChunkZ = playerZChunkScale;
//   }

//   #endregion

//   #region Tile Generation Functions

//   private int GetLOD(Vector2 playerChunkCoords, Vector2 chunkCoords) {
//     int lod = _defaultLOD;
    
//     for (int i = 0; i < _lodLevels.Length; i++) {
//       if (Mathf.Max(Mathf.Abs(playerChunkCoords.x - chunkCoords.x), Mathf.Abs(playerChunkCoords.y - chunkCoords.y)) < _lodLevels[i].distance) {
//       lod = _lodLevels[i].factor;
//       break;
//       }
//     }

//     return lod;
//   }

//   private void GenerateTile(int x, int z, int index) {
//     GameObject go = new GameObject("Tile");
//     go.transform.parent = transform;
//     MeshFilter mf = go.AddComponent<MeshFilter>();
//     MeshRenderer mr = go.AddComponent<MeshRenderer>();
//     mr.material = _material;
//     mf.mesh = new Mesh();
//     Mesh msh = mf.mesh;
//     WorldTile tile = new WorldTile();
//     int lodFactor = GetLOD(new Vector2(_playerXChunkScale, _playerZChunkScale), new Vector2(x, z));
//     tile.currentLOD = lodFactor;
//     lodFactor = (int) Mathf.Pow(2, lodFactor);
//     if (_size * lodFactor * _size * lodFactor > 65000) {
//         msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//     }
    
//     Vector3[] result = AmalgamNoise.GenerateTerrain(_size, lodFactor, x * _size * _resolution + _seed, z * _size * _resolution + _seed, _resolution / lodFactor, _resolution / lodFactor,
//         _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
//         _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
//         _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
//         _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
//         _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean, _noiseParameters.amplitudePower);
//     msh.vertices = result;
    
//     go.transform.position = new Vector3(x * _size * _resolution, 0, z * _size * _resolution);
//     go.isStatic = true;
    
//     // If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
//     go.tag = "Ground";
//     go.layer = 6;
//     _structures.GenerateChunkStructures(new Vector2(x * _size * _resolution, z * _size * _resolution), new Vector2((x + 1) * _size * _resolution, (z + 1) * _size * _resolution));
//     _generatedStructureTiles.Add(new Vector2(x, z));
//     tile.mesh = msh;
//     tile.obj = go;
//     tile.x = x;
//     tile.z = z;
//     _tilePool[index] = tile;
//     _tilePositions[index / _tileCount, index % _tileCount] = index;

//     WindTriangles(msh);
//     UpdateMesh(msh, index);
//     ScatterTile(index);
//     float maxDistance = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
//     if (_enableColliders && maxDistance <= _colliderRange) UpdateCollider(index);

//     if (index == (_tileCount * _tileCount) - 1) {
//       // Generation Complete
//       UpdateWaterMesh();
//       GenerationComplete?.Invoke();
//     }
//   }

//   private void UpdateTile(int index) {
//     int x = _tilePool[index].x;
//     int z = _tilePool[index].z;
//     int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//     if (_size * lodFactor * _size * lodFactor > 65000) {
//         _tilePool[index].mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//     }
//     float maxDistance = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x - _playerXChunkScale), 2) + Mathf.Pow(Mathf.Abs(z - _playerZChunkScale), 2));
//     Vector3[] result = AmalgamNoise.GenerateTerrain(_size, lodFactor, x * _size * _resolution + _seed, z * _size * _resolution + _seed, _resolution / lodFactor, _resolution / lodFactor,
//         _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
//         _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
//         _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
//         _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
//         _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean, _noiseParameters.amplitudePower);
    
//     WriteToMesh(_tilePool[index].mesh, result);

//     _tilePool[index].obj.transform.position = new Vector3(x * _size * _resolution, 0, z * _size * _resolution);
//     if (!_generatedStructureTiles.Contains(new Vector2(x, z))) {
//         _structures.GenerateChunkStructures(new Vector2(x * _size * _resolution, z * _size * _resolution), new Vector2((x + 1) * _size * _resolution, (z + 1) * _size * _resolution));
//         _generatedStructureTiles.Add(new Vector2(x, z));
//     }
//     UpdateMesh(_tilePool[index].mesh, index);
//     ScatterTile(index);
//     maxDistance = Mathf.Max(Mathf.Abs(x - _playerXChunkScale), Mathf.Abs(z - _playerZChunkScale));
//     if (_enableColliders && maxDistance <= _colliderRange) UpdateCollider(index);
//   }

//   private void ScatterTile(int index) {
//     // Implement idk
//   }

//   private IEnumerator GenerateTileJobs(int x, int z, int index) {
//     _currentlyGenerating++;
//     GameObject go = new GameObject("Tile");
//     go.transform.parent = transform;
//     MeshFilter mf = go.AddComponent<MeshFilter>();
//     MeshRenderer mr = go.AddComponent<MeshRenderer>();
//     mr.material = _material;
//     mf.mesh = new Mesh();
//     Mesh msh = mf.mesh;
//     WorldTile tile = new WorldTile();
//     int lodFactor = GetLOD(new Vector2(_playerXChunkScale, _playerZChunkScale), new Vector2(x, z));
//     tile.currentLOD = lodFactor;
//     lodFactor = (int) Mathf.Pow(2, lodFactor);
//     go.transform.position = new Vector3(x * _size * _resolution, 0, z * _size * _resolution);
//     go.isStatic = true;
    
//     // If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
//     go.tag = "Ground";
//     go.layer = 6;
//     _structures.GenerateChunkStructures(new Vector2(x * _size * _resolution, z * _size * _resolution), new Vector2((x + 1) * _size * _resolution, (z + 1) * _size * _resolution));
//     _generatedStructureTiles.Add(new Vector2(x, z));
//     tile.mesh = msh;
//     tile.obj = go;
//     tile.x = x;
//     tile.z = z;
//     _tilePool[index] = tile;
//     _tilePositions[index / _tileCount, index % _tileCount] = index;
    
//     int tmpSize = _size * lodFactor + 5;
//     float tmpRes = _resolution / lodFactor;
//     NativeArray<float> output = new NativeArray<float>(tmpSize * tmpSize, Allocator.Persistent);
//     NoiseJob job = new NoiseJob {
//       size = tmpSize,
//       overlap = 1,
//       xOffset = x * _size * _resolution + _seed,
//       zOffset = z * _size * _resolution + _seed,
//       xResolution = tmpRes,
//       zResolution = tmpRes,
//       octaves = _noiseParameters.octaves,
//       lacunarity = _noiseParameters.lacunarity,
//       persistence = _noiseParameters.persistence,
//       sharpnessScale = 1f / _noiseParameters.sharpnessScale,
//       sharpnessAmplitude = _noiseParameters.sharpnessAmplitude,
//       sharpnessMean = _noiseParameters.sharpnessMean,
//       scaleScale = 1f / _noiseParameters.scaleScale,
//       scaleAmplitude = _noiseParameters.scaleAmplitude,
//       scaleMean = _noiseParameters.scaleMean,
//       amplitudeScale = 1f / _noiseParameters.amplitudeScale,
//       amplitudeAmplitude = _noiseParameters.amplitudeAmplitude,
//       amplitudeMean = _noiseParameters.amplitudeMean,
//       warpStrengthScale = 1f / _noiseParameters.warpStrengthScale,
//       warpStrengthAmplitude = _noiseParameters.warpStrengthAmplitude,
//       warpStrengthMean = _noiseParameters.warpStrengthMean,
//       warpScaleScale = 1f / _noiseParameters.warpScaleScale,
//       warpScaleAmplitude = _noiseParameters.warpScaleAmplitude,
//       warpScaleMean = _noiseParameters.warpScaleMean,
//       amplitudePower = _noiseParameters.amplitudePower,
//       output = output
//     };
//     JobHandle handle = job.Schedule(tmpSize * tmpSize, 64);
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();
//     NativeArray<Vector3> vertices = new NativeArray<Vector3>(tmpSize * tmpSize, Allocator.Persistent);
//     for (int i = 0; i < tmpSize * tmpSize; i++) {
//       vertices[i] = new Vector3((i % tmpSize - 1) * tmpRes, output[i], (i / tmpSize - 1) * tmpRes);
//     }
//     output.Dispose();

//     int sideLength0 = (int) Mathf.Sqrt(vertices.Length) - 1;
//     NativeArray<int> triangles = new NativeArray<int>(sideLength0 * sideLength0 * 6, Allocator.Persistent);
//     int vert = 0;
//     int tris = 0;
//     for (int z2 = 0; z2 < sideLength0; z2++)
//     {
//       for (int x2 = 0; x2 < sideLength0; x2++)
//       {
//         triangles[tris] = vert;
//         triangles[tris + 1] = vert + sideLength0 + 1;
//         triangles[tris + 2] = vert + 1;
//         triangles[tris + 3] = vert + 1;
//         triangles[tris + 4] = vert + sideLength0 + 1;
//         triangles[tris + 5] = vert + sideLength0 + 2;
//         vert++;
//         tris += 6;
//       }
//       vert++;
//     }

//     NativeArray<Vector3> normals = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);
//     NormalJobManaged normalJob = new NormalJobManaged {
//       vertices = vertices,
//       triangles = triangles,
//       normals = normals
//     };

//     handle = normalJob.Schedule(triangles.Length / 3, 64);
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();
    
//     NativeArray<float> heightMods = new NativeArray<float>(vertices.Length, Allocator.Persistent);
//     RiverJob riverJob = new RiverJob {
//       size = tmpSize,
//       octaves = _riverParameters.octaves,
//       lacunarity = _riverParameters.lacunarity,
//       persistence = _riverParameters.persistence,
//       xOffset = x * _size * _resolution + _seed % 216812,
//       zOffset = z * _size * _resolution + _seed % 216812,
//       xResolution = tmpRes,
//       zResolution = tmpRes,
//       scale = 1f / _riverParameters.scale,
//       warpScale = 1f / _riverParameters.warpScale,
//       warpStrength = _riverParameters.warpStrength,
//       output = heightMods
//     };
//     handle = riverJob.Schedule(tmpSize * tmpSize, 64);
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();

//     for (int i = 0; i < heightMods.Length; i++) heightMods[i] = _riverParameters.noiseCurve.Evaluate(heightMods[i]) * _riverParameters.heightCurve.Evaluate(vertices[i].y / _maxPossibleHeight) * _riverParameters.normalCurve.Evaluate(Mathf.Abs(normals[i].y));

//     NativeList<Vector3> waterVertsFinal = new NativeList<Vector3>(0, Allocator.Persistent);
//     NativeList<int> waterTrisFinal = new NativeList<int>(0, Allocator.Persistent);

//     RiverPassJob riverPassJob = new RiverPassJob {
//       vertices = vertices,
//       heightMods = heightMods,
//       positionOffset = _tilePool[index].obj.transform.position,
//       waterLevel = _riverParameters.waterLevel,
//       waterAmplitude = _riverParameters.amplitude,
//       lakePlaneHeight = _lakePlaneHeight,
//       triangles = triangles,
//       waterVertsOut = waterVertsFinal,
//       waterTrisOut = waterTrisFinal
//     };

//     handle = riverPassJob.Schedule();
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();

//     _tilePool[index].waterVertices = waterVertsFinal.AsArray().ToArray();
//     _tilePool[index].waterTriangles = waterTrisFinal.AsArray().ToArray();
//     // Debug.Log(waterVertsFinal.Length);
//     // Debug.Log(_tilePool[index].waterVertices.Length);
//     waterVertsFinal.Dispose();
//     waterTrisFinal.Dispose();
//     heightMods.Dispose();

//     Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
//     WriteMeshJob meshJob = new WriteMeshJob {
//       vertices = vertices,
//       triangles = triangles,
//       normals = normals,
//       mesh = meshDataArray[0]
//     };
    
//     handle = meshJob.Schedule();
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();
    
//     Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, msh);
//     msh.RecalculateBounds();

//     vertices.Dispose();
//     triangles.Dispose();
//     normals.Dispose();
    
//     //RiverPass(msh, index);
//     ScatterTile(index);
//     float maxDistance = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
//     if (_enableColliders && maxDistance <= _colliderRange) UpdateCollider(index);

//     _currentlyGenerating--;
//     if (_currentlyGenerating == 0 && _generateQueue.Count == 0) {
//       _doneGenerating = true;
//       StartCoroutine(WaterMeshUpdate());
//     }
//   }

//   private IEnumerator UpdateTileJobs(int index) {
//     _currentlyUpdating++;
//     int x = _tilePool[index].x;
//     int z = _tilePool[index].z;
//     int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//     // if (_size * lodFactor * _size * lodFactor > 65000) {
//     //     _tilePool[index].mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//     // }
//     float maxDistance = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x - _playerXChunkScale), 2) + Mathf.Pow(Mathf.Abs(z - _playerZChunkScale), 2));
//     int tmpSize = _size * lodFactor + 5;
//     float tmpRes = _resolution / lodFactor;
//     NativeArray<float> output = new NativeArray<float>(tmpSize * tmpSize, Allocator.Persistent);
//     NoiseJob job = new NoiseJob {
//       size = tmpSize,
//       overlap = 1,
//       xOffset = x * _size * _resolution + _seed,
//       zOffset = z * _size * _resolution + _seed,
//       xResolution = tmpRes,
//       zResolution = tmpRes,
//       octaves = _noiseParameters.octaves,
//       lacunarity = _noiseParameters.lacunarity,
//       persistence = _noiseParameters.persistence,
//       sharpnessScale = 1f / _noiseParameters.sharpnessScale,
//       sharpnessAmplitude = _noiseParameters.sharpnessAmplitude,
//       sharpnessMean = _noiseParameters.sharpnessMean,
//       scaleScale = 1f / _noiseParameters.scaleScale,
//       scaleAmplitude = _noiseParameters.scaleAmplitude,
//       scaleMean = _noiseParameters.scaleMean,
//       amplitudeScale = 1f / _noiseParameters.amplitudeScale,
//       amplitudeAmplitude = _noiseParameters.amplitudeAmplitude,
//       amplitudeMean = _noiseParameters.amplitudeMean,
//       warpStrengthScale = 1f / _noiseParameters.warpStrengthScale,
//       warpStrengthAmplitude = _noiseParameters.warpStrengthAmplitude,
//       warpStrengthMean = _noiseParameters.warpStrengthMean,
//       warpScaleScale = 1f / _noiseParameters.warpScaleScale,
//       warpScaleAmplitude = _noiseParameters.warpScaleAmplitude,
//       warpScaleMean = _noiseParameters.warpScaleMean,
//       amplitudePower = _noiseParameters.amplitudePower,
//       output = output
//     };
//     JobHandle handle = job.Schedule(tmpSize * tmpSize, 64);
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();
//     NativeArray<Vector3> vertices = new NativeArray<Vector3>(tmpSize * tmpSize, Allocator.Persistent);
//     for (int i = 0; i < tmpSize * tmpSize; i++) {
//       vertices[i] = new Vector3((i % tmpSize - 1) * tmpRes, output[i], (i / tmpSize - 1) * tmpRes);
//     }
//     output.Dispose();

//     _tilePool[index].obj.transform.position = new Vector3(x * _size * _resolution, 0, z * _size * _resolution);
//     if (!_generatedStructureTiles.Contains(new Vector2(x, z))) {
//         _structures.GenerateChunkStructures(new Vector2(x * _size * _resolution, z * _size * _resolution), new Vector2((x + 1) * _size * _resolution, (z + 1) * _size * _resolution));
//         _generatedStructureTiles.Add(new Vector2(x, z));
//     }

//     int sideLength0 = (int) Mathf.Sqrt(vertices.Length) - 1;
//     NativeArray<int> triangles = new NativeArray<int>(sideLength0 * sideLength0 * 6, Allocator.Persistent);
//     int vert = 0;
//     int tris = 0;
//     for (int z2 = 0; z2 < sideLength0; z2++)
//     {
//       for (int x2 = 0; x2 < sideLength0; x2++)
//       {
//         triangles[tris] = vert;
//         triangles[tris + 1] = vert + sideLength0 + 1;
//         triangles[tris + 2] = vert + 1;
//         triangles[tris + 3] = vert + 1;
//         triangles[tris + 4] = vert + sideLength0 + 1;
//         triangles[tris + 5] = vert + sideLength0 + 2;
//         vert++;
//         tris += 6;
//       }
//       vert++;
//     }

//     NativeArray<Vector3> normals = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);
//     NormalJobManaged normalJob = new NormalJobManaged {
//       vertices = vertices,
//       triangles = triangles,
//       normals = normals
//     };

//     handle = normalJob.Schedule(triangles.Length / 3, 64);
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();

//     NativeArray<float> heightMods = new NativeArray<float>(vertices.Length, Allocator.Persistent);
//     RiverJob riverJob = new RiverJob {
//       size = tmpSize,
//       octaves = _riverParameters.octaves,
//       lacunarity = _riverParameters.lacunarity,
//       persistence = _riverParameters.persistence,
//       xOffset = x * _size * _resolution + _seed % 216812,
//       zOffset = z * _size * _resolution + _seed % 216812,
//       xResolution = tmpRes,
//       zResolution = tmpRes,
//       scale = 1f / _riverParameters.scale,
//       warpScale = 1f / _riverParameters.warpScale,
//       warpStrength = _riverParameters.warpStrength,
//       output = heightMods
//     };
//     handle = riverJob.Schedule(tmpSize * tmpSize, 64);
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();

//     for (int i = 0; i < heightMods.Length; i++) heightMods[i] = _riverParameters.noiseCurve.Evaluate(heightMods[i]) * _riverParameters.heightCurve.Evaluate(vertices[i].y / _maxPossibleHeight) * _riverParameters.normalCurve.Evaluate(Mathf.Abs(normals[i].y));

//     NativeList<Vector3> waterVertsFinal = new NativeList<Vector3>(0, Allocator.Persistent);
//     NativeList<int> waterTrisFinal = new NativeList<int>(0, Allocator.Persistent);

//     RiverPassJob riverPassJob = new RiverPassJob {
//       vertices = vertices,
//       heightMods = heightMods,
//       positionOffset = _tilePool[index].obj.transform.position,
//       waterLevel = _riverParameters.waterLevel,
//       waterAmplitude = _riverParameters.amplitude,
//       lakePlaneHeight = _lakePlaneHeight,
//       triangles = triangles,
//       waterVertsOut = waterVertsFinal,
//       waterTrisOut = waterTrisFinal
//     };

//     handle = riverPassJob.Schedule();
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();

//     _tilePool[index].waterVertices = waterVertsFinal.AsArray().ToArray();
//     _tilePool[index].waterTriangles = waterTrisFinal.AsArray().ToArray();
//     waterVertsFinal.Dispose();
//     waterTrisFinal.Dispose();
//     heightMods.Dispose();

//     // Write data to the mesh after all passes

//     Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
//     WriteMeshJob meshJob = new WriteMeshJob {
//       vertices = vertices,
//       triangles = triangles,
//       normals = normals,
//       mesh = meshDataArray[0]
//     };
    
//     handle = meshJob.Schedule();
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();
    
//     Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _tilePool[index].mesh);
//     _tilePool[index].mesh.RecalculateBounds();
//     // Vector3[] vertManaged = new Vector3[vertices.Length];
//     // int[] triManaged = new int[triangles.Length];
//     // for (int i = 0; i < vertices.Length; i++) vertManaged[i] = vertices[i];
//     // for (int i = 0; i < triangles.Length; i++) triManaged[i] = triangles[i];
//     // _tilePool[index].mesh.triangles = null;
//     // _tilePool[index].mesh.vertices = vertManaged;
//     // _tilePool[index].mesh.triangles = triManaged;
//     vertices.Dispose();
//     triangles.Dispose();
//     normals.Dispose();
    
//     ScatterTile(index);
//     maxDistance = Mathf.Max(Mathf.Abs(x - _playerXChunkScale), Mathf.Abs(z - _playerZChunkScale));
//     if (_enableColliders && maxDistance <= _colliderRange) UpdateCollider(index);
//     _currentlyUpdating--;
//     if (_currentlyUpdating == 0 && _updateQueue.Count == 0) StartCoroutine(WaterMeshUpdate());
//   }

//   #endregion

//   #region Water Management Functions

//   private struct RiverPassJob : IJob
//   {

//     public NativeArray<Vector3> vertices;
//     public NativeArray<float> heightMods;

//     [ReadOnly] public Vector3 positionOffset;
//     [ReadOnly] public float waterLevel;
//     [ReadOnly] public float waterAmplitude;
//     [ReadOnly] public float lakePlaneHeight;
//     [ReadOnly] public int waterVertCount;
//     [ReadOnly] public NativeArray<int> triangles;

//     [WriteOnly] public NativeList<Vector3> waterVertsOut;
//     [WriteOnly] public NativeList<int> waterTrisOut;

//     public void Execute() {
//       Vector3[] waterVerts = new Vector3[vertices.Length];
//       bool[] ignoreVerts = new bool[vertices.Length];
//       int ignored = 0;
//       for (int i = 0; i < heightMods.Length; i++) {
//         if (vertices[i].y <= lakePlaneHeight) {
//           if (heightMods[i] != 0) vertices[i] -= new Vector3(0, heightMods[i] * waterAmplitude, 0);
//           waterVerts[i] = vertices[i];
//           waterVerts[i].y = lakePlaneHeight - waterLevel;
//           waterVerts[i] += positionOffset;
//           continue;
//         }
//         waterVerts[i] = vertices[i] - new Vector3(0, waterLevel, 0);
//         waterVerts[i] += positionOffset;
//         if (heightMods[i] == 0) {
//           ignoreVerts[i] = true;
//           ignored++;
//         } else {
//           vertices[i] -= new Vector3(0, heightMods[i] * waterAmplitude, 0);
//         }
//       }

//       int sideLength = (int) Mathf.Sqrt(vertices.Length);
//       int[] waterTriangles = new int[(sideLength - 1) * (sideLength - 1) * 6];
//       for (int i = 0, j = 0; i < waterVerts.Length; i++) {
//         if (i / sideLength >= sideLength - 1) continue;
//         if (i % sideLength >= sideLength - 1) continue;
//         if (i / sideLength == 0) continue;
//         if (i % sideLength == 0) continue;
//         waterTriangles[j * 6] = i;
//         waterTriangles[j * 6 + 1] = i + sideLength;
//         waterTriangles[j * 6 + 2] = i + 1;
//         waterTriangles[j * 6 + 3] = i + sideLength;
//         waterTriangles[j * 6 + 4] = i + sideLength + 1;
//         waterTriangles[j * 6 + 5] = i + 1;
//         j++;
//       }

//       int[] realIndices = new int[waterVerts.Length];
//       waterVertsOut.Length = waterVerts.Length - ignored;
//       for (int i = 0, j = 0; i < waterVerts.Length; i++) {
//         if (!ignoreVerts[i]) {
//           waterVertsOut[j] = waterVerts[i];
//           realIndices[i] = j;
//           j++;
//         }
//       }
//       int triCount = 0;
//       for (int i = 0; i < waterTriangles.Length; i+= 3) {
//         if (!ignoreVerts[waterTriangles[i]] && !ignoreVerts[waterTriangles[i + 1]] && !ignoreVerts[waterTriangles[i + 2]]) {
//           triCount += 3;
//         }
//       }
//       waterTrisOut.Length = triCount;
//       for (int i = 0, j = 0; i < triangles.Length; i+= 3) {
//         if (!ignoreVerts[waterTriangles[i]] && !ignoreVerts[waterTriangles[i + 1]] && !ignoreVerts[waterTriangles[i + 2]]) {
//           waterTrisOut[j] = realIndices[waterTriangles[i]];
//           waterTrisOut[j + 1] = realIndices[waterTriangles[i + 1]];
//           waterTrisOut[j + 2] = realIndices[waterTriangles[i + 2]];
//           j += 3;
//         }
//       }
//     }
//   }

//   private IEnumerator WaterMeshUpdate() {
//     int vertexCount = 0;
//     int triangleCount = 0;
//     for (int i = 0; i < _tilePool.Length; i++) {
//       if (_tilePool[i].waterVertices == null) continue;
//       vertexCount += _tilePool[i].waterVertices.Length;
//       triangleCount += _tilePool[i].waterTriangles.Length;
//     }

//     NativeArray<Vector3> waterVertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
//     NativeArray<int> waterTriangles = new NativeArray<int>(triangleCount, Allocator.Persistent);
//     for (int i = 0, v = 0, t = 0; i < _tilePool.Length; i++) {
//       if (_tilePool[i].waterVertices == null) continue;
//       for (int j = 0; j < _tilePool[i].waterTriangles.Length; j++, t++) {
//         waterTriangles[t] = _tilePool[i].waterTriangles[j] + v;
//       }
//       for (int j = 0; j < _tilePool[i].waterVertices.Length; j++, v++) {
//         waterVertices[v] = _tilePool[i].waterVertices[j];
//       }
//     }

//     var meshDataArray = Mesh.AllocateWritableMeshData(1);
//     var meshData = meshDataArray[0];

//     WaterMeshJob meshJob = new WaterMeshJob {
//       vertices = waterVertices,
//       triangles = waterTriangles,
//       mesh = meshData
//     };

//     JobHandle handle = meshJob.Schedule();
//     while (!handle.IsCompleted) yield return null;
//     handle.Complete();

//     Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _waterMesh);

//     waterVertices.Dispose();
//     waterTriangles.Dispose();

//   }

//   private struct WaterMeshJob : IJob
//   {

//     [ReadOnly] public NativeArray<Vector3> vertices;
//     [ReadOnly] public NativeArray<int> triangles;

//     public Mesh.MeshData mesh;

//     public void Execute() {
//       mesh.SetVertexBufferParams(vertices.Length, new VertexAttributeDescriptor(VertexAttribute.Position));

//       NativeArray<Vector3> positions = mesh.GetVertexData<Vector3>();
//       for (int i = 0; i < positions.Length; i++) positions[i] = vertices[i];
      
//       mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);
//       NativeArray<int> indices = mesh.GetIndexData<int>();
//       for (int i = 0; i < indices.Length; i++) indices[i] = triangles[i];

//       mesh.subMeshCount = 1;
//       mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
//     }

//   }

//   private void UpdateWaterMesh() {
//     int vertexCount = 0;
//     int triangleCount = 0;
//     for (int i = 0; i < _tilePool.Length; i++) {
//       if (_tilePool[i].waterVertices == null) continue;
//       vertexCount += _tilePool[i].waterVertices.Length;
//       triangleCount += _tilePool[i].waterTriangles.Length;
//     }
//     //Debug.Log(vertexCount);
//     Vector3[] waterVertices = new Vector3[vertexCount];
//     int[] waterTriangles = new int[triangleCount];
//     for (int i = 0, v = 0, t = 0; i < _tilePool.Length; i++) {
//       if (_tilePool[i].waterVertices == null) continue;
//       for (int j = 0; j < _tilePool[i].waterTriangles.Length; j++, t++) {
//         waterTriangles[t] = _tilePool[i].waterTriangles[j] + v;
//       }
//       for (int j = 0; j < _tilePool[i].waterVertices.Length; j++, v++) {
//         waterVertices[v] = _tilePool[i].waterVertices[j];
//       }
//     }

//     var meshDataArray = Mesh.AllocateWritableMeshData(1);
//     var meshData = meshDataArray[0];
    
//     meshData.SetVertexBufferParams(waterVertices.Length, new VertexAttributeDescriptor(VertexAttribute.Position));

//     NativeArray<Vector3> positions = meshData.GetVertexData<Vector3>();
//     for (int i = 0; i < positions.Length; i++) positions[i] = waterVertices[i];
    
//     meshData.SetIndexBufferParams(waterTriangles.Length, IndexFormat.UInt32);
//     NativeArray<int> indices = meshData.GetIndexData<int>();
//     for (int i = 0; i < indices.Length; i++) indices[i] = waterTriangles[i];

//     meshData.subMeshCount = 1;
//     meshData.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
//     Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _waterMesh);
//   }

//   // private void RiverPass(Mesh targetMesh, int index) {
//   //   if (_tilePool[index].waterVertCount > 0) {
//   //     _waterVertices.RemoveRange(_tilePool[index].waterVertIndex, _tilePool[index].waterVertCount);
//   //     _waterTriangles.RemoveRange(_tilePool[index].waterTriIndex, _tilePool[index].waterTriCount);
//   //     for (int i = 0; i < _waterTriangles.Count; i++) {
//   //       if (_waterTriangles[i] >= _tilePool[index].waterVertIndex) _waterTriangles[i] -= _tilePool[index].waterVertCount;
//   //     }
//   //     for (int i = 0; i < _tileCount * _tileCount; i++) {
//   //       if (_tilePool[i].waterVertIndex > _tilePool[index].waterVertIndex) {
//   //         _tilePool[i].waterVertIndex -= _tilePool[index].waterVertCount;
//   //       }
//   //       if (_tilePool[i].waterTriIndex > _tilePool[index].waterTriIndex) {
//   //         _tilePool[i].waterTriIndex -= _tilePool[index].waterTriCount;
//   //       }
//   //     }
//   //   }
//   //   _tilePool[index].waterVertCount = 0;
//   //   Vector3[] vertices = targetMesh.vertices;
//   //   Vector3[] normals = targetMesh.normals;
//   //   int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//   //   float[] heightMods = AmalgamNoise.GenerateRivers(_size, lodFactor, _tilePool[index].x * _size * _resolution + _seed % 216812,
//   //       _tilePool[index].z * _size * _resolution + _seed % 216812, _resolution / lodFactor, _resolution / lodFactor, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength);
//   //   Vector3[] waterVerts = new Vector3[vertices.Length];
//   //   bool[] ignoreVerts = new bool[vertices.Length];
//   //   int ignored = 0;
//   //   for (int i = 0; i < heightMods.Length; i++) {
//   //     heightMods[i] = _riverParameters.noiseCurve.Evaluate(heightMods[i]) * _riverParameters.heightCurve.Evaluate(vertices[i].y / _maxPossibleHeight) * _riverParameters.normalCurve.Evaluate(Mathf.Abs(normals[i].y));
//   //     if (vertices[i].y <= _lakePlaneHeight) {
//   //       if (heightMods[i] != 0) vertices[i] -= new Vector3(0, heightMods[i] * _riverParameters.amplitude, 0);
//   //       waterVerts[i] = new Vector3(vertices[i].x, _lakePlaneHeight - _riverParameters.waterLevel, vertices[i].z);
//   //       waterVerts[i] += _tilePool[index].obj.transform.position;
//   //       continue;
//   //     }
//   //     waterVerts[i] = vertices[i] - new Vector3(0, _riverParameters.waterLevel, 0);
//   //     waterVerts[i] += _tilePool[index].obj.transform.position;
//   //     if (heightMods[i] == 0) {
//   //       ignoreVerts[i] = true;
//   //       ignored++;
//   //     } else {
//   //       vertices[i] -= new Vector3(0, heightMods[i] * _riverParameters.amplitude, 0);
//   //     }
//   //   }
//   //
//   //   int sideLength = (int) Mathf.Sqrt(vertices.Length);
//   //   int[] triangles = new int[(sideLength - 1) * (sideLength - 1) * 6];
//   //   for (int i = 0, j = 0; i < waterVerts.Length; i++) {
//   //     if (i / sideLength >= sideLength - 1) continue;
//   //     if (i % sideLength >= sideLength - 1) continue;
//   //     if (i / sideLength == 0) continue;
//   //     if (i % sideLength == 0) continue;
//   //     triangles[j * 6] = i;
//   //     triangles[j * 6 + 1] = i + sideLength;
//   //     triangles[j * 6 + 2] = i + 1;
//   //     triangles[j * 6 + 3] = i + sideLength;
//   //     triangles[j * 6 + 4] = i + sideLength + 1;
//   //     triangles[j * 6 + 5] = i + 1;
//   //     j++;
//   //   }
//   //   targetMesh.vertices = vertices;
//   //   int waterVertsLength = _waterVertices.Count;
//   //   int waterTriLength = _waterTriangles.Count;
//   //   int[] realIndices = new int[waterVerts.Length];
//   //   Vector3[] verts = new Vector3[waterVerts.Length - ignored];
//   //   for (int i = 0, j = 0; i < waterVerts.Length; i++) {
//   //     if (!ignoreVerts[i]) {
//   //       verts[j] = waterVerts[i];
//   //       realIndices[i] = j;
//   //       j++;
//   //     }
//   //   }
//   //   int triCount = 0;
//   //   for (int i = 0; i < triangles.Length; i+= 3) {
//   //     if (!ignoreVerts[triangles[i]] && !ignoreVerts[triangles[i + 1]] && !ignoreVerts[triangles[i + 2]]) {
//   //       triCount += 3;
//   //     }
//   //   }
//   //   int[] tris = new int[triCount];
//   //   for (int i = 0, j = 0; i < triangles.Length; i+= 3) {
//   //     if (!ignoreVerts[triangles[i]] && !ignoreVerts[triangles[i + 1]] && !ignoreVerts[triangles[i + 2]]) {
//   //       tris[j] = realIndices[triangles[i]] + waterVertsLength;
//   //       tris[j + 1] = realIndices[triangles[i + 1]] + waterVertsLength;
//   //       tris[j + 2] = realIndices[triangles[i + 2]] + waterVertsLength;
//   //       j += 3;
//   //     }
//   //   }
//   //   _waterVertices.AddRange(verts);
//   //   _waterTriangles.AddRange(tris);
//   //   _tilePool[index].waterVertIndex = waterVertsLength;
//   //   _tilePool[index].waterVertCount = waterVerts.Length - ignored;
//   //   _tilePool[index].waterTriIndex = waterTriLength;
//   //   _tilePool[index].waterTriCount = _waterTriangles.Count - waterTriLength;
//   // }

//   #endregion

//   #region Mesh Operation Functions

//   private struct NormalJobManaged : IJobParallelFor
//   {

//     [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Vector3> vertices;
//     [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<int> triangles;

//     [NativeDisableParallelForRestriction] public NativeArray<Vector3> normals;

//     public void Execute(int index) {
//       int vertexIndexA = triangles[index * 3];
//       int vertexIndexB = triangles[index * 3 + 1];
//       int vertexIndexC = triangles[index * 3 + 2];
//       Vector3 pointA = vertices[vertexIndexA];
//       Vector3 normal = Vector3.Cross(vertices[vertexIndexB] - pointA, vertices[vertexIndexC] - pointA);
//       normals[vertexIndexA] += normal;
//       normals[vertexIndexB] += normal;
//       normals[vertexIndexC] += normal;
//     }

//   }

//   private struct Vertex
//   {

//     public Vector3 position;
//     public Vector3 normal;
    
//   }

//   private struct WriteMeshJob : IJob
//   {

//     [ReadOnly] public NativeArray<Vector3> vertices;
//     [ReadOnly] public NativeArray<Vector3> normals;
//     [ReadOnly] public NativeArray<int> triangles;
    
//     public Mesh.MeshData mesh;

//     public void Execute() {

//       mesh.SetVertexBufferParams(vertices.Length,
//       new VertexAttributeDescriptor(VertexAttribute.Position),
//       new VertexAttributeDescriptor(VertexAttribute.Normal));

//       NativeArray<Vertex> verts = mesh.GetVertexData<Vertex>();

//       for (int i = 0; i < verts.Length; i++) {
//         Vertex v = new Vertex();
//         v.position = vertices[i];
//         v.normal = normals[i].normalized;
//         verts[i] = v;
//       }

//       mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);
//       NativeArray<int> tris = mesh.GetIndexData<int>();
//       for (int i = 0; i < tris.Length; i++) tris[i] = triangles[i];
//       mesh.subMeshCount = 1;
//       mesh.SetSubMesh(0, new SubMeshDescriptor(0, tris.Length));

//     }

//   }

//   private void WriteToMesh(Mesh targetMesh, Vector3[] vertices) {
//     int sideLength = (int) Mathf.Sqrt(vertices.Length) - 1;
//     int[] triangles = new int[sideLength * sideLength * 6];
//     int vert = 0;
//     int tris = 0;
//     for (int z = 0; z < sideLength; z++)
//     {
//       for (int x = 0; x < sideLength; x++)
//       {
//         triangles[tris] = vert;
//         triangles[tris + 1] = vert + sideLength + 1;
//         triangles[tris + 2] = vert + 1;
//         triangles[tris + 3] = vert + 1;
//         triangles[tris + 4] = vert + sideLength + 1;
//         triangles[tris + 5] = vert + sideLength + 2;
//         vert++;
//         tris += 6;
//       }
//       vert++;
//     }

//     NativeArray<Vector3> vNativeArray = new NativeArray<Vector3>(vertices, Allocator.TempJob);
//     NativeArray<int> tNativeArray = new NativeArray<int>(triangles, Allocator.TempJob);

//     var meshDataArray = Mesh.AllocateWritableMeshData(1);
//     var meshData = meshDataArray[0];

//     meshData.SetVertexBufferParams(vertices.Length,
//       new VertexAttributeDescriptor(VertexAttribute.Position),
//       new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));

//     NativeArray<Vector3> pos = meshData.GetVertexData<Vector3>();
//     for (int i = 0; i < pos.Length; i++) pos[i] = vertices[i];

//     meshData.SetIndexBufferParams(triangles.Length, IndexFormat.UInt16);
//     NativeArray<ushort> indexBuffer = meshData.GetIndexData<ushort>();
//     for (int i = 0; i < indexBuffer.Length; i++)
//       indexBuffer[i] = (ushort) triangles[i];
//     meshData.subMeshCount = 1;
//     meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexBuffer.Length));
//     Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, targetMesh);

//     vNativeArray.Dispose();
//     tNativeArray.Dispose();
//   }

//   private void WindTriangles(Mesh targetMesh) {
//     int sideLength = (int) Mathf.Sqrt(targetMesh.vertices.Length) - 1;
//     int[] triangles = new int[sideLength * sideLength * 6];
//     int vert = 0;
//     int tris = 0;
//     for (int z = 0; z < sideLength; z++)
//     {
//       for (int x = 0; x < sideLength; x++)
//       {
//         triangles[tris] = vert;
//         triangles[tris + 1] = vert + sideLength + 1;
//         triangles[tris + 2] = vert + 1;
//         triangles[tris + 3] = vert + 1;
//         triangles[tris + 4] = vert + sideLength + 1;
//         triangles[tris + 5] = vert + sideLength + 2;
//         vert++;
//         tris += 6;
//       }
//       vert++;
//     }

//     targetMesh.triangles = triangles;
//   }

//   private int[] CullTriangles(Mesh targetMesh) {
//     int[] triangles = targetMesh.triangles;
//     int sideLength = (int) Mathf.Sqrt(targetMesh.vertices.Length) - 1;
//     int[] culled = new int[(sideLength * sideLength - (sideLength * 2 + (sideLength - 2) * 2)) * 6];
//     for (int i = 0, j = 0; i < sideLength * sideLength; i++) {
//       if (i / sideLength == sideLength - 1) continue;
//       if (i % sideLength == sideLength - 1) continue;
//       if (i / sideLength == 0) continue;
//       if (i % sideLength == 0) continue;
//       culled[j] = triangles[i * 6];
//       culled[j + 1] = triangles[i * 6 + 1];
//       culled[j + 2] = triangles[i * 6 + 2];
//       culled[j + 3] = triangles[i * 6 + 3];
//       culled[j + 4] = triangles[i * 6 + 4];
//       culled[j + 5] = triangles[i * 6 + 5];
//       j += 6;
//     }

//     return culled;
//   }

//   private struct BakeColliderJob : IJobParallelFor
//   {

//     [DeallocateOnJobCompletion] public NativeArray<int> meshIds;

//     public void Execute(int index)
//     {

//       Physics.BakeMesh(meshIds[index], false);

//     }

//   }

//   private JobHandle ExecuteBake(int[] indices) {
//     Mesh[] meshes = new Mesh[indices.Length];
//     for (int i = 0; i < indices.Length; i++) {
//       meshes[i] = _tilePool[indices[i]].mesh;
//     }

//     NativeArray<int> meshIds = new NativeArray<int>(meshes.Length, Allocator.Persistent);
//     for (int i = 0; i < meshes.Length; i++) {
//       meshIds[i] = meshes[i].GetInstanceID();
//     }

//     BakeColliderJob job = new BakeColliderJob {
//       meshIds = meshIds
//     };
    
//     JobHandle handle = job.Schedule(meshIds.Length, 1);
//     return handle;
//   }

//   private IEnumerator BakeColliders() {
//     _bakingColliders = true;
//     int[] indices = _frameColliderBakeBuffer.ToArray();
//     JobHandle jobHandle = ExecuteBake(indices);
//     while (!jobHandle.IsCompleted) {
//       yield return null;
//     }
//     jobHandle.Complete();
//     for (int i = 0; i < indices.Length; i++) {
//       if (!_tilePool[indices[i]].meshCollider) _tilePool[indices[i]].meshCollider = _tilePool[indices[i]].obj.AddComponent<MeshCollider>();
//       _tilePool[indices[i]].meshCollider.enabled = true;
//       _tilePool[indices[i]].meshCollider.sharedMesh = _tilePool[indices[i]].mesh;
//     }
//     _frameColliderBakeBuffer.Clear();
//     _bakingColliders = false;
//   }

//   private void UpdateCollider(int index) {
//     _frameColliderBakeBuffer.Add(index);
//   }

//   private void UpdateMesh(Mesh targetMesh, int index) {
//     targetMesh.normals = CalculateNormalsJobs(targetMesh);
//     //RiverPass(targetMesh, index);
//     //targetMesh.triangles = CullTriangles(targetMesh);
//     targetMesh.RecalculateBounds();
//   }

//   [BurstCompile]
//   private struct NormalJob : IJobParallelFor
//   {

//     [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<float3> vertices;
//     [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<int> triangles;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> normals;

//     public void Execute(int index) {
//       int vertexIndexA = triangles[index * 3];
//       int vertexIndexB = triangles[index * 3 + 1];
//       int vertexIndexC = triangles[index * 3 + 2];
//       float3 pointA = vertices[vertexIndexA];
//       float3 normal = math.cross(vertices[vertexIndexB] - pointA, vertices[vertexIndexC] - pointA);
//       normals[vertexIndexA] += normal;
//       normals[vertexIndexB] += normal;
//       normals[vertexIndexC] += normal;
//     }

//   }

//   private Vector3[] CalculateNormalsJobs(Mesh targetMesh) {
//     Vector3[] verts = targetMesh.vertices;
//     int[] tris = targetMesh.triangles;
//     NativeArray<float3> vertices = new NativeArray<float3>(verts.Length, Allocator.TempJob);
//     NativeArray<int> triangles = new NativeArray<int>(tris.Length, Allocator.TempJob);
//     NativeArray<float3> normals = new NativeArray<float3>(verts.Length, Allocator.TempJob);
//     for (int i = 0; i < verts.Length; i++) vertices[i] = verts[i];
//     for (int i = 0; i < tris.Length; i++) triangles[i] = tris[i];
//     NormalJob job = new NormalJob {
//       vertices = vertices,
//       triangles = triangles,
//       normals = normals
//     };
//     JobHandle handle = job.Schedule(tris.Length / 3, 64);
//     handle.Complete();
//     Vector3[] normalOut = new Vector3[verts.Length];
//     normalOut = normals.Reinterpret<Vector3>().ToArray();
//     for (int i = 0; i < normalOut.Length; i++) normalOut[i].Normalize();
//     vertices.Dispose();
//     triangles.Dispose();
//     normals.Dispose();
//     return normalOut;
//   }

//   #endregion

// }