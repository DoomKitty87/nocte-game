using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InstanceObjects : MonoBehaviour
{
    public Transform _playerTransform;
    
    private InstanceObjectsHandler _instanceObjects;

    public int numberOfInstancesSquared = 5;

    private void Awake() {
        _instanceObjects = new InstanceObjectsHandler();
        _instanceObjects.InitializeDictionary();
    }

    // Temporary test case
    private void Start() {
        for (int i = 0; i < numberOfInstancesSquared; i++) {
            for (int j = 0; j < numberOfInstancesSquared; j++) {
                _instanceObjects.AddObject(new Vector2(i * 5 - (numberOfInstancesSquared / 2 * 5), j * 5 - (numberOfInstancesSquared / 2 * 5)), FoliageType.TestObject);
            }
        }
    }

    private void Update() {
        foreach (FoliageChunk chunk in _instanceObjects.ChunksDictionary.Values) {
            foreach (List<FoliageData> type in chunk.FoliageTypePerChunk.Values) {
                RenderObject(type);
            }
        }
    }

    private void RenderObject(List<FoliageData> typeOfFoliage) {
        FoliageData firstData = typeOfFoliage[0];
        FoliageType type = firstData.Type;
        FoliageMetaData metaData = _instanceObjects.GetFoliageMetaData(type);
        
        // TODO: Optimize this by precalculating number of possible LOD in chunk
        int numberOfLODRanges = _instanceObjects.GetFoliageMetaData(firstData.Type)._lodData.Length;
        float[] lodRanges = new float[numberOfLODRanges];
        List<FoliageData>[] lodLevels = new List<FoliageData>[numberOfLODRanges];
        

        BuildLODRanges(metaData, numberOfLODRanges, ref lodRanges);

        for (int i = 0; i < lodLevels.Length; i++) {
            lodLevels[i] = new List<FoliageData>();
        }
        
        foreach (FoliageData data in typeOfFoliage) {
            int lodLevel = GetLODRange(data.Position, lodRanges);
            lodLevels[lodLevel].Add(data);
        }

        for (int i = 0; i < lodLevels.Length - 1; i++) {
            if (lodLevels[i].Count == 0) continue;
            
            Material mat = metaData._lodData[i]._material;
            Mesh mesh = metaData._lodData[i]._mesh;
            
            RenderParams rp = new RenderParams(mat);

            Matrix4x4[] instData = new Matrix4x4[lodLevels[i].Count];

            for (int j = 0; j < instData.Length; j++) {
                instData[j] = Matrix4x4.Translate(lodLevels[i][j].Position);
            }
            
            // Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
        }
    }

    private void BuildLODRanges(FoliageMetaData data, int numberOfLODRanges, ref float[] array) {
        for (int i = 0; i < numberOfLODRanges; i++) {
            array[i] = data._lodData[i]._lodRange * data._lodData[i]._lodRange; // Squared due to DistanceSquared function
        }
    }

    private int GetLODRange(Vector3 pos, float[] ranges) {
        float distance = DistanceSquared(_playerTransform.position, pos);
        
        // Searches for LOD range, if none are found then defaults to last range.
        for (int i = 0; i < ranges.Length - 1; i++) {
            // Debug.Log(ranges[i]);
            if (distance < ranges[i]) return i;
        }

        return ranges.Length - 1;
    }

    private float DistanceSquared(Vector3 vector1, Vector3 vector2) {
        Vector3 diff = vector2 - vector1;
        float distance = (diff.x * diff.x) + (diff.y * diff.y) + (diff.z * diff.z);
        return distance;
    }
    
}

