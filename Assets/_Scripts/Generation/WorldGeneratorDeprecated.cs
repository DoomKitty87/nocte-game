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

// public class WorldGeneratorDeprecated : MonoBehaviour
// {

//     #region Structs

//     private struct WorldTile
//     {
//         public GameObject obj;
//         public Mesh mesh;
//         public MeshCollider meshCollider;
//         public float[] temperatureMap;
//         public float[] humidityMap;
//         public int x;
//         public int z;
//         public int[] grassIndexStart;
//         public int[] grassCount;
//         public int[] biomeData;
//         public int waterVertIndex;
//         public int waterVertCount;
//         public int waterTriIndex;
//         public int waterTriCount;
//         public int currentLOD;

//     }

//     [System.Serializable]
//     private struct LODLevel {
//       public int distance;
//       public int factor;
//     }
    
//     [System.Serializable]
//     public struct NoiseLayer
//     {

//         public string name;
//         [Tooltip("simplex or cellular - noise sampling used. Simplex looks like hills, cellular looks like hexagons.")]
//         [Space(10)]
//         public string noiseType;
//         [Tooltip("Height of noise layer in Unity units.")]
//         public float amplitude;
//         [Tooltip("Number of iterations of noise layered for final result.")]
//         public int octaves;
//         [Tooltip("Curve used to ease the noise function.")]
//         public AnimationCurve easeCurve;
//         [Tooltip("Easing based on noise from previous layers.")]
//         public AnimationCurve primaryEase;
//         [Tooltip("Size of noise - bigger means larger scale.")]
//         public float scaleX;
//         [Tooltip("Size of noise - bigger means larger scale.")]
//         public float scaleZ;
//         [Tooltip("Create more bumpy noise - enables sharpness.")]
//         public bool turbulent;
//         [Tooltip("Transition value between billow and ridge noise if turbulent (0-1).")]
//         public float sharpness;
//         [Tooltip("Scale change factor between octaves.")]
//         public float lacunarity;
//         [Tooltip("Significance of each octave.")]
//         public float persistence;
//         [Tooltip("Amplitude of domain warp.")]
//         public float warpStrength;
//         [Tooltip("Scale of domain warp noise.")]
//         public float warpSize;

//     }

//     [System.Serializable]
//     private struct AmalgamNoiseParams
//     {

//         [Tooltip("Iterations of noise layered for final result.")]
//         public int octaves;
//         [Tooltip("Scale change factor between octaves.")]
//         public float lacunarity;        
//         [Tooltip("Amplitude change factor between octaves.")]
//         public float persistence;        
//         [Tooltip("Scale of sharpness noise permutation.")]
//         public float sharpnessScale;        
//         [Tooltip("Amplitude of sharpness noise permutation.")]
//         public float sharpnessAmplitude;        
//         [Tooltip("Midpoint value of sharpness.")]
//         public float sharpnessMean;        
//         [Tooltip("Scale of scale noise permutation.")]
//         public float scaleScale;        
//         [Tooltip("Amplitude of scale noise permutation.")]
//         public float scaleAmplitude;        
//         [Tooltip("Midpoint value of scale.")]
//         public float scaleMean;
//         [Tooltip("Scale of amplitude noise permutation.")]
//         public float amplitudeScale;        
//         [Tooltip("Amplitude of amplitude noise permutation.")]
//         public float amplitudeAmplitude;         
//         [Tooltip("Midpoint value of amplitude.")]
//         public float amplitudeMean;       
//         [Tooltip("Scale of warp strength noise permutation.")]
//         public float warpStrengthScale;         
//         [Tooltip("Amplitude of warp strength noise permutation.")]
//         public float warpStrengthAmplitude;       
//         [Tooltip("Midpoint value of warp strength.")]
//         public float warpStrengthMean;        
//         [Tooltip("Scale of warp scale noise permutation.")]
//         public float warpScaleScale;      
//         [Tooltip("Amplitude of warp scale noise permutation.")]
//         public float warpScaleAmplitude;
//         [Tooltip("Midpoint value of warp scale.")]
//         public float warpScaleMean;        

//     }

//     [System.Serializable]
//     private struct RiverParams 
//     {

//         public float scale;
//         public float amplitude;
//         public int octaves;
//         public float lacunarity;
//         public float persistence;
//         public float warpScale;
//         public float warpStrength;
//         public float waterLevel;
//         public AnimationCurve noiseCurve;
//         public AnimationCurve heightCurve;
//         public AnimationCurve normalCurve;
//         public GameObject obj;

//     }

//     [System.Serializable]
//     public struct ScatterLayer
//     {

//         public string name;
//         public GameObject[] prefabs;
//         public float density;
//         public bool alignToNormal;
//         public float noiseScale;
//         public int noiseOctaves;
//         public float noiseWarpScale;
//         public float noiseWarpStrength;
//         public float jitterStrength;
//         public bool spawnsInWater;
//         public float waterThreshold;

//     }

//     [System.Serializable]
//     public struct Biome
//     {

//         public string name;
//         public NoiseLayer[] noiseLayers;
//         public ScatterLayer[] scatterLayers;

//     }
    
//     [System.Serializable]
//     public struct GrassLOD
//     {
        
//         public Mesh mesh;
//         public int distance;
//         public int density;

//     }
    
//     private struct GrassData
//     {

//         public Vector4 position;
//         public Vector2 uv;
//         public float displacement;

//     }
    
//     #endregion
    
//     #region Properties
    
//     public int _xSize = 64;
//     public int _zSize = 64;

//     public float _xResolution = 0.25f;
//     public float _zResolution = 0.25f;
    
//     public int _xTiles = 101;
//     public int _zTiles = 101;
//     [Space(5)]
//     public float _scale = 1000;
//     public float _biomeScale = 0.0001f;
    
//     //public float _amplitude = 50;
//     //public int _octaves = 10;
//     [SerializeField] private AmalgamNoiseParams _noiseParameters;
//     [SerializeField] private RiverParams _riverParameters;

//     public float _temperatureScale = 2;
//     public float _humidityScale = 1.5f;

//     public int _seed;
//     [Space(5)]
//     public int _colliderRange;
    
//     public Material _material;
//     public Gradient _colorGradient;
//     public bool _useColorGradient;
//     public AnimationCurve _easeCurve;
//     [Space(5)]
//     public float _maxUpdatesPerFrame = 5;

//     public bool _hasColliders;
    
//     public Material _material2;

//     public AnimationCurve _rockPassCurve;
//     public AnimationCurve _rockPassNoiseCurve;
//     public float _rockPassAmplitude = 1;
//     public float _rockPassScale = 2;
//     [Space(5)]
//     public float _cavePassScale = 100;
//     public float _cavePassAmplitude = 10;
//     public AnimationCurve _cavePassCurve;
//     public int _maxWaterRange;
//     public bool _limitWater;

//     private Mesh _waterMesh;

//     private List<Vector3> _waterVertices = new List<Vector3>();
//     private List<int> _waterTriangles = new List<int>();

//     [SerializeField] private bool _disableGrass;
    
//     [SerializeField] private GrassLOD[] _grassLODLevels;
//     [SerializeField] private LODLevel[] _terrainLODLevels;
//     [SerializeField] private int _baseLOD;
//     // [SerializeField] private Biome[] _biomes;
//     [SerializeField] private ScatterLayer[] _scatterLayers;
//     [SerializeField] private SecondaryStructures _structures;
//     [SerializeField] private PlaceStructures _storyStructures;

//     public int _scatterRange = 2;
//     // These are separated because density is used in calculations before biome is calculated

//     private WorldTile[] _tilePool;
//     private int[,] _tilePositions;
//     private List<Vector2> _generatedStructureTiles = new List<Vector2>();

//     private int _lastPlayerChunkX;
//     private int _lastPlayerChunkZ;
//     private int _playerChunkXWorld;
//     private int _playerChunkZWorld;
    
//     private List<int> _updateQueue = new List<int>();
//     private List<int[]> _generateQueue = new List<int[]>();
//     private float updatesLeft;

//     private Texture2D _wind;
    
//     GraphicsBuffer[] _commandBuf;
//     GraphicsBuffer.IndirectDrawIndexedArgs[][] _commandData;
//     private ComputeBuffer[] _positionsBuffer;
//     const int _commandCount = 2;
//     private List<GrassData>[] _grassData;
//     private RenderParams[] _rp;

