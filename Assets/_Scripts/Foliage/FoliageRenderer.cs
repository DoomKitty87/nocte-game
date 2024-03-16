using UnityEngine;

namespace Foliage
{
  public class FoliageRenderer
  {
    private Vector3 _position;
    private Vector2 _centerPosition;
    private readonly Mesh[] _meshes;
    private readonly Material _material;
    private readonly Material _material2;
    private readonly bool _useSubmesh;
    private readonly float _chunkSize;
    private readonly int[] _chunkDensity;
    private readonly int[] _lodDistances;

    private int _previousLOD;

    private readonly ComputeShader _positionCompute;

    private readonly uint[] _args = new uint[5];
    private readonly uint[] _args2 = new uint[5];
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _argsBuffer2;

    private ComputeBuffer _positionsBuffer;
    private Texture2D _heightmapTexture;
    private Texture2D _growthTexture;
  
    private FoliageScriptable _scriptable;
    private static readonly int PositionBuffer = Shader.PropertyToID("_instancePositions");

    public FoliageRenderer(FoliageScriptable scriptable, Vector2Int chunkPos, float chunkSize, Vector2 cameraPosition) {
      _position = new Vector3(chunkPos.x * chunkSize, 0, chunkPos.y * chunkSize);
      _chunkSize = chunkSize;
      _scriptable = scriptable;
    
      var numberOfLODs = _scriptable._lodRanges.Length;

      _meshes = new Mesh[numberOfLODs];
      _lodDistances = new int[numberOfLODs];
      _chunkDensity = new int[numberOfLODs];

      _positionCompute = _scriptable._positionComputeShader;
    
      _material = new Material(_scriptable.Material);
      _useSubmesh = _scriptable._useSubmesh;
      if (_useSubmesh) _material2 = new Material(_scriptable.Material2);

      var lodDatas = _scriptable._lodRanges;
    
      for (var i = 0; i < numberOfLODs; i++) {
          _meshes[i] = lodDatas[i].Mesh;
          _chunkDensity[i] = lodDatas[i].Density;
          _lodDistances[i] = (int) (lodDatas[i].Distance * chunkSize);
      }

      var lod = 0;
      var distance = Vector2.Distance(
          cameraPosition,
          new Vector2(_position.x + _chunkSize / 2, _position.z + _chunkSize / 2)
      );
      for (int i = 0; i < _lodDistances.Length - 1; i++) {
          if (distance > _lodDistances[i]) lod = i + 1;
      }

      Initialize(lod);
    
      ComputePositions(lod);
    
      _material.SetBuffer(PositionBuffer, _positionsBuffer);
      if (_useSubmesh) _material2.SetBuffer(PositionBuffer, _positionsBuffer);
    }

    private void UpdateDensity(int lod) {
      _positionsBuffer?.Release();
      _positionsBuffer = new ComputeBuffer(_chunkDensity[lod] * _chunkDensity[lod], sizeof(float) * 4);
      ComputePositions(lod);
      _material.SetBuffer(PositionBuffer, _positionsBuffer);
      if (_useSubmesh) _material2.SetBuffer(PositionBuffer, _positionsBuffer);
    }
    
    private void ComputePositions(int lod) {

      var kernelIndex = _positionCompute.FindKernel("ComputePosition");
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
      var textures = WorldGenInfo._worldGenerator.GetHeightmapTexture(new Vector2(_position.x, _position.z));
      _heightmapTexture = textures.Item1;
      _growthTexture = textures.Item2;
      _centerPosition = new Vector2(_position.x + _chunkSize / 2, _position.z + _chunkSize / 2);

      _positionsBuffer = new ComputeBuffer(_chunkDensity[lod] * _chunkDensity[lod], sizeof(float) * 4);

      _args[0] = (uint)_meshes[lod].GetIndexCount(0);
      _args[1] = (uint)(_chunkDensity[lod] * _chunkDensity[lod]);
      _args[2] = (uint)_meshes[lod].GetIndexStart(0);
      _args[3] = (uint)_meshes[lod].GetBaseVertex(0);
      if (_useSubmesh) {
        _args2[0] = (uint)_meshes[lod].GetIndexCount(1);
        _args2[1] = (uint)(_chunkDensity[lod] * _chunkDensity[lod]);
        _args2[2] = (uint)_meshes[lod].GetIndexStart(1);
        _args2[3] = (uint)_meshes[lod].GetBaseVertex(1);
        _argsBuffer2 = new ComputeBuffer(1, _args2.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _argsBuffer2.SetData(_args2);
      }
      _argsBuffer.SetData(_args);
      _previousLOD = lod;
    }

    public void Render(Vector2 cameraPosition) {

      //Debug.Log("Rendering");
      float distance = Vector2.Distance(cameraPosition, _centerPosition);
      int lod = 0;

      for (int i = 0; i < _lodDistances.Length - 1; i++) {
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

          if (_useSubmesh) {
            _args2[0] = (uint)_meshes[lod].GetIndexCount(1);
            _args2[1] = (uint)(_chunkDensity[lod] * _chunkDensity[lod]);
            _args2[2] = (uint)_meshes[lod].GetIndexStart(1);
            _args2[3] = (uint)_meshes[lod].GetBaseVertex(1);
            _argsBuffer2.SetData(_args2);
          }

          _previousLOD = lod;
      }

      //Debug.Log(lod);
      Graphics.DrawMeshInstancedIndirect(_meshes[lod], 0, _material, new Bounds(new Vector3(0, 0, 0), Vector3.one * 10000f), _argsBuffer);
      if (_useSubmesh) {
        Graphics.DrawMeshInstancedIndirect(_meshes[lod], 1, _material2, new Bounds(new Vector3(0, 0, 0), Vector3.one * 10000f), _argsBuffer2);
      }
      //Debug.Log($"Rendering mesh");
      
    }

    public void CleanUp() {

      _positionsBuffer?.Release();
      _positionsBuffer = null;

      _argsBuffer?.Release();
      _argsBuffer = null;

      _argsBuffer2?.Release();
      _argsBuffer2 = null;

    }
  }
}
