using UnityEngine;
using System.Collections.Generic;

public class GrassDrawIndirect : MonoBehaviour
{
  [Header("Dependencies")]
  [SerializeField] private ComputeShader _positionCompute;
  [SerializeField] private Material _grassMaterial;
  
  [SerializeField] private Mesh[] _grassMeshes;
  [SerializeField] private int[] _lodValues;

  [SerializeField] private int _chunkCount;
  [SerializeField] private float _chunkSize;
  [SerializeField] private int[] _grassDensity;

  private Dictionary<Vector2Int, RenderChunk> _chunkDict = new Dictionary<Vector2Int, RenderChunk>();
  private Vector2Int _middleChunk = new Vector2Int(0, 0);
  private Transform _cameraPosition;
  private Camera _camera;

  private void Awake() {
    if (_chunkCount % 2 == 0) _chunkCount++;

    WorldGenerator.GenerationComplete += Initialize;
  }

  private void OnDisable() {
    WorldGenerator.GenerationComplete -= Initialize;
    WorldGenerator.PlayerMove -= UpdatePlayerPosition;
  }

  public void Initialize() {
    _cameraPosition = Camera.main.transform;
    _camera = Camera.main;
    _chunkDict.Clear();
    for (int i = -(_chunkCount - 1) / 2; i < (_chunkCount - 1) / 2 + 1; i++) {
      for (int j = -(_chunkCount - 1) / 2; j < (_chunkCount - 1) / 2 + 1; j++) {
        //Debug.Log($"{i}, {j}");
        Vector2Int chunkPos = new Vector2Int(i, j);
        //Debug.Log(chunkPos);
        RenderChunk chunk = new RenderChunk(chunkPos, _chunkSize, _grassDensity, _grassMaterial, _grassMeshes, _lodValues, _positionCompute, new Vector2(_cameraPosition.position.x, _cameraPosition.position.z));
        _chunkDict.Add(chunkPos, chunk);
      }
    }
    //Debug.Log(_chunkDict.Count);
  }

  private void Update() {
    foreach (var chunk in _chunkDict.Values) {
      chunk.Render(new Vector2(_cameraPosition.position.x, _cameraPosition.position.z), _camera);
    }
  }

  public void UpdatePlayerPosition(Vector2 playerPosition) {
    Vector2Int moveDelta = new Vector2Int(Mathf.FloorToInt(playerPosition.x / _chunkSize) - _middleChunk.x, Mathf.FloorToInt(playerPosition.y / _chunkSize) - _middleChunk.y);
    moveDelta.x = Mathf.Clamp(moveDelta.x, -1, 1);
    moveDelta.y = Mathf.Clamp(moveDelta.y, -1, 1);
    if (moveDelta != Vector2Int.zero) {
      UpdateGrass(moveDelta);
    }
  }

  public void UpdateGrass(Vector2Int moveDelta) {
    //Debug.Log(_middleChunk);
    //Debug.Log(_chunkDict[new Vector2Int(_middleChunk.x, _middleChunk.y)]);

    if (moveDelta.x == 1) {
      for (int i = -(_chunkCount - 1) / 2; i < (_chunkCount - 1) / 2 + 1; i++) {
        _chunkDict[new Vector2Int( _middleChunk.x - (_chunkCount - 1) / 2, _middleChunk.y + i)].CleanUp();
        _chunkDict.Remove(new Vector2Int(_middleChunk.x - _chunkCount / 2, _middleChunk.y + i));

        Vector2Int chunkPos = new Vector2Int(_middleChunk.x + (_chunkCount - 1) / 2 + 1, _middleChunk.y + i);
        RenderChunk chunk = new RenderChunk(chunkPos, _chunkSize, _grassDensity, _grassMaterial, _grassMeshes, _lodValues, _positionCompute, new Vector2(_cameraPosition.position.x, _cameraPosition.position.z));
        _chunkDict.Add(chunkPos, chunk);
      }
      _middleChunk.x += 1;
    }
    else if (moveDelta.x == -1) {
      for (int i = -(_chunkCount - 1) / 2; i < (_chunkCount - 1) / 2 + 1; i++) {
        _chunkDict[new Vector2Int(_middleChunk.x + (_chunkCount - 1) / 2, _middleChunk.y + i)].CleanUp();
        _chunkDict.Remove(new Vector2Int(_middleChunk.x + (_chunkCount - 1) / 2, _middleChunk.y + i));

        Vector2Int chunkPos = new Vector2Int(_middleChunk.x - (_chunkCount - 1) / 2 - 1, _middleChunk.y + i);
        RenderChunk chunk = new RenderChunk(chunkPos, _chunkSize, _grassDensity, _grassMaterial, _grassMeshes, _lodValues, _positionCompute, new Vector2(_cameraPosition.position.x, _cameraPosition.position.z));
        _chunkDict.Add(chunkPos, chunk);
      }
      _middleChunk.x -= 1;
    }
    if (moveDelta.y == 1) {
      for (int i = -(_chunkCount - 1) / 2; i < (_chunkCount - 1) / 2 + 1; i++) {
        _chunkDict[new Vector2Int(_middleChunk.x + i, _middleChunk.y - (_chunkCount - 1) / 2)].CleanUp();
        _chunkDict.Remove(new Vector2Int(_middleChunk.x + i, _middleChunk.y - (_chunkCount - 1) / 2));

        Vector2Int chunkPos = new Vector2Int(_middleChunk.x + i, _middleChunk.y + (_chunkCount - 1) / 2 + 1);
        RenderChunk chunk = new RenderChunk(chunkPos, _chunkSize, _grassDensity, _grassMaterial, _grassMeshes, _lodValues, _positionCompute, new Vector2(_cameraPosition.position.x, _cameraPosition.position.z));
        _chunkDict.Add(chunkPos, chunk);
      }
      _middleChunk.y += 1;
    }
    else if (moveDelta.y == -1) {
      for (int i = -(_chunkCount - 1) / 2; i < (_chunkCount - 1) / 2 + 1; i++) {
        _chunkDict[new Vector2Int(_middleChunk.x + i, _middleChunk.y + (_chunkCount - 1) / 2)].CleanUp();
        _chunkDict.Remove(new Vector2Int(_middleChunk.x + i, _middleChunk.y + (_chunkCount - 1) / 2));

        Vector2Int chunkPos = new Vector2Int(_middleChunk.x + i, _middleChunk.y - (_chunkCount - 1) / 2 - 1);
        RenderChunk chunk = new RenderChunk(chunkPos, _chunkSize, _grassDensity, _grassMaterial, _grassMeshes, _lodValues, _positionCompute, new Vector2(_cameraPosition.position.x, _cameraPosition.position.z));
        _chunkDict.Add(chunkPos, chunk);
      }
      _middleChunk.y -= 1;
    }

  }

}