//     private int[] _grassLODLookupDensityArray;
//     private int[] _grassLODLookupBufferArray;
//     private int[] _grassLODChunkCache;
    
//     private float _playerX, _playerZ;
//     private int _playerXChunkScale, _playerZChunkScale;

//     [SerializeField] private float _lakePlaneHeight = 350;
//     [SerializeField] private Transform _lakeObject;
    
//     private Vector2 _windPos;

//     private float _maxPossibleHeight;

//     [SerializeField] private bool _refreshButton = false;

//     private List<int> _frameColliderBakeBuffer = new List<int>();

//     [SerializeField] private MeshColliderCookingOptions _colliderCookingOptions;
    
//     #endregion

//     private void OnValidate() {
//         if (_refreshButton) {
//             _refreshButton = false;
//             Regenerate();
//         }
//     }

//     private float HashVector3ToFloat(Vector3 inputVector, int otherSeed)
//     {
//         int seed = _seed;

//         float psuedoRandomValue = ((Mathf.Sin((float)(inputVector.x * 234.24 * (_seed * otherSeed / 1000))) +
//                                    Mathf.Tan((float)(inputVector.y * 937.24 * (_seed * otherSeed / 950)) -
//                                              Mathf.Cos((float)(inputVector.z * 734.52 * (_seed * otherSeed / 1050))))) * 100000) % 1;
        
//         return psuedoRandomValue;
//     }

//     private void MakeGrassBuffers() {
//         int numberOfBuffers = _grassLODLevels.Length;
//         _grassData = new List<GrassData>[numberOfBuffers];
//         _positionsBuffer = new ComputeBuffer[numberOfBuffers];
//         _rp = new RenderParams[numberOfBuffers];
//         _commandBuf = new GraphicsBuffer[numberOfBuffers];
//         _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[numberOfBuffers][];
//     }
    
//     private void FillGrassArray() {
//         int numberOfChunks = _xTiles * _zTiles;
//         _grassLODChunkCache = new int[numberOfChunks];
//         for (int i = 0; i < _grassLODChunkCache.Length; i++) {
//             _grassLODChunkCache[i] = -1;
//         }
        
//         int numberOfElements = _grassLODLevels[^1].distance;
//         _grassLODLookupDensityArray = new int[numberOfElements];
//         _grassLODLookupBufferArray = new int[numberOfElements];
//         for (int i = _grassLODLevels.Length - 1; i >= 0; i--) {
//             for (int j = 0; j < _grassLODLevels[i].distance; j++) {
//                 _grassLODLookupDensityArray[j] = _grassLODLevels[i].density;
//                 _grassLODLookupBufferArray[j] = i;
//             }
//         }
//     }
         
//     public void UpdatePlayerLoadedChunks(Vector3 playerPos) {
//         if (_generateQueue.Count > 0) return;
//         // _material.SetVector("_PlayerPosition", playerPos);
//         int playerXChunkScale = Mathf.FloorToInt(playerPos.x / (_xSize * _xResolution));
//         int playerZChunkScale = Mathf.FloorToInt(playerPos.z / (_zSize * _zResolution));

//         _playerChunkXWorld = playerXChunkScale;
//         _playerChunkZWorld = playerZChunkScale;

//         if (playerXChunkScale - _lastPlayerChunkX > 1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX - 1;
//         if (playerZChunkScale - _lastPlayerChunkZ > 1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ - 1;
//         if (playerXChunkScale - _lastPlayerChunkX < -1) playerXChunkScale -= playerXChunkScale - _lastPlayerChunkX + 1;
//         if (playerZChunkScale - _lastPlayerChunkZ < -1) playerZChunkScale -= playerZChunkScale - _lastPlayerChunkZ + 1;

//         int deltaX = playerXChunkScale - _lastPlayerChunkX;
//         int deltaZ = playerZChunkScale - _lastPlayerChunkZ;
        
//         if (deltaX < 0) {
//             int[] tempValues = new int[_zTiles];
//             for (int i = 0; i < _zTiles; i++) {
//                 tempValues[i] = _tilePositions[_xTiles - 1, i];
//             }
            
//             for (int x = _xTiles - 1; x > 0; x--) {
//                 for (int z = 0; z < _zTiles; z++) {
//                     _tilePositions[x, z] = _tilePositions[x - 1, z];
//                 }
//             }
            
//             for (int i = 0; i < _zTiles; i++) {
//                 _tilePositions[0, i] = tempValues[i];
//             }
            
//             for (int i = 0; i < _zTiles; i++) {
//                 _tilePool[_tilePositions[0, i]].x = playerXChunkScale - (_xTiles - 1) / 2;
//                 _tilePool[_tilePositions[0, i]].z = i + playerZChunkScale - (_zTiles - 1) / 2;
//                 QueueTileUpdate(_tilePositions[0, i]);
//             }
//         }
//         else if (deltaX > 0) {
//             int[] tempValues = new int[_zTiles];
//             for (int i = 0; i < _zTiles; i++) {
//                 tempValues[i] = _tilePositions[0, i];
//             }
            
//             for (int x = 0; x < _xTiles - 1; x++) {
//                 for (int z = 0; z < _zTiles; z++) {
//                     _tilePositions[x, z] = _tilePositions[x + 1, z];
//                 }
//             }
            
//             for (int i = 0; i < _zTiles; i++) {
//                 _tilePositions[_xTiles - 1, i] = tempValues[i];
//             }
            
//             for (int i = 0; i < _zTiles; i++) {
//                 _tilePool[_tilePositions[_xTiles - 1, i]].x = playerXChunkScale + (_xTiles - 1) / 2;
//                 _tilePool[_tilePositions[_xTiles - 1, i]].z = i + playerZChunkScale - (_zTiles - 1) / 2;
//                 QueueTileUpdate(_tilePositions[_xTiles - 1, i]);
//             }
//         }

//         if (deltaZ < 0) {
//             int[] tempValues = new int[_xTiles];
//             for (int i = 0; i < _xTiles; i++) {
//                 tempValues[i] = _tilePositions[i, _zTiles - 1];
//             }
            
//             for (int x = 0; x < _xTiles; x++) {
//                 for (int z = _zTiles - 1; z > 0; z--) {
//                     _tilePositions[x, z] = _tilePositions[x, z - 1];
//                 }
//             }
            
//             for (int i = 0; i < _xTiles; i++) {
//                 _tilePositions[i, 0] = tempValues[i];
//             }
            
//             for (int i = 0; i < _xTiles; i++) {
//                 _tilePool[_tilePositions[i, 0]].x = i + playerXChunkScale - (_xTiles - 1) / 2;
//                 _tilePool[_tilePositions[i, 0]].z = playerZChunkScale - (_zTiles - 1) / 2;
//                 QueueTileUpdate(_tilePositions[i, 0]);
//             }
//         }
//         else if (deltaZ > 0) {
//             int[] tempValues = new int[_xTiles];
//             for (int i = 0; i < _xTiles; i++) {
//                 tempValues[i] = _tilePositions[i, 0];
//             }
            
//             for (int x = 0; x < _xTiles; x++) {
//                 for (int z = 0; z < _zTiles - 1; z++) {
//                     _tilePositions[x, z] = _tilePositions[x, z + 1];
//                 }
//             }
            
//             for (int i = 0; i < _xTiles; i++) {
//                 _tilePositions[i, _zTiles - 1] = tempValues[i];
//             }
//             for (int i = 0; i < _xTiles; i++) {
//                 _tilePool[_tilePositions[i, _zTiles - 1]].x = i + playerXChunkScale - (_xTiles - 1) / 2;
//                 _tilePool[_tilePositions[i, _zTiles - 1]].z = playerZChunkScale + (_zTiles - 1) / 2;
//                 QueueTileUpdate(_tilePositions[i, _zTiles - 1]);
//             }
//         }

