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
        
        ChunksDictionary.TryAdd(chunkPosition, new FoliageChunk(position));

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

    private Vector2Int GetChunkPosition(Vector2 position) {
        float width = WorldGenInfo._foliageChunkWidth;
        return new Vector2Int(Mathf.FloorToInt((position.x - width / 2) / width),
                              Mathf.FloorToInt((position.y - width / 2) / width));
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
    public readonly Dictionary<FoliageType, (List<FoliageData>, RenderFoliage)> FoliageTypePerChunk = new Dictionary<FoliageType, (List<FoliageData>, RenderFoliage)>();

    public Vector3 position;
    public int previousLODLevel = -2;
    
    public FoliageChunk(Vector2 position) {
        Vector2 positionXZ = position + Vector2.one * (WorldGenInfo._foliageChunkWidth / 2); // Offsetting position to be center of chunk
        float yPosition = AmalgamNoise.GetPosition(position);
        this.position = new Vector3(positionXZ.x, yPosition, positionXZ.y);
    }
    
    public void AddObject(Vector2 position, FoliageType type)
    {
        if (!FoliageTypePerChunk.ContainsKey(type))
        {
            FoliageTypePerChunk[type] = (new List<FoliageData>(), new RenderFoliage());
            // FoliageTypePerChunk[type].Item2.Initialize(); // Initialize new renderer
        }

        FoliageTypePerChunk[type].Item1.Add(new FoliageData(GetWorldPosition(position), type));
    }

    private Vector3 GetWorldPosition(Vector2 position) =>
        new (position.x, AmalgamNoise.GetPosition(position), position.y);
}

public class FoliageRenderingData
{
    public bool hasInitialized = false;
    
    public Mesh stashedMesh;
    
    public RenderParams rp = new RenderParams();
    public Vector4[] positions = Array.Empty<Vector4>(); // Must be assigned due to Length check

    public GraphicsBuffer commandBuf;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;

    public int commandCount = 0;
    
    private static readonly int ObjectToWorld = Shader.PropertyToID("_ObjectToWorld");
    private static readonly int Positions = Shader.PropertyToID("_Positions");

    // Should be called when more foliage is added to a chunk, also only on render
    public void FillInstanceData(List<FoliageData> data) {
        hasInitialized = true;
        
        commandCount = data.Count;

        DestroySelf();
        
        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];

        positions = new Vector4[commandCount];
        for (int i = 0; i < data.Count; i++) {
            positions[i] = (Vector4)(data[i].Position);
        }
    }

    public void UpdateLOD((Mesh, Material) tuple, Vector3 chunkPosition) {
        Mesh mesh = tuple.Item1;
        Material mat = tuple.Item2;
        rp = new RenderParams(mat);
        rp.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetMatrix(ObjectToWorld, Matrix4x4.Translate(Vector3.zero));
        rp.matProps.SetVectorArray(Positions, positions);
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)commandCount;
        commandBuf.SetData(commandData);

        stashedMesh = mesh;
    }

    public void DestroySelf() {
        commandBuf?.Release();
        commandBuf = null;
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
    public Vector3 Position;
    public FoliageType Type;

    public FoliageData(Vector3 position, FoliageType type)
    {
        this.Position = position;
        this.Type = type;
    }
}

public class RenderFoliage
{
    public bool _initialized;
    
    public Mesh _instanceMesh;
    public Material _instanceMaterial;
    public Vector3 _chunkPosition;
    
    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private ComputeBuffer _argsBuffer;
    public int _count = 0;

    private ComputeBuffer _positionsBuffer;

    private static readonly int PositionBuffer = Shader.PropertyToID("position_buffer");

    public RenderFoliage() {
        Initialize();
    }
    
    private void Initialize() {
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    }
    
    public void Render() {
        if (_initialized) Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
    }

    private void Disable() {
        _positionsBuffer?.Release();
        _positionsBuffer = null;

        _argsBuffer.Release();
        _argsBuffer = null;
    }

    public void UpdateBuffer(Vector3[] positions3, Vector3 chunkPosition) {
        if (!_initialized) return;
        
        _count = positions3.Length;
        
        _positionsBuffer?.Release();
        _positionsBuffer = new ComputeBuffer(_count, 16);

        var positions4 = new Vector4[_count];
        for (int i = 0; i < _count; i++) positions4[i] = positions3[i];
        
        _positionsBuffer.SetData(positions4);
        _instanceMaterial.SetBuffer(PositionBuffer, _positionsBuffer);

        _args[0] = _instanceMesh.GetIndexCount(0);
        _args[1] = (uint)_count;
        _args[2] = _instanceMesh.GetIndexStart(0);
        _args[3] = _instanceMesh.GetBaseVertex(0);
        
        _argsBuffer.SetData(_args);
    }
}
