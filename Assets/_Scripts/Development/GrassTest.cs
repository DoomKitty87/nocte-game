using UnityEngine;
using System.Collections.Generic;

public class GrassTest : MonoBehaviour
{
  public Material _material;
  public Mesh _mesh;

  GraphicsBuffer _commandBuf;
  GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;
  private ComputeBuffer _positionsBuffer;
  const int _commandCount = 2;
  private float[] _positions;

  void Start()
  {
    _commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, _commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
    _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[_commandCount];
    _positions = new float[] {0, 0, 0, 1, 1, 1, 2, 2, 2, 2, 1, 3, 4, 1, 6}
    ;
    _positionsBuffer = new ComputeBuffer(_positions.Length, sizeof(float), ComputeBufferType.Default);
    _positionsBuffer.SetData(_positions);
  }

  void OnDestroy()
  {
    _positionsBuffer?.Release();
    _commandBuf?.Release();
    _commandBuf = null;
    _positionsBuffer = null;
  }

  void Update()
  {
    RenderParams rp = new RenderParams(_material);
    rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds for better FOV culling
    rp.matProps = new MaterialPropertyBlock();
    rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(0, 0, 0)));
    rp.matProps.SetBuffer("_PositionsBuffer", _positionsBuffer);
    _commandData[0].indexCountPerInstance = _mesh.GetIndexCount(0);
    _commandData[0].instanceCount = (uint) _positions.Length / 3;
    _commandData[1].indexCountPerInstance = _mesh.GetIndexCount(0);
    _commandData[1].instanceCount = (uint) _positions.Length / 3;
    _commandBuf.SetData(_commandData);
    Graphics.RenderMeshIndirect(rp, _mesh, _commandBuf, _commandCount);
  }
}