//         if (deltaZ != 0 || deltaX != 0) {
//             _structures.CheckStructures(new Vector2(playerPos.x, playerPos.z));
//             _storyStructures.CheckStructures(new Vector2(playerPos.x, playerPos.z));
//             _updateQueue.Sort((c1, c2) => (Mathf.Abs(_tilePool[c1].x - _playerChunkXWorld) + Mathf.Abs(_tilePool[c1].z - _playerChunkZWorld)).CompareTo(Mathf.Abs(_tilePool[c2].x - _playerChunkXWorld) + Mathf.Abs(_tilePool[c2].z - _playerChunkZWorld)));
//             for (int x = 0; x < _xTiles; x++) {
//                 for (int z = 0; z < _zTiles; z++) { 
//                 float maxDistance = Mathf.Max(Mathf.Abs(_tilePool[_tilePositions[x, z]].x - playerXChunkScale), Mathf.Abs(_tilePool[_tilePositions[x, z]].z - playerZChunkScale));
//                 if (_hasColliders && maxDistance < _colliderRange) { 
//                     if (_tilePool[_tilePositions[x, z]].meshCollider == null || !_tilePool[_tilePositions[x, z]].meshCollider.enabled) UpdateCollider(_tilePositions[x, z]);
//                 } else if (_hasColliders) {
//                 if (_tilePool[_tilePositions[x, z]].meshCollider) _tilePool[_tilePositions[x, z]].meshCollider.enabled = false;
//                 }
//                 if (!_disableGrass) GenerateGrassBasedOffLODs(x, z, Mathf.FloorToInt(maxDistance));
//                 if (GetLOD(new Vector2(_tilePool[_tilePositions[x, z]].x, _tilePool[_tilePositions[x, z]].z), new Vector2(playerXChunkScale, playerZChunkScale)) != _tilePool[_tilePositions[x, z]].currentLOD) {
//                     _tilePool[_tilePositions[x, z]].currentLOD = GetLOD(new Vector2(_tilePool[_tilePositions[x, z]].x, _tilePool[_tilePositions[x, z]].z), new Vector2(playerXChunkScale, playerZChunkScale));
//                     QueueTileUpdate(_tilePositions[x, z]);
//                     continue;
//                 }
//                 if (_tilePool[_tilePositions[x, z]].waterVertCount == 0 && _limitWater && maxDistance <= _maxWaterRange) QueueTileUpdate(_tilePositions[x, z]);
//                 else if (_tilePool[_tilePositions[x, z]].waterVertCount != 0 && _limitWater && maxDistance > _maxWaterRange) QueueTileUpdate(_tilePositions[x, z]);
//                 }
//             }
//         }
        
//         _lastPlayerChunkX = playerXChunkScale;
//         _lastPlayerChunkZ = playerZChunkScale;
//         if (!_disableGrass) {
//             for (int i = 0; i < _rp.Length; i++) {
//                 _rp[i].matProps.SetVector("_PlayerPosition", playerPos);
//             }
//         }
//     }
    
//     private void GenerateGrass(int index, int density, int bufferID) {
//         List<GrassData> buffer = _grassData[bufferID];
//         if (_tilePool[index].grassCount[bufferID] > 0) return;
//         Vector3[] vertexData = _tilePool[index].mesh.vertices;
// 		int[] triangles = _tilePool[index].mesh.triangles;
// 		Vector3[] normals = new Vector3[triangles.Length / 3];

// 		triangles[^1] = 0; // Fixes bug with last triangle vertex being incorrect

// 		// Calculate normals of triangles
// 		for (int i = 0; i < normals.Length; i++) {
// 			normals[i] = Vector3.Cross(vertexData[triangles[i * 3 + 1]] - vertexData[triangles[i * 3]], vertexData[triangles[i * 3 + 2]] - vertexData[triangles[i * 3]]).normalized;
// 		}

// 		_tilePool[index].grassIndexStart[bufferID] = buffer.Count;
        
//         // TODO:
//         // Increase performance by converting to compute shader
//         for (int i = 0; i < triangles.Length / 3; i++) {
//             if (normals[i].y < 0.7f) continue;
//             for (int j = 0; j < density; j++) {
//                 GrassData gd = new GrassData();
                
//                 float r1 = UnityEngine.Random.Range(0f, 1f); // HashVector3ToFloat(vertexData[triangles[i]], j);
//                 float r2 = UnityEngine.Random.Range(0f, 1f); // HashVector3ToFloat(vertexData[triangles[i]], j + 1);
                
//                 // Corrects for values
//                 if (r1 + r2 > 1) {
//                     r1 = 1 - r1;
//                     r2 = 1 - r2;
//                 }
//                 float r3 = 1 - r1 - r2;
//                 // Randomly pick points between the vertices of the triangle
//                 Vector3 randomPosition = r1 * vertexData[triangles[i * 3]]
//                                          + r2 * vertexData[triangles[i * 3 + 1]]
//                                          + r3 * vertexData[triangles[i * 3 + 2]];

//                 // Vector3 randomPosition = 
//                 //     (float)(1 - Math.Sqrt(r1)) * vertexData[triangles[i * 3]] 
//                 //     + (float)(Math.Sqrt(r1) * (1 - r2)) * vertexData[triangles[i * 3 + 1]]
//                 //     + (float)(r2 * Math.Sqrt(r1)) * vertexData[triangles[i * 3 + 2]];
//                 gd.position = new Vector4(
// 					randomPosition.x + (_tilePool[index].x * _xSize * _xResolution),
// 				    randomPosition.y, 
//                     randomPosition.z + (_tilePool[index].z * _zSize * _zResolution), 
//                     0);
//                 gd.uv = new Vector2(gd.position.x / (_xSize * _xResolution * _xTiles), gd.position.z / (_zSize * _zResolution * _zTiles));

//                 buffer.Add(gd);
//             }
//         }
        
//         _tilePool[index].grassCount[bufferID] = buffer.Count - _tilePool[index].grassIndexStart[bufferID];
//     }
    
//     private void GenerateGrassBasedOffLODs(int x, int z, int maxDistance) {
        
//         int currentChunkSetting;
//         int previousChunkSetting = _grassLODChunkCache[_tilePositions[x, z]];
//         if (maxDistance >= _grassLODLevels[^1].distance) currentChunkSetting = -1;
//         else currentChunkSetting = _grassLODLookupBufferArray[maxDistance];
        
//         if (currentChunkSetting != previousChunkSetting) {
//             if (previousChunkSetting != -1) {
//                     _grassData[previousChunkSetting].RemoveRange(
//                         _tilePool[_tilePositions[x, z]].grassIndexStart[previousChunkSetting],
//                         _tilePool[_tilePositions[x, z]].grassCount[previousChunkSetting]);
//                     for (int j = 0; j < _xTiles * _zTiles; j++) {
//                         if (_tilePool[j].grassIndexStart[previousChunkSetting] > _tilePool[_tilePositions[x, z]].grassIndexStart[previousChunkSetting])
//                             _tilePool[j].grassIndexStart[previousChunkSetting] -= _tilePool[_tilePositions[x, z]].grassCount[previousChunkSetting];
//                     }
            
//                 _tilePool[_tilePositions[x, z]].grassCount[previousChunkSetting] = 0;
//                 _tilePool[_tilePositions[x, z]].grassIndexStart[previousChunkSetting] = 0;
//             }

//             if (currentChunkSetting != -1) {
//                     GenerateGrass(_tilePositions[x, z], _grassLODLookupDensityArray[maxDistance], currentChunkSetting);
//             }

//             _grassLODChunkCache[_tilePositions[x, z]] = currentChunkSetting;
//         }
//     }

//     public float GetHeightValue(Vector2 worldPosition) {
//         float heightVal = AmalgamNoise.GenerateTerrain(0, 1, worldPosition.x + _seed, worldPosition.y + _seed, 0, 0, _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
//             _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
//             _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
//             _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
//             _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean)[0].y;
//         heightVal -= _riverParameters.heightCurve.Evaluate(heightVal / _maxPossibleHeight) * (_riverParameters.noiseCurve.Evaluate(AmalgamNoise.GenerateRivers(
//             0, 1, worldPosition.x + _seed % 216812, worldPosition.y + _seed % 216812, 0, 0, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength)[0]) * _riverParameters.amplitude);
//         return heightVal;
//     }

//     private float GetRiverValue(Vector2 worldPosition) {
//       return _riverParameters.heightCurve.Evaluate(worldPosition.y / _maxPossibleHeight) * _riverParameters.noiseCurve.Evaluate(AmalgamNoise.GenerateRivers(
//           0, 1, worldPosition.x + _seed % 216812, worldPosition.y + _seed % 216812, 0, 0, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength)[0]);
//     }

//     public float GetSeedHash() {
//         return _seed;
//     }

