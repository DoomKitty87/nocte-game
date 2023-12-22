using System;
using UnityEngine;

public class RenderGrass : MonoBehaviour
{
    #region Exposed Variables
    
    [Header("Chunks")]
    [Tooltip("Length of one side of each chunk.")]
    [SerializeField, Min(0.1f), Delayed] private float _chunkSize = 10f;
    [Tooltip("Distance from player in any given direction. Equal to (2 * NumberOfChunksRadius - 1)^2.")]
    [SerializeField, Min(1), Delayed] private int _numberOfChunksRadius = 3;
    [Tooltip("Number of grass blades per chunk.")]
    [SerializeField, Min(0), Delayed] private int _grassBladesPerChunk = 1000;
    
    [Header("Grass")]
    [Tooltip("Mesh to be rendered.")]
    [SerializeField] private Mesh _mesh;
    [Tooltip("Material to be applied to mesh. Must be custom material with correct properties.")]
    [SerializeField] private Material _material;

    [Header("Target")] 
    [Tooltip("Object that grass spawns around. Most likely set to player or active camera.")]
    [SerializeField] private Transform _target;
    
    #endregion
    
    #region Local Variables
    
    private Matrix4x4[] _matrices;
    
    private GraphicsBuffer _commandBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;

    private ComputeShader _initializeGrassShader;
    
    #endregion
    
    #region Structs

    private struct GrassData {
        public Vector4 position;
        public Vector2 uv;
        public float displacement;
    }

    public struct GrassChunk {
        
    }
    
    #endregion
    
    #region Setup
    private void OnEnable() {
        _initializeGrassShader = Resources.Load<ComputeShader>("GrassPositions");
    }

    void OnDisable()
    {
        _commandBuffer?.Release();
        _commandBuffer = null;
    }
    #endregion

    private void SetUpGrass() {
        _commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
    }

    // private void RenderObjects() {
    //     RenderParams rp = new RenderParams(_material);
    //     rp.worldBounds = new Bounds(Vector3.zero, new Vector3(_count * _distance / 2, 5, _count * _distance / 2));
    //     rp.matProps = new MaterialPropertyBlock();
    //     
    //     Vector3 position = new Vector3(transform.position.x - ((_count - 1) * _distance / 2), 2, transform.position.z - ((_count - 1) * _distance / 2));
    //     rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(position));
    //     rp.matProps.SetInt("_NumberOfInstances", (int)_count);
    //     rp.matProps.SetFloat("_Distance", _distance);
    //     _commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
    //     _commandData[0].instanceCount = _count * _count;
    //     
    //     _commandBuffer.SetData(_commandData);
    //     Graphics.RenderMeshIndirect(rp, mesh, _commandBuffer);
    // }
}
