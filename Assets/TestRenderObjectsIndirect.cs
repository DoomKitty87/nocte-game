using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRenderObjectsIndirect : MonoBehaviour
{    
    
    public Mesh _instanceMesh;
    public Mesh _secondInstancedMesh;
    public Material _instanceMaterial;
    public Material _secondinstancedMaterial;

    public Vector3 _firstPosition;
    public Vector3 _secondPosition;
    
    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private readonly uint[] _secondArgs = { 0, 0, 0, 0, 0 };
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _secondArgsBuffer;

    private ComputeBuffer _positionsBuffer;
    private ComputeBuffer _secondPositionsBuffer;


    private static readonly int PositionBuffer = Shader.PropertyToID("position_buffer");

    public void Start() {
        Initialize();
    }
    
    private void Initialize() {
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments, 0);
    }
    
    public void Update() {
        Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
        
        Graphics.DrawMeshInstancedIndirect(_secondInstancedMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);

    }
    
    private void OnDisable() {
        _positionsBuffer?.Release();
        _positionsBuffer = null;

        _argsBuffer.Release();
        _argsBuffer = null;
    }

    public void UpdateBuffer() {
        _positionsBuffer?.Release();
        _positionsBuffer = new ComputeBuffer(1, 16);
        
        _secondPositionsBuffer?.Release();
        _secondPositionsBuffer = new ComputeBuffer(1, 16);

        Vector4[] firstPositions4 = new Vector4[1];
        firstPositions4[0] = new Vector4(_firstPosition.x, _firstPosition.y, _firstPosition.z);
        
        Vector4[] secondPositions4 = new Vector4[1];
        secondPositions4[0] = new Vector4(_secondPosition.x, _secondPosition.y, _secondPosition.z);
        
        _positionsBuffer.SetData(firstPositions4);
        _instanceMaterial.SetBuffer(PositionBuffer, _positionsBuffer);
        
        _secondPositionsBuffer.SetData(secondPositions4);
        _secondinstancedMaterial.SetBuffer(PositionBuffer, _secondPositionsBuffer);

        _args[0] = _instanceMesh.GetIndexCount(0);
        _args[1] = (uint)1;
        _args[2] = _instanceMesh.GetIndexStart(0);
        _args[3] = _instanceMesh.GetBaseVertex(0);
        
        _argsBuffer.SetData(_args);
        
        _secondArgs[0] = _secondInstancedMesh.GetIndexCount(0);
        _secondArgs[1] = (uint)1;
        _secondArgs[2] = _secondInstancedMesh.GetIndexStart(0);
        _secondArgs[3] = _secondInstancedMesh.GetBaseVertex(0);
        
        _secondArgsBuffer.SetData(_secondArgs);
    }
}