//     private void Awake() {
//         _maxPossibleHeight = _noiseParameters.amplitudeMean + _noiseParameters.amplitudeAmplitude;
//         if (!_disableGrass) {
//             MakeGrassBuffers();
//             FillGrassArray();
//         }
//         _waterMesh = new Mesh();
//         _waterMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//         _riverParameters.obj.GetComponent<WaterSurface>().mesh = _waterMesh;
//         _seed = int.Parse(Hash128.Compute(_seed).ToString().Substring(0, 6), System.Globalization.NumberStyles.HexNumber);
//         _scale = 1 / _scale;
//         _lakeObject.position = new Vector3(0, _lakePlaneHeight, 0);
//     }
    
//     private void Start() {
//         if (!_disableGrass) SetupGrass();
//         SetupPool();
//     }

//     public void Regenerate() {
//         // Debug.Log("Regenerating Terrain");
//         for (int i = 0; i < _tilePool.Length; i++) {
//             Destroy(_tilePool[i].obj);
//         }
//         SetupPool();
//     }

//     private void Update() {
//         while (updatesLeft > 0) {
//             if (_updateQueue.Count > 0 && _generateQueue.Count == 0) {
//                 UpdateTile(_updateQueue[0]);
//                 _updateQueue.RemoveAt(0);
//                 if (_updateQueue.Count == 0) UpdateWaterMesh();
//                 updatesLeft--;
//             } else break;
//         }
//         for (int i = 0; i < _maxUpdatesPerFrame * 20; i++) {
//             if (_generateQueue.Count > 0) {
//                 GenerateTile(_generateQueue[0][0], _generateQueue[0][1], _generateQueue[0][2]);
//                 _generateQueue.RemoveAt(0);
//                 if (Time.timeScale == 0f) Time.timeScale = 1f;
//             } else break;
//         }
//         updatesLeft += _maxUpdatesPerFrame;
//         updatesLeft = Mathf.Min(updatesLeft, 1);
//         //UpdateWind();
//         //for (int i = 0; i < _positionsBuffer.Length; i++) {
//             //Graphics.RenderMeshIndirect(_rp[i], _grassLODLevels[i].mesh, _commandBuf[i], _commandCount);
//         //}
//         if (_frameColliderBakeBuffer.Count > 0) {
//             BakeColliders();
//         }
//     }
    
//     private void UpdateWind() {
//         _windPos += new Vector2(Time.deltaTime, Time.deltaTime);
//         float[] windData = NoiseMaps.GenerateWindMap(128, 128, _windPos.x, _windPos.y, 0.1f);
//         for (int i = 0; i < windData.Length; i++) {
//             _wind.SetPixel(i % 128, i / 128, new Color(windData[i], windData[i], windData[i], 1));
//         }
//         _wind.Apply();
//     }

//     public (Vector3[][], Vector2[]) GetVertices(int distance) {
//         int included = 0;
//         for (int i = 0, j = 0; i < _xTiles * _zTiles; i++) {
//             if (Mathf.Sqrt(Mathf.Pow(_tilePool[i].x, 2) + Mathf.Pow(_tilePool[i].z, 2)) <= distance) included++;
//         }
//         Vector3[][] vertices = new Vector3[included][];
//         Vector2[] positions = new Vector2[included];
//         for (int i = 0, j = 0; i < _xTiles * _zTiles; i++) {
//             if (Mathf.Sqrt(Mathf.Pow(_tilePool[i].x, 2) + Mathf.Pow(_tilePool[i].z, 2)) <= distance) {
//                 vertices[j] = _tilePool[i].mesh.vertices;
//                 positions[j] = new Vector2(_tilePool[i].obj.transform.position.x, _tilePool[i].obj.transform.position.z);
//                 j++;
//             }
//         }

//         return (vertices, positions);
//     }
    
//     private void OnDestroy() {
//         if (_disableGrass) return;
//         for (int i = 0; i < _positionsBuffer.Length; i++) {
//             _commandBuf[i]?.Release();
//             _positionsBuffer[i]?.Release();
//             _positionsBuffer[i] = null;

//         }
//         _commandBuf = null;
//         _positionsBuffer = null;
//     }
    
//     private void SetupPool() {
//         Time.timeScale = 0f;
//         _tilePool = new WorldTile[_xTiles * _zTiles];
//         _tilePositions = new int[_xTiles, _zTiles];
//         for (int x = -(_xTiles - 1) / 2, i = 0; x <= (_xTiles - 1) / 2; x++)
//         {
//             for (int z = -(_zTiles - 1) / 2; z <= (_zTiles - 1) / 2; z++) {
//                 QueueTileGen(x, z, i);
//                 i++;
//             }
//         }
//         _generateQueue.Sort((c1, c2) => (Mathf.Abs(c1[0]) + Mathf.Abs(c1[1])).CompareTo(Mathf.Abs(c2[0]) + Mathf.Abs(c2[1])));
//     }

//     private void SetupGrass() {
//         for (int i = 0; i < _grassData.Length; i++) {
//             _commandBuf[i] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, _commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
//             _commandData[i] = new GraphicsBuffer.IndirectDrawIndexedArgs[_commandCount];
//             _grassData[i] = new List<GrassData>();
//             _positionsBuffer[i] = new ComputeBuffer((_xSize + 2) * (_zSize + 2) * _xTiles * _zTiles, sizeof(float) * 7, ComputeBufferType.Default);
//             _rp[i] = new RenderParams(_material2);
//             _rp[i].matProps = new MaterialPropertyBlock();
//             _wind = new Texture2D(128, 128);
//             _rp[i].matProps.SetTexture("_Wind", _wind);
//             _rp[i].matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(0, 0, 0)));
//             _rp[i].matProps.SetBuffer("_PositionsBuffer", _positionsBuffer[i]);
//         }
//     }

//     private int GetLOD(Vector2 playerChunkCoords, Vector2 chunkCoords) {
//         int lod = _baseLOD;
        
//         for (int i = 0; i < _terrainLODLevels.Length; i++) {
//             if (Mathf.Max(Mathf.Abs(playerChunkCoords.x - chunkCoords.x), Mathf.Abs(playerChunkCoords.y - chunkCoords.y)) < _terrainLODLevels[i].distance) {
//             lod = _terrainLODLevels[i].factor;
//             break;
//             }
//         }

//         return lod;
//     }

//     private bool GetLODEdge(Vector2 playerChunkCoords, Vector2 chunkCoords) {
//         bool edge = false;
//         for (int i = 0; i < _terrainLODLevels.Length; i++) {
//             if (Mathf.Max(Mathf.Abs(playerChunkCoords.x - chunkCoords.x), Mathf.Abs(playerChunkCoords.y - chunkCoords.y)) == _terrainLODLevels[i].distance) {
//                 edge = true;
//                 break;
//             }
//         }
//         return edge;
//     }

//     private void GenerateTile(int x, int z, int index)
//     {
//         GameObject go = new GameObject("Tile");
//         go.transform.parent = transform;
//         MeshFilter mf = go.AddComponent<MeshFilter>();
//         MeshRenderer mr = go.AddComponent<MeshRenderer>();
//         mr.material = _material;
//         mf.mesh = new Mesh();
//         Mesh msh = mf.mesh;
//         WorldTile tile = new WorldTile();
//         int seed = _seed;
//         int lodFactor = GetLOD(new Vector2(_playerChunkXWorld, _playerChunkZWorld), new Vector2(x, z));
//         tile.currentLOD = lodFactor;
//         lodFactor = (int) Mathf.Pow(2, lodFactor);
//         if (_xSize * lodFactor * _zSize * lodFactor > 65000) {
//             msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//         }
//         // bool rendered = Mathf.Sqrt(x * x + z * z) <= (_xTiles - 1) / 2;
        
//         //var result = NoiseMaps.GenerateTerrainBiomes(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _biomes, _biomeScale, _xResolution, _zResolution);
//         Vector3[] result = AmalgamNoise.GenerateTerrain(_xSize, lodFactor, x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xResolution / lodFactor, _zResolution / lodFactor,
//             _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
//             _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
//             _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
//             _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
//             _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean);
//         msh.vertices = result;
        
//         //float[] temperatureMap = NoiseMaps.GenerateTemperatureMap(result, x * _xSize * _xResolution + (seed * 2), z * _zSize * _zResolution + (seed * 2), _xSize * lodFactor, _zSize * lodFactor, _scale / _temperatureScale, _easeCurve, _xResolution / lodFactor, _zResolution / lodFactor);
//         //float[] humidityMap = NoiseMaps.GenerateHumidityMap(result, temperatureMap, x * _xSize * _xResolution + (seed * 0.5f), z * _zSize * _zResolution + (seed * 0.5f), _xSize * lodFactor, _zSize * lodFactor, _scale / _humidityScale, _easeCurve, _xResolution / lodFactor, _zResolution / lodFactor);
//         go.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
//         go.isStatic = true;
        
