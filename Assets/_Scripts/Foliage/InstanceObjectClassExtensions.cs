using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to handle all chunk fetching/settings.
/// Contains a dictionary of all chunks, looked up by position value in world space.
/// Also contains dictionary linking FoliageTypes to FoliageMetaData so FoliageMetaData doesn't need to be passed
/// between objects.
///
/// Will probably later add a way to add multiple objects at once, for now this is fine I guess.
///
/// InitializeDictionary should be called on start in RenderInstancedObjects, maybe change this?
/// </summary>
public class InstanceObjectsHandler
{
    public readonly Dictionary<Vector2Int, FoliageChunk> ChunksDictionary = new Dictionary<Vector2Int, FoliageChunk>();

    public readonly Dictionary<FoliageType, FoliageMetaData> FoliageDataDictionary =
        new Dictionary<FoliageType, FoliageMetaData>();

    public void AddObject(Vector2 position, FoliageType type)
    {
        var chunkPosition = GetChunkPosition(position);
        
        ChunksDictionary.TryAdd(chunkPosition, new FoliageChunk(position, chunkPosition));

        ChunksDictionary[chunkPosition].AddObject(position, type);
    }

    public void RemoveObject() {
        throw new NotImplementedException();
    }

    public void RemoveChunk(Vector2 position) {
        // Glorified TryRemove
        var key = GetChunkPosition(position);
        if (ChunksDictionary.TryGetValue(key, out _)) {
            ChunksDictionary.Remove(key);
        }
    }

    private Vector2Int GetChunkPosition(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / WorldGenInfo._foliageChunkWidth),
                              Mathf.FloorToInt(position.y / WorldGenInfo._foliageChunkWidth));
    }

    public void InitializeDictionary() {
        FoliageMetaData[] foliageMetaDataArray = Resources.LoadAll<FoliageMetaData>("FoliageObjects");

        foreach (FoliageMetaData data in foliageMetaDataArray) {
            FoliageDataDictionary[data._type] = data;
        }
    }

    public FoliageMetaData GetFoliageMetaData(FoliageType type) {
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

    public Vector3 position;
    public int previousLODLevel = -2;

    public FoliageRenderingData renderData;
    
    public FoliageChunk(Vector2 position, Vector2Int chunkIndex) {
        Vector2 positionXZ = position * WorldGenInfo._foliageChunkWidth + Vector2.one * (WorldGenInfo._foliageChunkWidth / 2); // Offsetting position to be center of chunk
        float yPosition = AmalgamNoise.GetPosition(position);
        this.position = new Vector3(positionXZ.x, yPosition, positionXZ.y);

        this.renderData = new FoliageRenderingData();
    }
    
    public void AddObject(Vector2 position, FoliageType type)
    {
        if (!FoliageTypePerChunk.ContainsKey(type))
        {
            FoliageTypePerChunk[type] = new List<FoliageData>();
        }

        FoliageTypePerChunk[type].Add(new FoliageData(GetWorldPosition(position), type));
    }

    private Vector3 GetWorldPosition(Vector2 position) =>
        new (position.x, AmalgamNoise.GetPosition(position), position.y);
}

public class FoliageRenderingData
{
    public Material mat;
    public Mesh mesh;
    public RenderParams rp;
    public Matrix4x4[] instData;
}

public enum FoliageType
{
    Tree,
    Shrub,
    Bush,
    TestObject
    // Add more types here
}


/// <summary>
/// 'Meta' data stored on each object.
/// Can be used to add more data (condition, color, windiness etc).
/// </summary>
public class FoliageData
{
    public Vector3 Position;
    public FoliageType Type;

    public FoliageData(Vector3 position, FoliageType type)
    {
        this.Position = position;
        this.Type = type;
    }
}
