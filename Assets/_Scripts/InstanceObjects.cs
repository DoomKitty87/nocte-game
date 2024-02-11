using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RenderInstancedObjects : MonoBehaviour
{
    private InstanceObjects _instanceObjects;

    private void Awake() {
        _instanceObjects = new InstanceObjects();
        _instanceObjects.InitializeDictionary();
    }

    // Temporary test case
    private void Start() {
        _instanceObjects.AddObject(new Vector2(0, 0), FoliageType.Tree);
    }

    private void Update() {
        int numberOfChunks = _instanceObjects.ChunksDictionary.Count;
        int numberOfTypes = _instanceObjects.FoliageDataDictionary.Count;

        // For each chunk
        foreach (FoliageChunk chunk in _instanceObjects.ChunksDictionary.Values) {
            // For each type in given chunk
            foreach (List<FoliageData> type in chunk.FoliageTypePerChunk.Values) {
                // For each type of foliage in given chunk
                RenderObject(type);
            }
        }
    }

    private void RenderObject(List<FoliageData> type) {
        int numberOfObjects = type.Count;
        
        RenderParams rp = new RenderParams(material);
        Matrix4x4[] instData = new Matrix4x4[numberOfObjects];
        
        for (int i = 0; i < numberOfObjects; i++) {
            Mesh mesh = _instanceObjects.GetFoliageData(type[i].Type).GetLODData(0).Item1;
            Material material = _instanceObjects.GetFoliageData(type[i].Type).GetLODData(0).Item2;
            Vector3 position = new Vector3(type[i].Position.x, AmalgamNoise.GetPosition(type[i].Position), type[i].Position.y);
            

            instData[i] = Matrix4x4.Translate(position);
        }
        
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
    }
}

/// <summary>
/// Class used to handle all chunk fetching/settings.
/// Contains a dictionary of all chunks, looked up by position value in worldspace.
/// Also contains dictionary linking FoliageTypes to FoliageMetaData so FoliageMetaData doesn't need to be passed
/// between objects.
///
/// Will probably later add a way to add multiple objects at once, for now this is fine I guess.
///
/// InitializeDictionary should be called on start in RenderInstancedObjects, maybe change this?
/// </summary>
public class InstanceObjects
{
    public readonly Dictionary<Vector2Int, FoliageChunk> ChunksDictionary = new Dictionary<Vector2Int, FoliageChunk>();

    public readonly Dictionary<FoliageType, FoliageMetaData> FoliageDataDictionary =
        new Dictionary<FoliageType, FoliageMetaData>();

    public void AddObject(Vector2 position, FoliageType type)
    {
        var chunkPosition = GetChunkPosition(position);
        
        ChunksDictionary.TryAdd(chunkPosition, new FoliageChunk());

        ChunksDictionary[chunkPosition].AddObject(position, type);
    }

    public void RemoveObject() {
        throw new NotImplementedException();
    }

    public void RemoveChunk(Vector2 position) {
        // Glorified TryRemove()
        var key = GetChunkPosition(position);
        if (ChunksDictionary.TryGetValue(key, out _)) {
            ChunksDictionary.Remove(key);
        }
    }

    private Vector2Int GetChunkPosition(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / WorldGenInfo._tileEdgeSize),
                              Mathf.FloorToInt(position.y / WorldGenInfo._tileEdgeSize));
    }

    public void InitializeDictionary() {
        FoliageMetaData[] foliageMetaDataArray = Resources.LoadAll<FoliageMetaData>("FoliageObjects");

        foreach (FoliageMetaData data in foliageMetaDataArray) {
            FoliageDataDictionary[data._type] = data;
        }
    }

    public FoliageMetaData GetFoliageData(FoliageType type) {
        if (FoliageDataDictionary.TryGetValue(type, out var data)) {
            return data;
        }
        else {
            Debug.LogError($"No FoliageData type {type} found in foliageDataDictionary");
            return null;
        }
    }
}


/// <summary>
/// The actual chunk object being stored in dictionary on InstanceObjects.
/// Contains a dictionary of types for the current chunk, mainly for tidiness but also helps in GPU instancing phase.
/// </summary>
public class FoliageChunk // Might at some point incorporate this with other Chunk class?
{
    public readonly Dictionary<FoliageType, List<FoliageData>> FoliageTypePerChunk = new Dictionary<FoliageType, List<FoliageData>>();

    public void AddObject(Vector2 position, FoliageType type)
    {
        if (!FoliageTypePerChunk.ContainsKey(type))
        {
            FoliageTypePerChunk[type] = new List<FoliageData>();
        }

        FoliageTypePerChunk[type].Add(new FoliageData(position, type));
    }
}


public enum FoliageType
{
    Tree,
    Shrub,
    Bush
    // Add more types here
}


/// <summary>
/// 'Meta' data stored on each object.
/// Can be used to add more data (condition, color, windiness etc).
/// </summary>
public class FoliageData
{
    public Vector2 Position;
    public FoliageType Type;

    public FoliageData(Vector2 position, FoliageType type)
    {
        this.Position = position;
        this.Type = type;
    }
}


/// <summary>
/// ScriptableObject for each foliage type.
///
/// Should NOT be used outside of InstanceObjects script.
/// Instead use FoliageData as it contains this object through FoliageType and GetFoliageData.
/// 
/// FoliageType is used for easily choosing a type of foliage from outside scripts,
/// automatically set in InstanceObject InitializeDictionary
///
/// Public fetch function to return current mesh and material at given LOD level
/// </summary>
[CreateAssetMenu(fileName = "FoliageMetaData", menuName = "ScriptableObjects/FoliageMetaData")]
public class FoliageMetaData : ScriptableObject
{
    [HideInInspector] public FoliageType _type;
    
    [Serializable]
    public class LODData
    {
        public int _lodLevel;
        public Mesh _mesh;
        public Material _material;
    }

    public List<LODData> _lodData = new List<LODData>();
    
    public (Mesh, Material) GetLODData(int lodLevel)
    {
        LODData lodMesh = _lodData.Find(mesh => mesh._lodLevel == lodLevel);
        LODData lodMat = _lodData.Find(material => material._lodLevel == lodLevel);
        
        return ((lodMesh != null && lodMat != null) ? (mesh: lodMesh._mesh, material: lodMat._material) : (null, null));
    }
}