//         // If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
//         go.tag = "Ground";
//         go.layer = 6;
//         _structures.GenerateChunkStructures(new Vector2(x * _xSize * _xResolution, z * _zSize * _zResolution), new Vector2((x + 1) * _xSize * _xResolution, (z + 1) * _zSize * _zResolution));
//         _generatedStructureTiles.Add(new Vector2(x, z));
//         tile.mesh = msh;
//         tile.obj = go;
//         // if (!rendered) go.SetActive(false);
//         // else go.SetActive(true);
//         //tile.temperatureMap = temperatureMap;
//         //tile.humidityMap = humidityMap;
//         tile.x = x;
//         tile.z = z;
//         _tilePool[index] = tile;
//         _tilePositions[index / _zTiles, index % _zTiles] = index;

//         if (!_disableGrass) {
//             tile.grassCount = new int[_grassLODLevels[^1].distance];
//             tile.grassIndexStart = new int[_grassLODLevels[^1].distance];
//         }

//         WindTriangles(msh, index);
//         //tile.biomeData = result.Item2;
//         UpdateMesh(msh, index);
//         float maxDistance = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
//         if (_hasColliders && maxDistance <= _colliderRange) {
//             UpdateCollider(index);
//         }
//         //if (!_useColorGradient) CalculateUVs(msh);
//         //else CalculateColors(index);
//         if (!_disableGrass) {
//             if (index == (_xTiles * _zTiles) - 1) {
//                 UpdateGrassBuffers();
                
//             }
//         }
//         if (index == (_xTiles * _zTiles) - 1) UpdateWaterMesh();
//         if (maxDistance <= _scatterRange) {
//             ScatterTile(index);
//         }
//     }

//     private void QueueTileUpdate(int index) {
//         _updateQueue.Add(index);
//     }

//     private void QueueTileGen(int x, int z, int index) {
//         _generateQueue.Add(new int[] {
//             x, z, index
//         }); 
//     }
    
//     //Regenerate given tile based on an LOD parameter.
//     private void UpdateTile(int index) {
//         int x = _tilePool[index].x;
//         int z = _tilePool[index].z;
//         int seed = _seed;
//         // var result = NoiseMaps.GenerateTerrainBiomes(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _biomes, _biomeScale, _xResolution, _zResolution);
//         int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//         if (_xSize * lodFactor * _zSize * lodFactor > 65000) {
//             _tilePool[index].mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//         }
//         float maxDistance = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x - _playerXChunkScale), 2) + Mathf.Pow(Mathf.Abs(z - _playerZChunkScale), 2));
//         // bool rendered = maxDistance <= (_xTiles - 1) / 2;
//         // if (!rendered) _tilePool[index].obj.SetActive(false);
//         // else _tilePool[index].obj.SetActive(true);
//         Vector3[] result = AmalgamNoise.GenerateTerrain(_xSize, lodFactor, x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xResolution / lodFactor, _zResolution / lodFactor,
//             _noiseParameters.octaves, _noiseParameters.lacunarity, _noiseParameters.persistence, _noiseParameters.sharpnessScale,
//             _noiseParameters.sharpnessAmplitude, _noiseParameters.sharpnessMean, _noiseParameters.scaleScale, _noiseParameters.scaleAmplitude,
//             _noiseParameters.scaleMean, _noiseParameters.amplitudeScale, _noiseParameters.amplitudeAmplitude, _noiseParameters.amplitudeMean,
//             _noiseParameters.warpStrengthScale, _noiseParameters.warpStrengthAmplitude, _noiseParameters.warpStrengthMean,
//             _noiseParameters.warpScaleScale, _noiseParameters.warpScaleAmplitude, _noiseParameters.warpScaleMean);
//         _tilePool[index].mesh.triangles = null;
//         _tilePool[index].mesh.vertices = result;
//         //_tilePool[index].temperatureMap = NoiseMaps.GenerateTemperatureMap(_tilePool[index].mesh.vertices, x * _xSize * _xResolution + (seed * 2), z * _zSize * _zResolution + (seed * 2), _xSize * lodFactor, _zSize * lodFactor, _scale / _temperatureScale, _easeCurve, _xResolution / lodFactor, _zResolution / lodFactor);
//         //_tilePool[index].humidityMap = NoiseMaps.GenerateHumidityMap(_tilePool[index].mesh.vertices, _tilePool[index].temperatureMap, x * _xSize * _xResolution + (seed * 0.5f), z * _zSize * _zResolution + (seed * 0.5f), _xSize * lodFactor, _zSize * lodFactor, _scale / _humidityScale, _easeCurve, _xResolution / lodFactor, _zResolution / lodFactor);

//         WindTriangles(_tilePool[index].mesh, index);
//         //if (!_useColorGradient) CalculateUVs(_tilePool[index].mesh);
//         //else CalculateColors(index);
//         _tilePool[index].obj.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
//         if (!_generatedStructureTiles.Contains(new Vector2(x, z))) {
//             _structures.GenerateChunkStructures(new Vector2(x * _xSize * _xResolution, z * _zSize * _zResolution), new Vector2((x + 1) * _xSize * _xResolution, (z + 1) * _zSize * _zResolution));
//             _generatedStructureTiles.Add(new Vector2(x, z));
//         }
//         UpdateMesh(_tilePool[index].mesh, index);
//         maxDistance = Mathf.Max(Mathf.Abs(x - _lastPlayerChunkX), Mathf.Abs(z - _lastPlayerChunkZ));
//         if (maxDistance <= _colliderRange) UpdateCollider(index);
//         else if (_tilePool[index].meshCollider) _tilePool[index].meshCollider.enabled = false;

//         if (maxDistance <= _scatterRange) {
//             ScatterTile(index);
//         } else {
//             ClearScatter(index);
//         }
//         if (_updateQueue.Count <= 1 && !_disableGrass) UpdateGrassBuffers();
//     }

//     private void UpdateWaterMesh() {
//         _waterMesh.triangles = null;
//         _waterMesh.vertices = _waterVertices.ToArray();
//         _waterMesh.triangles = _waterTriangles.ToArray();
//     }
    
//     private void UpdateGrassBuffers() {
//         for (int i = 0; i < _grassData.Length; i++) {
//             _rp[i].worldBounds = new Bounds(new Vector3(_playerX, 0, _playerZ),
//                 _xTiles * _xSize * _xResolution * Vector3.one); // use tighter bounds for better FOV culling
//             _commandData[i][0].indexCountPerInstance = _grassLODLevels[i].mesh.GetIndexCount(0);
//             _commandData[i][0].instanceCount = (uint)_grassData[i].Count;
//             _commandData[i][1].indexCountPerInstance = _grassLODLevels[i].mesh.GetIndexCount(0);
//             _commandData[i][1].instanceCount = (uint)_grassData[i].Count;
//             _commandBuf[i].SetData(_commandData[i]);
//             _positionsBuffer[i].SetData(_grassData[i]);
//         }
//     }

//     private void WindTriangles(Mesh targetMesh, int index) {
//         int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//         int sideLength = (int) Mathf.Sqrt(targetMesh.vertices.Length) - 1;
//         int[] triangles = new int[sideLength * sideLength * 6];
//         int vert = 0;
//         int tris = 0;
//         for (int z = 0; z < sideLength; z++)
//         {
//             for (int x = 0; x < sideLength; x++)
//             {
//                 triangles[tris] = vert;
//                 triangles[tris + 1] = vert + sideLength + 1;
//                 triangles[tris + 2] = vert + 1;
//                 triangles[tris + 3] = vert + 1;
//                 triangles[tris + 4] = vert + sideLength + 1;
//                 triangles[tris + 5] = vert + sideLength + 2;
//                 vert++;
//                 tris += 6;
//             }
//             vert++;
//         }

//         targetMesh.triangles = triangles;
//     }

//     private void CalculateUVs(Mesh targetMesh, int index) {
//         int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//         int sideLength = (int) Mathf.Sqrt(targetMesh.vertices.Length) - 1;

//         Vector2[] uvs = new Vector2[sideLength * sideLength];

