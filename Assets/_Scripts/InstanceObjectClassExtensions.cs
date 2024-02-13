using System;
using System.Collections.Generic;
using UnityEngine;

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
public class InstanceObjectsHandler
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
    public Vector2 Position;
    public FoliageType Type;

    public FoliageData(Vector2 position, FoliageType type)
    {
        this.Position = position;
        this.Type = type;
    }
}
