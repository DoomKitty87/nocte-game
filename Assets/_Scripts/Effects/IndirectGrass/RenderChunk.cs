using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class RenderChunk {
  private readonly Vector3 _position;
  private Vector2 _centerPosition;
  private readonly Mesh[] _meshes;
  private readonly Material _material;
  private readonly float _chunkSize;
  private readonly int[] _chunkDensity;
  private readonly int[] _lodDistances;

  private readonly ComputeShader _positionCompute;

  private readonly uint[] _args = new uint[5];
  private ComputeBuffer _argsBuffer;

  private ComputeBuffer _positionsBuffer;
  private Texture2D _heightmapTexture;
  private Texture2D _growthTexture;

  private int _densityLevel;

  private static readonly int PositionBuffer = Shader.PropertyToID("_instancePositions");
  
  // chunkDensity is amount of blades in one direction
  // chunkSize is the size in one direction
  public RenderChunk(Vector2Int chunkPos, float chunkSize, int[] chunkDensity, Material grassMaterial, Mesh[] grassMeshes, int[] lodValues, ComputeShader positionCompute, Vector2 cameraPosition) {
    _position = new Vector3(chunkPos.x * chunkSize, 0, chunkPos.y * chunkSize);
    //Debug.Log(_position);
    _chunkSize = chunkSize;
    _chunkDensity = chunkDensity;
    _material = Material.Instantiate(grassMaterial);
    _meshes = grassMeshes;
    _lodDistances = lodValues;
    _positionCompute = positionCompute;

    int lod = 0;
    float distance = Vector2.Distance(cameraPosition, new Vector2(_position.x + _chunkSize / 2, _position.z + _chunkSize / 2));
    for (int i = 0; i < _lodDistances.Length; i++) {
      if (distance > _lodDistances[i]) {
        lod = i + 1;
      }
    }

    Initialize(lod);

    ComputePositions(lod);
    // Vector4[] arry = new Vector4[_chunkDensity * _chunkDensity];
    // _positionsBuffer.GetData(arry);
    // Debug.Log(arry[0]);
    _material.SetBuffer(PositionBuffer, _positionsBuffer);
    
  }
  
  private void UpdateDensity(int lod) {
    _positionsBuffer.Release();
    _positionsBuffer = new ComputeBuffer(_chunkDensity[lod] * _chunkDensity[lod], sizeof(float) * 4);
    ComputePositions(lod);
    _material.SetBuffer(PositionBuffer, _positionsBuffer);
  }

  private void ComputePositions(int lod) {

    int kernelIndex = _positionCompute.FindKernel("ComputePosition");
    _positionCompute.SetFloat(Shader.PropertyToID("_samples"), _chunkDensity[lod]);
    _positionCompute.SetFloat(Shader.PropertyToID("_size"), _chunkSize);
    _positionCompute.SetVector(Shader.PropertyToID("_chunkBottomLeftPosition"), _position);
    _positionCompute.SetInt(Shader.PropertyToID("_tileHeightmapTextureWidth"), _heightmapTexture.width);
    _positionCompute.SetFloat("_tileWidth", WorldGenInfo._tileEdgeSize);
    _positionCompute.SetVector("_tileBottomLeftPosition", new Vector3(Mathf.Floor(_position.x / WorldGenInfo._tileEdgeSize) * WorldGenInfo._tileEdgeSize, 0, Mathf.Floor(_position.z / WorldGenInfo._tileEdgeSize) * WorldGenInfo._tileEdgeSize));
    _positionCompute.SetTexture(kernelIndex, Shader.PropertyToID("_tileHeightmapTexture"), _heightmapTexture);
    _positionCompute.SetTexture(kernelIndex, Shader.PropertyToID("_tileGrowthTexture"), _growthTexture);
    _positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_positionOutputBuffer"), _positionsBuffer);
    _positionCompute.GetKernelThreadGroupSizes(kernelIndex, out uint threadX, out _, out _);
    _positionCompute.Dispatch(kernelIndex, Mathf.CeilToInt(_chunkDensity[lod] * _chunkDensity[lod] / (float)threadX), 1, 1);
  
  }


  private void Initialize(int lod) {
    _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    var tmp = WorldGenInfo._worldGenerator.GetHeightmapTexture(new Vector2(_position.x, _position.z));
    _heightmapTexture = tmp.Item1;
    _growthTexture = tmp.Item2;
    _centerPosition = new Vector2(_position.x + _chunkSize / 2, _position.z + _chunkSize / 2);

    _positionsBuffer = new ComputeBuffer(_chunkDensity[lod] * _chunkDensity[lod], sizeof(float) * 4);

  }


  private int _previousLOD = -1;
  public void Render(Vector2 cameraPosition) {

    float distance = Vector2.Distance(cameraPosition, _centerPosition);
    int lod = 0;
    
    for (int i = 0; i < _lodDistances.Length; i++) {
      if (distance > _lodDistances[i]) {
        lod = i + 1;
      }
    }

    if (lod != _previousLOD) {
      UpdateDensity(lod);

      _args[0] = (uint)_meshes[lod].GetIndexCount(0);
      _args[1] = (uint)(_chunkDensity[lod] * _chunkDensity[lod]);
      _args[2] = (uint)_meshes[lod].GetIndexStart(0);
      _args[3] = (uint)_meshes[lod].GetBaseVertex(0);
      _argsBuffer.SetData(_args);

      _previousLOD = lod;
    }
    _args[0] = (uint)_meshes[lod].GetIndexCount(0);
    _args[1] = (uint)(_chunkDensity[lod] * _chunkDensity[lod]);
    _args[2] = (uint)_meshes[lod].GetIndexStart(0);
    _args[3] = (uint)_meshes[lod].GetBaseVertex(0);
    _argsBuffer.SetData(_args);
    Graphics.DrawMeshInstancedIndirect(_meshes[lod], 0, _material, new Bounds(new Vector3(0, 0, 0), new Vector3(5000, 5000, 5000)), _argsBuffer);

  }

  public void CleanUp() {

    _positionsBuffer?.Release();
    _positionsBuffer = null;

    _argsBuffer?.Release();
    _argsBuffer = null;

  }
}