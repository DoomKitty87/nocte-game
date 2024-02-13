using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InstanceObjects : MonoBehaviour
{
    public Transform _playerTransform;
    private Vector2 _playerPositionXZ;
    
    private InstanceObjectsHandler _instanceObjects;

    private void Awake() {
        _instanceObjects = new InstanceObjectsHandler();
        _instanceObjects.InitializeDictionary();
    }

    // Temporary test case
    private void Start() {
        for (int i = 0; i < 5000; i++) {
            for (int j = 0; j < 5000; j++) {
                _instanceObjects.AddObject(new Vector2(i * 5 - 12500, j * 5 - 12500), FoliageType.TestObject);
            }
        }
    }

    private void Update() {
        _playerPositionXZ = new Vector2(_playerTransform.position.x, _playerTransform.position.z);
        
        int numberOfChunks = _instanceObjects.ChunksDictionary.Count;
        int numberOfTypes = _instanceObjects.FoliageDataDictionary.Count;

        // For each chunk
        foreach (FoliageChunk chunk in _instanceObjects.ChunksDictionary.Values) {
            // For each type in given chunk
            foreach (List<FoliageData> type in chunk.FoliageTypePerChunk.Values) {
                // For each type of foliage in given chunk
                RenderObject(chunk, type);
            }
        }
    }

    private void RenderObject(FoliageChunk chunk, List<FoliageData> typeOfFoliage) {
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

        for (int i = 0; i < lodLevels.Length; i++) {
            if (lodLevels[i].Count == 0) continue;
            
            Material mat = metaData._lodData[i]._material;
            Mesh mesh = metaData._lodData[i]._mesh;
            
            RenderParams rp = new RenderParams(mat);

            Matrix4x4[] instData = new Matrix4x4[lodLevels[i].Count];

            for (int j = 0; j < instData.Length; j++) {
                Vector2 positionXZ = lodLevels[i][j].Position;
                Vector3 positionXYZ = new Vector3(
                    positionXZ.x,
                    0, //AmalgamNoise.GetPosition(positionXZ),
                    positionXZ.y
                );
                
                instData[j] = Matrix4x4.Translate(positionXYZ);
            }
            
            Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
        }
    }

    private void BuildLODRanges(FoliageMetaData data, int numberOfLODRanges, ref float[] array) {
        for (int i = 0; i < numberOfLODRanges; i++) {
            array[i] = data._lodData[i]._lodRange;
        }
    }

    private int GetLODRange(Vector2 pos, float[] ranges) {
        float distance = Vector2.Distance(_playerPositionXZ, pos);
        
        // Searches for LOD range, if none are found then defaults to last range.
        for (int i = 0; i < ranges.Length - 1; i++) {
            if (distance < ranges[i]) return i;
        }

        return ranges.Length - 1;
    }

}