//         Vector3[] vertices = targetMesh.vertices;
//         for (int i = 0; i < vertices.Length; i++) {
//             uvs[i] = new Vector2(vertices[i].x / (sideLength * _xResolution / lodFactor), vertices[i].z / (sideLength * _zResolution / lodFactor));
//         }

//         targetMesh.uv = uvs;
//     }

//     private void CalculateColors(int index) {
//         Color[] colors = new Color[_tilePool[index].temperatureMap.Length];
//         for (int i = 0; i < _tilePool[index].temperatureMap.Length; i++) {
//             colors[i] = _colorGradient.Evaluate(_tilePool[index].temperatureMap[i]);
//         }

//         _tilePool[index].mesh.colors = colors;
//     }

//     private Vector3[] CalculateNormals(Mesh targetMesh, int index) {
//         Vector3[] vertices = targetMesh.vertices;
//         int[] triangles = targetMesh.triangles;
//         Vector3[] normals = new Vector3[vertices.Length];
//         int triangleCount = triangles.Length / 3;
//         // int sideLength = (int) Mathf.Sqrt(vertices.Length);

//         for (int i = 0; i < triangleCount; i++) {
//             int normalTriangleIndex = i * 3;
//             int vertexIndexA = triangles[normalTriangleIndex];
//             int vertexIndexB = triangles[normalTriangleIndex + 1];
//             int vertexIndexC = triangles[normalTriangleIndex + 2];

//             Vector3 pointA = vertices[vertexIndexA];
//             Vector3 pointB = vertices[vertexIndexB];
//             Vector3 pointC = vertices[vertexIndexC];
//             Vector3 sideAB = pointB - pointA;
//             Vector3 sideAC = pointC - pointA;
//             Vector3 normal = Vector3.Cross(sideAB, sideAC).normalized;
//             normals[vertexIndexA] += normal;
//             normals[vertexIndexB] += normal;
//             normals[vertexIndexC] += normal;
//         }

//         for (int i = 0; i < normals.Length; i++) {
//             normals[i].Normalize();
//         }

//         return normals;
//     }

//     private int[] CullTriangles(Mesh targetMesh, int index) {
//         int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//         int[] triangles = targetMesh.triangles;
//         int sideLength = (int) Mathf.Sqrt(targetMesh.vertices.Length) - 1;
//         int[] culled = new int[(sideLength * sideLength - (sideLength * 2 + (sideLength - 2) * 2)) * 6];
//         for (int i = 0, j = 0; i < sideLength * sideLength; i++) {
//             if (i / sideLength == sideLength - 1) continue;
//             if (i % sideLength == sideLength - 1) continue;
//             if (i / sideLength == 0) continue;
//             if (i % sideLength == 0) continue;
//             culled[j] = triangles[i * 6];
//             culled[j + 1] = triangles[i * 6 + 1];
//             culled[j + 2] = triangles[i * 6 + 2];
//             culled[j + 3] = triangles[i * 6 + 3];
//             culled[j + 4] = triangles[i * 6 + 4];
//             culled[j + 5] = triangles[i * 6 + 5];
//             j += 6;
//         }
//         // Additional is for subdivision (not used)
//         //List<int> additional = new List<int>();
//         //for (int i = (sideLength + 2) * (sideLength + 2) * 6; i < triangles.Length; i++) {
//         //    additional.Add(triangles[i]);
//         //}

//         //culled.AddRange(additional);
//         return culled;
//     }

//     private struct BakeColliderJob : IJobParallelFor
//     {

//       public NativeArray<int> meshIds;

//       public void Execute(int index)
//       {

//         Physics.BakeMesh(meshIds[index], false);

//       }

//     }

//     private void BakeMeshColliders(int[] indices) {
//       Mesh[] meshes = new Mesh[indices.Length];
//       for (int i = 0; i < indices.Length; i++) {
//         meshes[i] = _tilePool[indices[i]].mesh;
//       }

//       NativeArray<int> meshIds = new NativeArray<int>(meshes.Length, Allocator.TempJob);
//       for (int i = 0; i < meshes.Length; i++) {
//         meshIds[i] = meshes[i].GetInstanceID();
//       }

//       BakeColliderJob job = new BakeColliderJob {
//           meshIds = meshIds
//       };
      
//       JobHandle handle = job.Schedule(meshIds.Length, 1);
//       handle.Complete();
//       meshIds.Dispose();

//     }

//     private void BakeColliders() {
//       int[] indices = _frameColliderBakeBuffer.ToArray();
//       BakeMeshColliders(indices);
//       for (int i = 0; i < indices.Length; i++) {
//         if (!_tilePool[indices[i]].meshCollider) _tilePool[indices[i]].meshCollider = _tilePool[indices[i]].obj.AddComponent<MeshCollider>();
//         _tilePool[indices[i]].meshCollider.enabled = true;
//         _tilePool[indices[i]].meshCollider.sharedMesh = _tilePool[indices[i]].mesh;
//       }
//       _frameColliderBakeBuffer.Clear();
//     }

//     private void UpdateCollider(int index) {
//         if (_tilePool[index].meshCollider)
//             _tilePool[index].meshCollider.cookingOptions = _colliderCookingOptions;
//         _frameColliderBakeBuffer.Add(index);
//     }

//     private Vector3 CalculateAbsVertex(Vector3 vertex, int index) {
//         Vector3 vertexAbs = new Vector3(vertex.x * _xResolution + _tilePool[index].x * _xSize * _xResolution, vertex.y, vertex.z * _zResolution + _tilePool[index].z * _zSize * _zResolution);
//         return vertexAbs;
//     }

//     private void ClearScatter(int index) {
//         for (int i = _tilePool[index].obj.transform.childCount - 1; i >= 0; i--) Destroy(_tilePool[index].obj.transform.GetChild(i).gameObject);
//     }

//     private void ScatterTile(int index) {
//         for (int i = _tilePool[index].obj.transform.childCount - 1; i >= 0; i--) Destroy(_tilePool[index].obj.transform.GetChild(i).gameObject);
//         for (int i = 0; i < _scatterLayers.Length; i++) {
//             ScatterObjects(index, i);
//         }
//     }
    
//     private void ScatterObjects(int index, int layer) {
//         Mesh targetMesh = _tilePool[index].mesh;
//         Vector3[] vertices = targetMesh.vertices;
//         Vector3[] normals = targetMesh.normals;
//         int sideLength = (int) Mathf.Sqrt(vertices.Length);
//         // List<Vector2> points = PoissonDisk.GeneratePoints(1 / _scatterLayers[layer].density * Mathf.Pow(2, _tilePool[index].currentLOD), new Vector2(sideLength, sideLength), 30, (int) vertices[0].y + _seed);
//         float lodFactor = Mathf.Pow(2, _tilePool[index].currentLOD);
//         int[] points = ScatterNoise.ScatterOrganic(sideLength, _scatterLayers[layer].density, _tilePool[index].x * _xResolution * _xSize, _tilePool[index].z * _zResolution * _zSize, _xResolution / lodFactor, _zResolution / lodFactor, _scatterLayers[layer].noiseScale, _scatterLayers[layer].noiseOctaves, _scatterLayers[layer].noiseWarpScale, _scatterLayers[layer].noiseWarpStrength);
//         for (int i = 0; i < points.Length; i++) {
//             // Vector3 vertex = vertices[(int)points[i].x + (int)points[i].y * sideLength];
//             // Vector3 normal = normals[(int)points[i].x + (int)points[i].y * sideLength];
//             Vector3 vertex = vertices[points[i]];
//             Vector3 normal = normals[points[i]];
//             Vector3 position = new Vector3(vertex.x + (_tilePool[index].x * _xSize * _xResolution), vertex.y, vertex.z + (_tilePool[index].z * _zSize * _zResolution));
//             position += new Vector3(_scatterLayers[layer].jitterStrength * (vertex.y % 2.951f / 2.951f), 0, _scatterLayers[layer].jitterStrength * (vertex.y % 1.622f / 1.622f));
//             if (!_scatterLayers[layer].spawnsInWater) {
//                 if (position.y < _lakePlaneHeight) continue;
//                 if (GetRiverValue(new Vector2(position.x, position.z)) > _scatterLayers[layer].waterThreshold) continue;
//             }
//             position.y = GetHeightValue(new Vector2(position.x, position.z));
//             int chosen = Mathf.Max(0, Mathf.CeilToInt(vertex.y % 1 * _scatterLayers[layer].prefabs.Length) - 1);
//             GameObject go = Instantiate(_scatterLayers[layer].prefabs[chosen], position, _scatterLayers[layer].alignToNormal ? Quaternion.LookRotation(normal) : _scatterLayers[layer].prefabs[chosen].transform.rotation);
//             go.transform.parent = _tilePool[index].obj.transform;
//         }
//     }

