using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Foliage
{

  public static class FoliagePool
  {

    public static Dictionary<FoliageScriptable, List<GameObject>> _pool;
    public static List<(Vector2, Vector2)> _structureBounds = new List<(Vector2, Vector2)>();
    public static ComputeBuffer _boundsBuffer;

  }

  public class FoliageHandler : MonoBehaviour
  {
    private Dictionary<Vector2Int, FoliageChunk> _chunkDict = new Dictionary<Vector2Int, FoliageChunk>();
    private FoliageScriptable[] _scriptables;
    private Transform _cameraPosition;

    public float _chunkSize;

    private Vector2Int _middleChunk = new Vector2Int(0, 0);
    private int _chunkDistance;
    private Camera _camera;

    private void Awake() {
      WorldGenerator.GenerationComplete += Initialize;
      FoliagePool._pool = new Dictionary<FoliageScriptable, List<GameObject>>();
    }

    private void OnDisable() {
      WorldGenerator.GenerationComplete -= Initialize;
      WorldGenerator.PlayerMove -= UpdatePlayerPosition;
    }

    private void Start() {
      _scriptables = Resources.LoadAll<FoliageScriptable>("FoliageObjects");
      
      foreach (var scriptable in _scriptables) {
        FoliagePool._pool.Add(scriptable, new List<GameObject>());
      }

      // Finds the farthest LOD distance in all FoliageScriptable objects
      var farthestChunkDistance = _scriptables.Select(scriptable => scriptable._maxBillboardDistance).
        Prepend(0).Max();

      _chunkDistance = Mathf.FloorToInt(farthestChunkDistance / _chunkSize);
    }

    private bool _initialized = false;
    private void Initialize() {
      if (_initialized) return;

      FoliagePool._boundsBuffer = new ComputeBuffer(FoliagePool._structureBounds.Count, sizeof(float) * 4);
      Vector4[] bounds = new Vector4[FoliagePool._structureBounds.Count];
      for (var i = 0; i < FoliagePool._structureBounds.Count; i++) {
        bounds[i] = new Vector4(FoliagePool._structureBounds[i].Item1.x, FoliagePool._structureBounds[i].Item1.y, FoliagePool._structureBounds[i].Item2.x, FoliagePool._structureBounds[i].Item2.y);
      }
      FoliagePool._boundsBuffer.SetData(bounds);

      _cameraPosition = Camera.main.transform;
      _camera = Camera.main;
      _chunkDict.Clear();
      
      for (var i = -_chunkDistance; i <= _chunkDistance; i++) {
        for (var j = -_chunkDistance; j <= _chunkDistance; j++) {
          var chunkPos = new Vector2Int(i, j);
          var position = _cameraPosition.position;
          var chunk = new FoliageChunk(
            _scriptables,
            new Vector2Int(i, j),
            _chunkSize,
            new Vector2(position.x, position.z)
          );
          _chunkDict.Add(chunkPos, chunk);
        }
      }
      _initialized = true;
      WorldGenerator.PlayerMove += UpdatePlayerPosition;
    }

    private void Update() {
      foreach (var chunk in _chunkDict.Values) chunk.Render(new Vector2(_cameraPosition.position.x, _cameraPosition.position.z), _camera);
    }

    public void UpdatePlayerPosition(Vector2 playerPosition) {
      var moveDelta = new Vector2Int(Mathf.FloorToInt(playerPosition.x / _chunkSize) - _middleChunk.x, Mathf.FloorToInt(playerPosition.y / _chunkSize) - _middleChunk.y);
      if (moveDelta != Vector2Int.zero) {
        UpdateFoliage(moveDelta);
      }
    }
    
    // private void OnDrawGizmos() {
    //   Gizmos.color = Color.red;
    //   foreach (var chunk in _chunkDict.Values) {
    //     Gizmos.DrawWireCube(new Vector3(chunk._chunkCenter.x, 0, chunk._chunkCenter.y), new Vector3(_chunkSize, 0, _chunkSize));
    //   }
    // }

    private void UpdateFoliage(Vector2Int moveDelta) {
      switch (moveDelta.x) {
        case 1: {
          for (var i = -_chunkDistance; i <= _chunkDistance; i++) {
          
            _chunkDict[new Vector2Int( _middleChunk.x - _chunkDistance, _middleChunk.y + i)].CleanUp();
            _chunkDict.Remove(new Vector2Int( _middleChunk.x - _chunkDistance, _middleChunk.y + i));

            var chunkPos = new Vector2Int(_middleChunk.x + _chunkDistance + 1, _middleChunk.y + i);
            var position = _cameraPosition.position;
            var chunk = new FoliageChunk(
              _scriptables,
              chunkPos,
              _chunkSize,
              new Vector2(position.x, position.z)
            );           
            _chunkDict.Add(chunkPos, chunk);
          }
          _middleChunk.x += 1;
          break;
        }
        case -1: {
          for (var i = -_chunkDistance; i <= _chunkDistance; i++) {
          
            _chunkDict[new Vector2Int( _middleChunk.x + _chunkDistance, _middleChunk.y + i)].CleanUp();
            _chunkDict.Remove(new Vector2Int( _middleChunk.x + _chunkDistance, _middleChunk.y + i));

            var chunkPos = new Vector2Int(_middleChunk.x - _chunkDistance - 1, _middleChunk.y + i);
            var position = _cameraPosition.position;
            var chunk = new FoliageChunk(
              _scriptables,
              chunkPos,
              _chunkSize,
              new Vector2(position.x, position.z)
            );           
            _chunkDict.Add(chunkPos, chunk);
          }
          _middleChunk.x -= 1;
          break;
        }
      }

      switch (moveDelta.y) {
        case 1: {
          for (var i = -_chunkDistance; i <= _chunkDistance; i++) {
            _chunkDict[new Vector2Int(_middleChunk.x + i, _middleChunk.y - _chunkDistance)].CleanUp();
            _chunkDict.Remove(new Vector2Int(_middleChunk.x + i, _middleChunk.y - _chunkDistance));

            var chunkPos = new Vector2Int(_middleChunk.x + i, _middleChunk.y + _chunkDistance + 1);
            var position = _cameraPosition.position;
            var chunk = new FoliageChunk(
              _scriptables,
              chunkPos,
              _chunkSize,
              new Vector2(position.x, position.z)
            );           
            _chunkDict.Add(chunkPos, chunk);
          }
          _middleChunk.y += 1;
          break;
        }
        case -1: {
          for (var i = -_chunkDistance; i <= _chunkDistance; i++) {
          
            _chunkDict[new Vector2Int(_middleChunk.x + i, _middleChunk.y + _chunkDistance)].CleanUp();
            _chunkDict.Remove(new Vector2Int(_middleChunk.x + i, _middleChunk.y + _chunkDistance));

            var chunkPos = new Vector2Int(_middleChunk.x + i, _middleChunk.y - _chunkDistance - 1);
            var position = _cameraPosition.position;
            var chunk = new FoliageChunk(
              _scriptables,
              chunkPos,
              _chunkSize,
              new Vector2(position.x, position.z)
            );           
            _chunkDict.Add(chunkPos, chunk);
          }
          _middleChunk.y -= 1;
          break;
        }
      }
    }
  }
}
