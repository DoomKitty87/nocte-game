using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRenderObjectsIndirect : MonoBehaviour
{
    public bool _initialized;
    
    public Mesh _instanceMesh;
    public Material _instanceMaterial;
    public int _length;
    public float _distance;

    private Vector4[] _positions;
    
    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private ComputeBuffer _argsBuffer;
    public int _count = 0;

    private ComputeBuffer _positionsBuffer;

    private static readonly int PositionBuffer = Shader.PropertyToID("position_buffer");
    
    private void Start() {
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        CreatePositions();
        
        UpdateBuffer();
    }
    
    public void Update() {
        Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
    }

    private void OnDisable() {
        _positionsBuffer?.Release();
        _positionsBuffer = null;

        _argsBuffer.Release();
        _argsBuffer = null;
    }

    private void CreatePositions() {
        int _count = _length * _length * _length;

        _positions = new Vector4[_count];

        int index = 0;
        for (int i = 0; i < _length; i++) {
            for (int j = 0; j < _length; j++) {
                for (int k = 0; k < _length; k++) {
                    _positions[index] = new Vector3(i * _distance, j * _distance, k * _distance);
                    index++;
                }
            }
        }
    }

    
    private void UpdateBuffer() {
        if (!_initialized) return;
        
        _positionsBuffer?.Release();
        _positionsBuffer = new ComputeBuffer(_count, 16);

        var positions4 = new Vector4[_count];
        for (int i = 0; i < _count; i++) positions4[i] = [i];
        
        _positionsBuffer.SetData(positions4);
        _instanceMaterial.SetBuffer(PositionBuffer, _positionsBuffer);

        _args[0] = _instanceMesh.GetIndexCount(0);
        _args[1] = (uint)_count;
        _args[2] = _instanceMesh.GetIndexStart(0);
        _args[3] = _instanceMesh.GetBaseVertex(0);
        
        _argsBuffer.SetData(_args);
    }
}
