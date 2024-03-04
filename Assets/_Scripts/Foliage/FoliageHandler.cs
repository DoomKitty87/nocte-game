using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoliageHandler : MonoBehaviour
{
    private Dictionary<Vector2Int, FoliageChunk> _chunkDict = new Dictionary<Vector2Int, FoliageChunk>();

    public FoliageScriptable[] _foliageScriptables;

    private Transform _cameraPosition;

    public float _chunkSize;
    
    private Vector2Int _middleChunk = new Vector2Int(0, 0); 
    private int _chunkDistance;

    private void Awake() {
        WorldGenerator.GenerationComplete += Initialize;
    }
    

    private void Start() {
        _foliageScriptables = Resources.LoadAll<FoliageScriptable>("FoliageObjects");

        // Finds the farthest LOD distance in all FoliageScriptable objects
        var farthestChunkDistance = _foliageScriptables.Select(scriptable => scriptable._lodRanges[^1].Distance).
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
                var chunk = new FoliageChunk();
                _chunkDict.Add(chunkPos, chunk);
            }
        }
        _initialized = true;
    }

    private void Update() {
        foreach (var chunk in _chunkDict.Values) chunk.Render();
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
            var chunk = new FoliageChunk();
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
            var chunk = new FoliageChunk();
            _chunkDict.Add(chunkPos, chunk);
            Debug.Log($"Added chunk {chunkPos}");
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
            var chunk = new FoliageChunk();
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
            var chunk = new FoliageChunk();
            _chunkDict.Add(chunkPos, chunk);
          }
          _middleChunk.y -= 1;
          break;
        }
      }
    }
}
