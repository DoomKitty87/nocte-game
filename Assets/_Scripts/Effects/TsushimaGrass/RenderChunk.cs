using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class RenderChunk {
  private Vector3 _position;
  private Vector3 _centerPosition;
  private Mesh[] _meshes;
  private Material _material;
  private float _chunkSize;
  private int _chunkDensity;
  private int[] _lodDistances;

  private ComputeShader _positionCompute;

  private uint[] _args = new uint[5];
  private ComputeBuffer _argsBuffer;

  private GraphicsBuffer _positionsBuffer;
  private Texture2D _heightmapTexture;

  private static readonly int PositionBuffer = Shader.PropertyToID("_instancePositionMatrices");
  
  // chunkDensity is amount of blades in one direction
  // chunkSize is the size in one direction
  public RenderChunk(Vector2Int chunkPos, float chunkSize, int chunkDensity, Material grassMaterial, Mesh[] grassMeshes, int[] lodValues, ComputeShader positionCompute) {

    _position = new Vector3(chunkPos.x * chunkSize, 0, chunkPos.y * chunkSize);
    _chunkSize = chunkSize;
    _chunkDensity = chunkDensity;
    _material = grassMaterial;
    _meshes = grassMeshes;
    _lodDistances = lodValues;
    _positionCompute = positionCompute;

    Initialize();

    ComputePositions();

    _material.SetBuffer(PositionBuffer, _positionsBuffer);
    
  }

  private void ComputePositions() {

    int kernelIndex = _positionCompute.FindKernel("ComputePosition");
    _positionCompute.SetFloat(Shader.PropertyToID("_samples"), _chunkDensity);
    _positionCompute.SetFloat(Shader.PropertyToID("_size"), _chunkSize);
    _positionCompute.SetVector(Shader.PropertyToID("_chunkBottomLeftPosition"), _position);
    _positionCompute.SetInt(Shader.PropertyToID("_tileHeightmapTextureWidth"), _heightmapTexture.width);
    _positionCompute.SetTexture(kernelIndex, Shader.PropertyToID("_tileHeightmapTexture"), _heightmapTexture);
    _positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_positionOutputBuffer"), _positionsBuffer);
    _positionCompute.GetKernelThreadGroupSizes(kernelIndex, out uint threadX, out _, out _);
    _positionCompute.Dispatch(kernelIndex, Mathf.CeilToInt(_chunkDensity * _chunkDensity / (float)threadX), 1, 1);
  
  }


  private void Initialize() {
    _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    _heightmapTexture = WorldGenInfo._worldGenerator.GetHeightmapTexture(new Vector2(_position.x, _position.z));

    _centerPosition = new Vector3(_position.x + _chunkSize / 2, 0, _position.z + _chunkSize / 2);

    _positionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _chunkDensity * _chunkDensity, sizeof(float) * 3);

  }

  public void Render(Vector3 cameraPosition) {

    float distance = Vector3.Distance(cameraPosition, _centerPosition);
    int lod = 0;
    
    for (int i = 0; i < _lodDistances.Length; i++) {
      if (distance > _lodDistances[i]) {
        lod = i + 1;
      }
    }
    _args[0] = (uint)_meshes[lod].GetIndexCount(0);
    _args[1] = (uint)(_chunkDensity * _chunkDensity);
    _args[2] = (uint)_meshes[lod].GetIndexStart(0);
    _args[3] = (uint)_meshes[lod].GetBaseVertex(0);
    _argsBuffer.SetData(_args);
    Graphics.DrawMeshInstancedIndirect(_meshes[lod], 0, _material, new Bounds(_centerPosition, Vector3.one * _chunkSize), _argsBuffer);

  }

  public void CleanUp() {

    _positionsBuffer?.Release();
    _positionsBuffer = null;

    _argsBuffer?.Release();
    _argsBuffer = null;

  }
}