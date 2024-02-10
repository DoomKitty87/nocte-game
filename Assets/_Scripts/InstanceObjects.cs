using System;
using System.Collections.Generic;
using UnityEngine;

public class RenderInstancedObjects : MonoBehaviour
{
    private InstanceObjects _instanceObject;

    private void Awake() {
        _instanceObject = new InstanceObjects();
    }

    private void Start() {
        _instanceObject.AddObject(new Vector2(0, 0), ObjectType.Tree);
    }

    private void Update() {
        
    }
}

public class InstanceObjects
{
    private Dictionary<Vector2Int, FoliageChunks> chunks = new Dictionary<Vector2Int, FoliageChunks>();

    public void AddObject(Vector2 position, ObjectType type)
    {
        Vector2Int chunkPosition = GetChunkPosition(position);
        
        chunks.TryAdd(chunkPosition, new FoliageChunks());

        chunks[chunkPosition].AddObject(position, type, metaData);
    }

    private Vector2Int GetChunkPosition(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / WorldGenInfo._tileEdgeSize),
                              Mathf.FloorToInt(position.y / WorldGenInfo._tileEdgeSize));
    }
}

public class FoliageChunks // Might at some point incorporate this with Chunk type?
{
    private Dictionary<ObjectType, List<ObjectData>> objects = new Dictionary<ObjectType, List<ObjectData>>();

    public void AddObject(Vector2 position, ObjectType type)
    {
        // Check if the object type exists, if not, create it
        if (!objects.ContainsKey(type))
        {
            objects[type] = new List<ObjectData>();
        }

        objects[type].Add(new ObjectData(position, metaData));
    }
}

public enum ObjectType
{
    Tree,
    Shrub,
    Bush
    // Add more stuff here
}

[CreateAssetMenu(fileName = "FoliageMetaData", menuName = "ScriptableObjects/FoliageMetaData")]
public class FoliageMetaData : ScriptableObject
{
    public ObjectType type;
    
    [System.Serializable]
    public class LODData
    {
        public int lodLevel;
        public Mesh mesh;
        public Material material;
    }

    public List<LODData> lodDatas = new List<LODData>();

    // Add any other metadata properties you need

    // Function to get the appropriate LOD mesh based on the LOD level
    public (Mesh, Material) GetLODData(int lodLevel)
    {
        LODData lodMesh = lodDatas.Find(mesh => mesh.lodLevel == lodLevel);
        LODData lodMat = lodDatas.Find(material => material.lodLevel == lodLevel);
        
        return ((lodMesh != null && lodMat != null) ? (lodMesh.mesh, lodMat.material) : (null, null));
    }
}

public class ObjectData
{
    public Vector2 position;
    public FoliageMetaData metaData;

    public ObjectData(Vector2 position, FoliageMetaData metaData)
    {
        this.position = position;
        this.metaData = metaData;
    }
}
