using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Foliage
{
  public class FoliageHandler : MonoBehaviour
  {
    public ComputeShader _positionCompute;
    
    private Dictionary<Vector2Int, FoliageChunk> _chunkDict = new Dictionary<Vector2Int, FoliageChunk>();
    private FoliageScriptable[] _scriptables;
    private Transform _cameraPosition;

    public float _chunkSize;
    
    private Vector2Int _middleChunk = new Vector2Int(0, 0); 
    private int _chunkDistance;

    private void Awake() {
      WorldGenerator.GenerationComplete += Initialize;
    }
    

    private void Start() {
      _scriptables = Resources.LoadAll<FoliageScriptable>("FoliageObjects");

      // Finds the farthest LOD distance in all FoliageScriptable objects
      var farthestChunkDistance = _scriptables.Select(scriptable => scriptable._lodRanges[^1].Distance).
        Prepend(0).Max();

      _chunkDistance = farthestChunkDistance;
    }

    private bool _initialized;
    private void Initialize() {
      if (_initialized) return;
      _cameraPosition = Camera.main.transform;
      _chunkDict.Clear();

        
      for (var i = -_chunkDistance; i <= _chunkDistance; i++) {
        for (var j = -_chunkDistance; j <= _chunkDistance; j++) {
          var chunkPos = new Vector2Int(i, j);
          var position = _cameraPosition.position;
          var chunk = new FoliageChunk(
            _scriptables, 
            new Vector2Int(i, j), 
            _chunkSize, 
            _positionCompute, 
            new Vector2(position.x, position.z)
          );
          _chunkDict.Add(chunkPos, chunk);
        }
      }
      _initialized = true;
    }

    private void Update() {
      foreach (var chunk in _chunkDict.Values) chunk.Render(new Vector2(_cameraPosition.position.x, _cameraPosition.position.z));
    }
    
    public void UpdatePlayerPosition(Vector2 playerPosition) {
      var moveDelta = new Vector2Int(Mathf.FloorToInt(playerPosition.x / _chunkSize) - _middleChunk.x, Mathf.FloorToInt(playerPosition.y / _chunkSize) - _middleChunk.y);
      if (moveDelta != Vector2Int.zero) {
        UpdateFoliage(moveDelta);
      }
    }

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
              _positionCompute, 
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
              _positionCompute, 
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
              _positionCompute, 
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
              _positionCompute, 
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
