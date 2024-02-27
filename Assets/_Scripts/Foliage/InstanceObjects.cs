using System.Collections.Generic;
using UnityEngine;

public class InstanceObjects : MonoBehaviour
{
    public Transform _playerTransform;
    
    private InstanceObjectsHandler _instanceObjects;

    public int numberOfChunksInOneDirection = 5;
    
    // public int numberOfInstancesSquared = 5;
    // public int instanceDensity = 5;

    public int foliagePerChunk = 1000;
    
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

        int samples = Mathf.FloorToInt(Mathf.Sqrt(foliagePerChunk));
        
        for (int i = 0; i < numberOfChunksInOneDirection; i++) {
            for (int j = 0; j < numberOfChunksInOneDirection; j++) {
                Vector2[] positions = AmalgamNoise.GenerateFoliage(
                    new Vector2(-(chunkWidth * numberOfChunksInOneDirection / 2) + chunkWidth * i,
                        -(chunkWidth * numberOfChunksInOneDirection / 2) + chunkWidth * j),
                new Vector2(-(chunkWidth * numberOfChunksInOneDirection / 2) + chunkWidth * (i + 1),
                    -(chunkWidth * numberOfChunksInOneDirection / 2) + chunkWidth * (j + 1)),
                    samples, 10, 2000, 0.9f, 2, 0.005f, 1500, 0.5f, 0
                );
                
                foreach (Vector2 pos in positions) _instanceObjects.AddObject(new Vector2(
                    pos.x,
                    pos.y),
                    FoliageType.TestObject);
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
    
    private void RenderObject(List<FoliageData> typeOfFoliage, RenderFoliage rd, FoliageChunk chunk) {
        // if (currentFrame != framesPerChunkUpdate) { 
        //     if (render) renderer.Render();
        //     return;
        // }
        
        
        FoliageData firstData = typeOfFoliage[0];
        FoliageType type = firstData.Type;
        FoliageMetaData metaData = _instanceObjects.GetFoliageMetaData(type);
        int numberOfFoliage = typeOfFoliage.Count;

        int numberOfLODRanges = _instanceObjects.GetFoliageMetaData(firstData.Type)._lodData.Length;
        float[] lodRanges = new float[numberOfLODRanges];

        BuildLODRanges(metaData, numberOfLODRanges, ref lodRanges);
        
        int lodLevel = GetLODRange(chunk.position, lodRanges);

        Vector3[] positions = new Vector3[numberOfFoliage];

        for (int i = 0; i < numberOfFoliage; i++) {
            positions[i] = typeOfFoliage[i].Position;
        }

        Debug.Log(positions.Length);
        
        rd.UpdateBuffer(positions, Vector3.zero);

        
        if (lodLevel == chunk.previousLODLevel) {
            // If the chunk doesnt change LOD range, take stashed data
            if (lodLevel == -1) return; // Break out of rendering look if LOD is out of range
            
            if (rd._count != numberOfFoliage) rd.UpdateBuffer(positions, chunk.position); 

            if (render) rd.Render();
        }
        else {
            if (lodLevel == -1) return; // Break out of rendering look if LOD is out of range
            
            var tuple = metaData.GetLODData(lodLevel);
            // If not, reassign data
            rd._instanceMesh = tuple.Item1;
            rd._instanceMaterial = tuple.Item2;
            rd._initialized = true;
            
            rd.UpdateBuffer(positions, chunk.position);
            
            if (render) rd.Render();
        }
    }

    private static void BuildLODRanges(FoliageMetaData data, int numberOfLODRanges, ref float[] array) {
        for (int i = 0; i < numberOfLODRanges; i++) {
            array[i] = data._lodData[i]._lodRange;
        }
    }

    private int GetLODRange(Vector3 pos, float[] ranges) {
        return 0;
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