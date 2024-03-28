using System;
using System.Collections.Generic;
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
    private readonly Material _billboardMaterial;
    private readonly bool _useSubmesh;
    private readonly float _chunkSize;
    private readonly int[] _chunkDensity;
    private readonly int[] _lodDistances;
    private readonly float _billboardDistance;
    private readonly Mesh _billboardMesh;
    private readonly bool _useBillboard;

    private int _previousLOD;

    private readonly ComputeShader _positionCompute;
    private readonly ComputeShader _cullingCompute;

    private readonly uint[] _args = new uint[5];
    private readonly uint[] _args2 = new uint[5];
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _argsBuffer2;

    private ComputeBuffer _positionsBuffer;
    private Texture2D _heightmapTexture;
    private Texture2D _growthTexture;

    private List<GameObject> _activeColliders = new List<GameObject>();
  
    private FoliageScriptable _scriptable;
    private static readonly int PositionBuffer = Shader.PropertyToID("_instancePositions");

    public static Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 1000000000f);

    private uint _currentInstanceCount;

    private float _noiseScale;
    private int _noiseOctaves;
    private float _noisePersistence;
    private float _noiseLacunarity;
    private float _noiseOffset;
    private float _noiseCutoff;

    public FoliageRenderer(FoliageScriptable scriptable, Vector2Int chunkPos, float chunkSize, Vector2 cameraPosition) {
      _position = new Vector3(chunkPos.x * chunkSize, 0, chunkPos.y * chunkSize);
      _chunkSize = chunkSize;
      _scriptable = scriptable;
    
      var numberOfLODs = _scriptable._lodRanges.Length;

      _meshes = new Mesh[numberOfLODs];
      _lodDistances = new int[numberOfLODs];
      _chunkDensity = new int[numberOfLODs];

      _positionCompute = _scriptable._positionComputeShader;
      _cullingCompute = _scriptable._cullingComputeShader;

      _noiseScale = 1f / _scriptable._noiseScale;
      _noiseOctaves = _scriptable._noiseOctaves;
      _noisePersistence = _scriptable._noisePersistence;
      _noiseLacunarity = _scriptable._noiseLacunarity;
      _noiseOffset = WorldGenInfo._seed;
      _noiseCutoff = _scriptable._noiseCutoff;
    
      _material = new Material(_scriptable.Material);
      _useSubmesh = _scriptable._useSubmesh;
      if (_useSubmesh) _material2 = new Material(_scriptable.Material2);
      _useBillboard = _scriptable.UseBillboard;
      
      if (_useBillboard) {
        _billboardMaterial = new Material(_scriptable.BillboardMaterial);
        _billboardDistance = _scriptable._maxBillboardDistance;
        _billboardMesh = _scriptable.BillboardMesh;
      }

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

      if (distance > _lodDistances[_lodDistances.Length - 1] && distance < _billboardDistance) lod = -1;

      Initialize(lod);
    
      ComputePositions(lod);

      if (lod != -1) {
        _args[0] = (uint)_meshes[lod].GetIndexCount(0);
        _args[1] = _currentInstanceCount;
        _args[2] = (uint)_meshes[lod].GetIndexStart(0);
        _args[3] = (uint)_meshes[lod].GetBaseVertex(0);
        if (_useSubmesh) {
          _args2[0] = (uint)_meshes[lod].GetIndexCount(1);
          _args2[1] = _currentInstanceCount;
          _args2[2] = (uint)_meshes[lod].GetIndexStart(1);
          _args2[3] = (uint)_meshes[lod].GetBaseVertex(1);
          _argsBuffer2.SetData(_args2);
        }
        _argsBuffer.SetData(_args);
      }
      else if (_useBillboard) {
        _args[0] = (uint)_billboardMesh.GetIndexCount(0);
        _args[1] = _currentInstanceCount;
        _args[2] = (uint)_billboardMesh.GetIndexStart(0);
        _args[3] = (uint)_billboardMesh.GetBaseVertex(0);
        _argsBuffer.SetData(_args);
      }

      if (lod == 0) UpdateColliders(lod);
    
      _material.SetBuffer(PositionBuffer, _positionsBuffer);
      if (_useBillboard) _billboardMaterial.SetBuffer(PositionBuffer, _positionsBuffer);
      if (_useSubmesh) _material2.SetBuffer(PositionBuffer, _positionsBuffer);
    }

    private void UpdateDensity(int lod) {
      if (lod == -1) lod = _chunkDensity.Length - 1;
      _positionsBuffer?.Release();
      _positionsBuffer = new ComputeBuffer(_chunkDensity[lod] * _chunkDensity[lod], sizeof(float) * 4);
      ComputePositions(lod);
      _material.SetBuffer(PositionBuffer, _positionsBuffer);
      if (_useBillboard) _billboardMaterial.SetBuffer(PositionBuffer, _positionsBuffer);
      if (_useSubmesh) _material2.SetBuffer(PositionBuffer, _positionsBuffer);
    }
    
    private void ComputePositions(int lod) {
      if (lod == -1) lod = _chunkDensity.Length - 1;
      var kernelIndex = _positionCompute.FindKernel("ComputePosition");
      _positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_boundsBuffer"), FoliagePool._boundsBuffer);
      _positionCompute.SetInt(Shader.PropertyToID("_structureBoundsCount"), FoliagePool._structureBounds.Count);
      _positionCompute.SetFloat(Shader.PropertyToID("_samples"), _chunkDensity[lod]);
      _positionCompute.SetFloat(Shader.PropertyToID("_size"), _chunkSize);
      _positionCompute.SetVector(Shader.PropertyToID("_chunkBottomLeftPosition"), _position);
      _positionCompute.SetInt(Shader.PropertyToID("_tileHeightmapTextureWidth"), _heightmapTexture.width);
      _positionCompute.SetFloat("_tileWidth", WorldGenInfo._tileEdgeSize);
      _positionCompute.SetVector("_tileBottomLeftPosition", new Vector3(Mathf.Floor(_position.x / WorldGenInfo._tileEdgeSize) * WorldGenInfo._tileEdgeSize, 0, Mathf.Floor(_position.z / WorldGenInfo._tileEdgeSize) * WorldGenInfo._tileEdgeSize));
      _positionCompute.SetTexture(kernelIndex, Shader.PropertyToID("_tileHeightmapTexture"), _heightmapTexture);
      _positionCompute.SetTexture(kernelIndex, Shader.PropertyToID("_tileGrowthTexture"), _growthTexture);
      _positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_positionOutputBuffer"), _positionsBuffer);
      _positionCompute.SetFloat("_noiseScale", _noiseScale);
      _positionCompute.SetInt("_noiseOctaves", _noiseOctaves);
      _positionCompute.SetFloat("_noisePersistence", _noisePersistence);
      _positionCompute.SetFloat("_noiseLacunarity", _noiseLacunarity);
      _positionCompute.SetFloat("_noiseOffset", _noiseOffset);
      _positionCompute.SetFloat("_noiseCutoff", _noiseCutoff);
      _positionCompute.GetKernelThreadGroupSizes(kernelIndex, out uint threadX, out _, out _);
      _positionCompute.Dispatch(kernelIndex, Mathf.CeilToInt(_chunkDensity[lod] * _chunkDensity[lod] / (float)threadX), 1, 1);
    
      var voteBuffer = new ComputeBuffer(_chunkDensity[lod] * _chunkDensity[lod], sizeof(uint));
      var sumBuffer = new ComputeBuffer(_chunkDensity[lod] * _chunkDensity[lod], sizeof(uint));
      var culledCountBuffer = new ComputeBuffer(1, sizeof(uint));

      _cullingCompute.SetInt(Shader.PropertyToID("_inputCount"), _chunkDensity[lod] * _chunkDensity[lod]);

      var kernelIndexVote = _cullingCompute.FindKernel("Vote");
      _cullingCompute.SetBuffer(kernelIndexVote, Shader.PropertyToID("_inputBuffer"), _positionsBuffer);
      _cullingCompute.SetBuffer(kernelIndexVote, Shader.PropertyToID("_voteBuffer"), voteBuffer);
      _cullingCompute.GetKernelThreadGroupSizes(kernelIndexVote, out threadX, out _, out _);
      _cullingCompute.Dispatch(kernelIndexVote, Mathf.CeilToInt(_chunkDensity[lod] * _chunkDensity[lod] / (float)threadX), 1, 1);

      var kernelIndexSum = _cullingCompute.FindKernel("Sum");
      _cullingCompute.SetBuffer(kernelIndexSum, Shader.PropertyToID("_voteBuffer"), voteBuffer);
      _cullingCompute.SetBuffer(kernelIndexSum, Shader.PropertyToID("_sumBuffer"), sumBuffer);
      _cullingCompute.GetKernelThreadGroupSizes(kernelIndexSum, out threadX, out _, out _);
      _cullingCompute.Dispatch(kernelIndexSum, Mathf.CeilToInt(_chunkDensity[lod] * _chunkDensity[lod] / (float)threadX), 1, 1);

      var kernelIndexCompact = _cullingCompute.FindKernel("Compact");
      _cullingCompute.SetBuffer(kernelIndexCompact, Shader.PropertyToID("_inputBuffer"), _positionsBuffer);
      _cullingCompute.SetBuffer(kernelIndexCompact, Shader.PropertyToID("_sumBuffer"), sumBuffer);      
      _cullingCompute.SetBuffer(kernelIndexCompact, Shader.PropertyToID("_voteBuffer"), voteBuffer);
      _cullingCompute.SetBuffer(kernelIndexCompact, Shader.PropertyToID("_culledCount"), culledCountBuffer);
      _cullingCompute.GetKernelThreadGroupSizes(kernelIndexCompact, out threadX, out _, out _);
      _cullingCompute.Dispatch(kernelIndexCompact, Mathf.CeilToInt(_chunkDensity[lod] * _chunkDensity[lod] / (float)threadX), 1, 1);

      var culledCount = new uint[1];
      culledCountBuffer.GetData(culledCount);
      _currentInstanceCount = culledCount[0];
      // Debug.Log(_currentInstanceCount);

      voteBuffer.Release();
      sumBuffer.Release();
      culledCountBuffer.Release();
    }
    
    private void Initialize(int lod) {
      _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
      _argsBuffer2 = new ComputeBuffer(1, _args2.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
      var textures = WorldGenInfo._worldGenerator.GetHeightmapTexture(new Vector2(_position.x, _position.z));
      _heightmapTexture = textures.Item1;
      _growthTexture = textures.Item2;
      _centerPosition = new Vector2(_position.x + _chunkSize / 2, _position.z + _chunkSize / 2);

      _positionsBuffer = new ComputeBuffer(_chunkDensity[lod == -1 ? ^1 : lod] * _chunkDensity[lod == -1 ? ^1 : lod], sizeof(float) * 4);
      
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

      if (distance > _lodDistances[_lodDistances.Length - 1]) lod = -1;
      if (lod == -1 && !_useBillboard) return;

      if (lod != _previousLOD) {
          if (lod != -1 && _previousLOD != -1) UpdateDensity(lod);

          if (lod != -1) {
            _args[0] = (uint)_meshes[lod].GetIndexCount(0);
            _args[1] = _currentInstanceCount;
            _args[2] = (uint)_meshes[lod].GetIndexStart(0);
            _args[3] = (uint)_meshes[lod].GetBaseVertex(0);
            _argsBuffer.SetData(_args);

            if (_useSubmesh) {
              _args2[0] = (uint)_meshes[lod].GetIndexCount(1);
              _args2[1] = _currentInstanceCount;
              _args2[2] = (uint)_meshes[lod].GetIndexStart(1);
              _args2[3] = (uint)_meshes[lod].GetBaseVertex(1);
              _argsBuffer2.SetData(_args2);
            }
          }
          else if (_useBillboard) {
            _args[0] = (uint)_billboardMesh.GetIndexCount(0);
            _args[1] = _currentInstanceCount;
            _args[2] = (uint)_billboardMesh.GetIndexStart(0);
            _args[3] = (uint)_billboardMesh.GetBaseVertex(0);
            _argsBuffer.SetData(_args);
          }

          if (lod == 0) UpdateColliders(lod);
          else if (_previousLOD == 0) UpdateColliders(lod);

          _previousLOD = lod;
      }

      //Debug.Log(lod);
      if (lod != -1) {
        Graphics.DrawMeshInstancedIndirect(_meshes[lod], 0, _material, _bounds, _argsBuffer);
        if (_useSubmesh) {
          Graphics.DrawMeshInstancedIndirect(_meshes[lod], 1, _material2, _bounds, _argsBuffer2);
        }
      }
      else if (_useBillboard) {
        Graphics.DrawMeshInstancedIndirect(_billboardMesh, 0, _billboardMaterial, _bounds, _argsBuffer);
      }

      //Debug.Log($"Rendering mesh");
      
    }

    private void UpdateColliders(int lod) {
      if (lod == 0) {
        Vector4[] data = new Vector4[_currentInstanceCount];
        _positionsBuffer.GetData(data);
        //Debug.Log(data[0]);
        foreach (Vector4 v in data) {
          if (FoliagePool._pool[_scriptable].Count == 0) {
            var collider = GameObject.Instantiate(_scriptable.ColliderPrefab, new Vector3(v.x, v.y, v.z), Quaternion.identity);
            collider.transform.parent = FoliageHandler.Instance;
            _activeColliders.Add(collider);
          }
          else {
            var collider = FoliagePool._pool[_scriptable][0];
            FoliagePool._pool[_scriptable].RemoveAt(0);
            collider.transform.position = new Vector3(v.x, v.y, v.z);
            _activeColliders.Add(collider);
          }
        }
      }
      else {
        foreach (GameObject g in _activeColliders) {
          FoliagePool._pool[_scriptable].Add(g);
        }
        _activeColliders.Clear();
      }
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