//     private void RockPass(Mesh targetMesh, int index) {
//         Vector3[] vertices = targetMesh.vertices;
//         Vector3[] normals = targetMesh.normals;

//         float[] rockVal = NoiseMaps.GenerateRockNoise((int) Mathf.Sqrt(vertices.Length), _tilePool[index].x * _xSize * _xResolution + _seed, _tilePool[index].z * _zSize * _zResolution + _seed, _rockPassScale, _rockPassScale, 1, _xResolution, _zResolution, true);
//         //This doesn't work with caves right now. Just need to make it per vertex noise instead of grid generated.
//         for (int i = 0; i < vertices.Length; i++) {
//             vertices[i] += _rockPassAmplitude * _rockPassCurve.Evaluate(Mathf.Abs(normals[i].y)) *_rockPassNoiseCurve.Evaluate(rockVal[i]) * normals[i];
//         }

//         targetMesh.vertices = vertices;
//     }

//     private int[,] SubdivideFaces(Mesh targetMesh, int[] toSubdivide) {
//         Vector3[] verticesOriginal = targetMesh.vertices;
//         int[] trianglesOriginal = targetMesh.triangles;

//         int originalVertLength = verticesOriginal.Length;
//         int originalTriangleLength = trianglesOriginal.Length;

//         Vector3[] vertices = new Vector3[verticesOriginal.Length + toSubdivide.Length];
//         int[] triangles = new int[trianglesOriginal.Length + 6 * toSubdivide.Length];

//         for (int i = 0; i < trianglesOriginal.Length; i++) {
//             triangles[i] = trianglesOriginal[i];
//         }

//         for (int i = 0; i < verticesOriginal.Length; i++) {
//             vertices[i] = verticesOriginal[i];
//         }

//         int[,] newVertices = new int[toSubdivide.Length,4];

//         for (int i = 0; i < toSubdivide.Length; i++) {
//             // An entry in the array is a triangle in triangles, so needs to be fetched with triangles[toSubdivide[i] * 3]
//             int pointAIndx = triangles[toSubdivide[i]];
//             int pointBIndx = triangles[toSubdivide[i] + 1];
//             int pointCIndx = triangles[toSubdivide[i] + 2];
//             Vector3 pointA = vertices[pointAIndx];
//             Vector3 pointB = vertices[pointBIndx];
//             Vector3 pointC = vertices[pointCIndx];
            
//             Vector3 newVertex = (pointA + pointB + pointC) / 3;

//             vertices[originalVertLength + i] = newVertex;
//             newVertices[i,0] = originalVertLength + i;
//             newVertices[i,1] = pointAIndx;
//             newVertices[i,2] = pointBIndx;
//             newVertices[i,3] = pointCIndx;
//             triangles[toSubdivide[i] ] = pointAIndx;
//             triangles[toSubdivide[i] + 1] = pointBIndx;
//             triangles[toSubdivide[i] + 2] = originalVertLength + i;
//             triangles[originalTriangleLength + i * 6] = pointBIndx;
//             triangles[originalTriangleLength + i * 6 + 1] = pointCIndx;
//             triangles[originalTriangleLength + i * 6 + 2] = originalVertLength + i;
//             triangles[originalTriangleLength + i * 6 + 3] = pointCIndx;
//             triangles[originalTriangleLength + i * 6 + 4] = pointAIndx;
//             triangles[originalTriangleLength + i * 6 + 5] = originalVertLength + i;
//         }

//         targetMesh.vertices = vertices;
//         targetMesh.triangles = triangles;
//         return newVertices;
//     }

//     private void CavePass(Mesh targetMesh, int index) {
//         //Decide which faces to subdivide
//         int[] triangles = targetMesh.triangles;
//         Vector3[] triangleSamples = new Vector3[triangles.Length / 3];
//         int[] sampledTris = new int[triangles.Length / 3];
//         Vector3[] faceNormals = new Vector3[sampledTris.Length];
//         Vector3[] vertices = targetMesh.vertices;
//         for (int i = 0, j = 0; i < triangles.Length / 6; i++) {
//             Vector3 pointA = vertices[triangles[i * 6]];
//             Vector3 pointB = vertices[triangles[i * 6 + 1]];
//             Vector3 pointC = vertices[triangles[i * 6 + 2]];
//             triangleSamples[j] = (pointA + pointB + pointC) / 3;
//             faceNormals[j] = Vector3.Cross(pointB - pointA, pointC - pointA);
//             sampledTris[j] = i * 6;
//             j++;
//             pointA = vertices[triangles[i * 6 + 3]];
//             pointB = vertices[triangles[i * 6 + 4]];
//             pointC = vertices[triangles[i * 6 + 5]];
//             triangleSamples[j] = (pointA + pointB + pointC) / 3;
//             faceNormals[j] = Vector3.Cross(pointB - pointA, pointC - pointA).normalized;
//             sampledTris[j] = i * 6 + 3;
//             j++;
//         }
//         float[] caveMap = NoiseMaps.GenerateCavePass(triangleSamples, _tilePool[index].x * _xResolution * _xSize + _seed, _tilePool[index].z * _zResolution * _zSize + _seed, _cavePassScale);

//         List<int> toSubdivide = new List<int>();
//         for (int i = 0; i < caveMap.Length; i++) {
//             if (caveMap[i] > 0.5f) {
//                 toSubdivide.Add(sampledTris[i]);
//             }
//         }
//         int[,] subdVerts = SubdivideFaces(targetMesh, toSubdivide.ToArray());
//         vertices = targetMesh.vertices;
//         Vector3[] normalsOriginal = targetMesh.normals;
//         Vector3[] normals = new Vector3[vertices.Length];
//         for (int i = 0; i < normalsOriginal.Length; i++) normals[i] = normalsOriginal[i];
//         Vector3[] effectValues = new Vector3[vertices.Length];
//         int[] normalizations = new int[vertices.Length];
//         for (int i = 0; i < subdVerts.GetLength(0); i++) {
//             if (vertices[subdVerts[i,0]].x <= _xResolution * 2 || vertices[subdVerts[i,0]].x >= _xResolution * (_xSize - 1) || vertices[subdVerts[i,0]].z <= _zResolution * 2 || vertices[subdVerts[i,0]].z >= _zResolution * (_zSize - 1)) continue;
//             //if (faceNormals[i].y > 0.6f) continue;
//             Vector3 subValue = new Vector3(0, _cavePassCurve.Evaluate(caveMap[toSubdivide[i] / 3]) * _cavePassAmplitude, 0);
//             effectValues[subdVerts[i,0]] += subValue;
//             effectValues[subdVerts[i,1]] += subValue;
//             effectValues[subdVerts[i,2]] += subValue;
//             effectValues[subdVerts[i,3]] += subValue;
//             normalizations[subdVerts[i,1]]++;
//             normalizations[subdVerts[i,2]]++;
//             normalizations[subdVerts[i,3]]++;
//             normals[subdVerts[i,0]] = faceNormals[i];
//         }
//         for (int i = 0; i < vertices.Length; i++) {
//             if (normalizations[i] > 0) {
//                 effectValues[i] /= normalizations[i];
//                 normals[i].Normalize();
//             }                
//             vertices[i] -= effectValues[i];
//         }
//         /* This did not work
//         for (int i = 0; i < vertices.Length; i++) {
//             List<int> newTriangles = new List<int>();
//             int triangleCount = 0;
//             for (int j = 0; j < triangles.Length; j++) {
//                 if (triangles[j] == i) triangleCount++;
//             }
//             //Debug.Log(triangleCount);
//             if (triangleCount == 6) {
//                 for (int j = 0; j < triangles.Length / 3; j++) {
//                     if (triangles[j * 3] != i && triangles[j * 3 + 1] != i && triangles[j * 3 + 2] != i) {
//                         newTriangles.Add(triangles[j * 3]);
//                         newTriangles.Add(triangles[j * 3 + 1]);
//                         newTriangles.Add(triangles[j * 3 + 2]);
//                     }
//                 }
//             }
//             else newTriangles.AddRange(triangles);
//             triangles = newTriangles.ToArray();
//         } */
//         targetMesh.vertices = vertices;
//         targetMesh.normals = normals;
//         targetMesh.RecalculateNormals();
//     }

