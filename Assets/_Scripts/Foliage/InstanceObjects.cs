using System.Collections.Generic;
using UnityEngine;

public class InstanceObjects : MonoBehaviour
{
    public Transform _playerTransform;
    
    private InstanceObjectsHandler _instanceObjects;

    public int numberOfInstancesSquared = 5;
    public int instanceDensity = 5;

    
    public float chunkWidth;

    public int framesPerChunkUpdate = 5;
    private int currentFrame = 0;
    
    public bool render = true;
    
    private void Awake() {
        currentFrame = framesPerChunkUpdate;
        
        _instanceObjects = new InstanceObjectsHandler();
        _instanceObjects.InitializeDictionary();

        WorldGenInfo._foliageChunkWidth = chunkWidth;
    }

    // Temporary test case
    private void Start() {
        for (int i = 0; i < numberOfInstancesSquared; i++) {
            for (int j = 0; j < numberOfInstancesSquared; j++) {
                _instanceObjects.AddObject(new Vector2(
                    i * instanceDensity - (numberOfInstancesSquared / 2 * instanceDensity), 
                    j * instanceDensity - (numberOfInstancesSquared / 2 * instanceDensity)
                    ), FoliageType.Tree);
            }
        }
    }

    private void Update() {
        currentFrame++;
        
        foreach (FoliageChunk chunk in _instanceObjects.ChunksDictionary.Values) {
            foreach (var type in chunk.FoliageTypePerChunk.Values) {
                RenderObject(type.Item1, type.Item2, chunk);
            }
        }

        if (currentFrame >= framesPerChunkUpdate) currentFrame = 0;
    }
    
    private void RenderObject(List<FoliageData> typeOfFoliage, FoliageRenderingData data, FoliageChunk chunk) {
        if (currentFrame != framesPerChunkUpdate) { 
            if (!data.hasInitialized) return;
            
            if (render) Graphics.RenderMeshIndirect(data.rp, data.stashedMesh, data.commandBuf, data.commandCount);
            return;
        }
        
        FoliageData firstData = typeOfFoliage[0];
        FoliageType type = firstData.Type;
        FoliageMetaData metaData = _instanceObjects.GetFoliageMetaData(type);
        int numberOfFoliage = typeOfFoliage.Count;

        int numberOfLODRanges = _instanceObjects.GetFoliageMetaData(firstData.Type)._lodData.Length;
        float[] lodRanges = new float[numberOfLODRanges];

        BuildLODRanges(metaData, numberOfLODRanges, ref lodRanges);
        
        int lodLevel = GetLODRange(chunk.position, lodRanges);

        
        if (lodLevel == chunk.previousLODLevel) {
            // If the chunk doesnt change LOD range, take stashed data
            if (lodLevel == -1) return; // Break out of rendering look if LOD is out of range
            
            if (data.positions.Length != numberOfFoliage) data.FillInstanceData(typeOfFoliage); 

            if (render) Graphics.RenderMeshIndirect(data.rp, data.stashedMesh, data.commandBuf, data.commandCount);
        }
        else {
            if (lodLevel == -1) return; // Break out of rendering look if LOD is out of range
            
            if (data.positions.Length != numberOfFoliage) data.FillInstanceData(typeOfFoliage); 
            
            (Mesh, Material) Tuple = metaData.GetLODData(lodLevel);
            // If not, reassign data
            data.UpdateLOD(Tuple, chunk.position);
            
            if (render) Graphics.RenderMeshIndirect(data.rp, data.stashedMesh, data.commandBuf, data.commandCount);
        }
    }

    private void BuildLODRanges(FoliageMetaData data, int numberOfLODRanges, ref float[] array) {
        for (int i = 0; i < numberOfLODRanges; i++) {
            array[i] = data._lodData[i]._lodRange;
        }
    }

    private int GetLODRange(Vector3 pos, float[] ranges) {
        float distance = Vector3.Distance(_playerTransform.position, pos);
        
        // Searches for LOD range, if none are found then defaults to last range.
        for (int i = 0; i < ranges.Length; i++) {
            // Debug.Log(ranges[i]);
            if (distance < ranges[i]) return i;
        }

        return -1;
    }

    private float DistanceSquared(Vector3 vector1, Vector3 vector2) {
        Vector3 diff = vector2 - vector1;
        float distance = (diff.x * diff.x) + (diff.y * diff.y) + (diff.z * diff.z);
        return distance;
    }
}