//     private void RiverPass(Mesh targetMesh, int index) {
//         if (_tilePool[index].waterVertCount > 0) {
//             _waterVertices.RemoveRange(_tilePool[index].waterVertIndex, _tilePool[index].waterVertCount);
//             _waterTriangles.RemoveRange(_tilePool[index].waterTriIndex, _tilePool[index].waterTriCount);
//             for (int i = 0; i < _waterTriangles.Count; i++) {
//                 if (_waterTriangles[i] >= _tilePool[index].waterVertIndex) _waterTriangles[i] -= _tilePool[index].waterVertCount;
//             }
//             for (int i = 0; i < _xTiles * _zTiles; i++) {
//                 if (_tilePool[i].waterVertIndex > _tilePool[index].waterVertIndex) {
//                     _tilePool[i].waterVertIndex -= _tilePool[index].waterVertCount;
//                 }
//                 if (_tilePool[i].waterTriIndex > _tilePool[index].waterTriIndex) {
//                     _tilePool[i].waterTriIndex -= _tilePool[index].waterTriCount;
//                 }
//             }
//         }
//         _tilePool[index].waterVertCount = 0;
//         float maxDistance = Mathf.Max(Mathf.Abs(_tilePool[index].x - _playerXChunkScale), Mathf.Abs(_tilePool[index].z - _playerZChunkScale));
//         Vector3[] vertices = targetMesh.vertices;
//         Vector3[] normals = targetMesh.normals;
//         int lodFactor = (int) Mathf.Pow(2, _tilePool[index].currentLOD);
//         float[] heightMods = AmalgamNoise.GenerateRivers(_xSize, lodFactor, _tilePool[index].x * _xSize * _xResolution + _seed % 216812,
//             _tilePool[index].z * _zSize * _zResolution + _seed % 216812, _xResolution / lodFactor, _zResolution / lodFactor, _riverParameters.scale, _riverParameters.octaves, _riverParameters.lacunarity, _riverParameters.persistence, _riverParameters.warpScale, _riverParameters.warpStrength);
//         Vector3[] waterVerts = new Vector3[vertices.Length];
//         bool[] ignoreVerts = new bool[vertices.Length];
//         int ignored = 0;
//         for (int i = 0; i < heightMods.Length; i++) {
//             heightMods[i] = _riverParameters.noiseCurve.Evaluate(heightMods[i]) * _riverParameters.heightCurve.Evaluate(vertices[i].y / _maxPossibleHeight) * _riverParameters.normalCurve.Evaluate(Mathf.Abs(normals[i].y));
//             waterVerts[i] = vertices[i] - new Vector3(0, _riverParameters.waterLevel, 0);
//             if (heightMods[i] == 0) {
//                 waterVerts[i] -= new Vector3(0, _riverParameters.amplitude / 10, 0);
//                 ignoreVerts[i] = true;
//                 ignored++;
//                 continue;
//             }
//             vertices[i] -= new Vector3(0, heightMods[i] * _riverParameters.amplitude, 0);
//             waterVerts[i] += _tilePool[index].obj.transform.position;
//         }

//         int sideLength = (int) Mathf.Sqrt(vertices.Length);
//         int[] triangles = new int[(sideLength - 1) * (sideLength - 1) * 6];
//         for (int i = 0, j = 0; i < waterVerts.Length; i++) {
//             if (i / sideLength >= sideLength - 1) continue;
//             if (i % sideLength >= sideLength - 1) continue;
//             if (i / sideLength == 0) continue;
//             if (i % sideLength == 0) continue;
//             triangles[j * 6] = i;
//             triangles[j * 6 + 1] = i + sideLength;
//             triangles[j * 6 + 2] = i + 1;
//             triangles[j * 6 + 3] = i + sideLength;
//             triangles[j * 6 + 4] = i + sideLength + 1;
//             triangles[j * 6 + 5] = i + 1;
//             j++;
//         }
//         targetMesh.vertices = vertices;
//         if (maxDistance >= _maxWaterRange && _limitWater) return;
//         int waterVertsLength = _waterVertices.Count;
//         int waterTriLength = _waterTriangles.Count;
//         int[] realIndices = new int[waterVerts.Length];
//         Vector3[] verts = new Vector3[waterVerts.Length - ignored];
//         for (int i = 0, j = 0; i < waterVerts.Length; i++) {
//             if (!ignoreVerts[i]) {
//                 verts[j] = waterVerts[i];
//                 realIndices[i] = j;
//                 j++;
//             }
//         }
//         int triCount = 0;
//         for (int i = 0; i < triangles.Length; i+= 3) {
//             if (!ignoreVerts[triangles[i]] && !ignoreVerts[triangles[i + 1]] && !ignoreVerts[triangles[i + 2]]) {
//                 triCount += 3;
//             }
//         }
//         int[] tris = new int[triCount];
//         for (int i = 0, j = 0; i < triangles.Length; i+= 3) {
//             if (!ignoreVerts[triangles[i]] && !ignoreVerts[triangles[i + 1]] && !ignoreVerts[triangles[i + 2]]) {
//                 tris[j] = realIndices[triangles[i]] + waterVertsLength;
//                 tris[j + 1] = realIndices[triangles[i + 1]] + waterVertsLength;
//                 tris[j + 2] = realIndices[triangles[i + 2]] + waterVertsLength;
//                 j += 3;
//             }
//         }
//         _waterVertices.AddRange(verts);
//         _waterTriangles.AddRange(tris);
//         _tilePool[index].waterVertIndex = waterVertsLength;
//         _tilePool[index].waterVertCount = waterVerts.Length - ignored;
//         _tilePool[index].waterTriIndex = waterTriLength;
//         _tilePool[index].waterTriCount = _waterTriangles.Count - waterTriLength;
//     }

//     private void UpdateMesh(Mesh targetMesh, int index) {
//         targetMesh.normals = CalculateNormalsJobs(targetMesh);
//         RiverPass(targetMesh, index);
//         // CavePass(targetMesh, index);
//         targetMesh.triangles = CullTriangles(targetMesh, index);
//         //RockPass(targetMesh, index);
//         targetMesh.RecalculateBounds();
//     }

//     [BurstCompile]
//     private struct NormalJob : IJobParallelFor
//     {

//         [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<float3> vertices;
//         [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<int> triangles;
//         [NativeDisableParallelForRestriction] public NativeArray<float3> normals;

//         public void Execute(int index) {
//             int vertexIndexA = triangles[index * 3];
//             int vertexIndexB = triangles[index * 3 + 1];
//             int vertexIndexC = triangles[index * 3 + 2];
//             float3 pointA = vertices[vertexIndexA];
//             float3 normal = math.cross(vertices[vertexIndexB] - pointA, vertices[vertexIndexC] - pointA);
//             normals[vertexIndexA] += normal;
//             normals[vertexIndexB] += normal;
//             normals[vertexIndexC] += normal;
//         }

//     }

//     private Vector3[] CalculateNormalsJobs(Mesh targetMesh) {
//         Vector3[] verts = targetMesh.vertices;
//         int[] tris = targetMesh.triangles;
//         NativeArray<float3> vertices = new NativeArray<float3>(verts.Length, Allocator.TempJob);
//         NativeArray<int> triangles = new NativeArray<int>(tris.Length, Allocator.TempJob);
//         NativeArray<float3> normals = new NativeArray<float3>(verts.Length, Allocator.TempJob);
//         for (int i = 0; i < verts.Length; i++) vertices[i] = verts[i];
//         for (int i = 0; i < tris.Length; i++) triangles[i] = tris[i];
//         NormalJob job = new NormalJob {
//             vertices = vertices,
//             triangles = triangles,
//             normals = normals
//         };
//         JobHandle handle = job.Schedule(tris.Length / 3, 64);
//         handle.Complete();
//         Vector3[] normalOut = new Vector3[verts.Length];
//         normalOut = normals.Reinterpret<Vector3>().ToArray();
//         for (int i = 0; i < normalOut.Length; i++) normalOut[i].Normalize();
//         vertices.Dispose();
//         triangles.Dispose();
//         normals.Dispose();
//         return normalOut;
//     }

// }