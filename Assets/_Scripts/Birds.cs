using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

public class Birds : MonoBehaviour
{

  private struct Bird
  {
    public GameObject obj;
    public Vector3 velocity;
  }

  [SerializeField] private GameObject _birdPrefab;
  [SerializeField] private int _birdCount;
  [SerializeField] private int _flockCount;
  [SerializeField] private float _yOffset;

  private Bird[,] _birds;

  private Vector3 _playerPosition;

  public void UpdatePlayerPosition(Vector3 playerPos) {
    _playerPosition = playerPos;
  }

  private void Start() {
    _birds = new Bird[_flockCount, _birdCount];
    for (int i = 0; i < _flockCount; i++) {
      for (int j = 0; j < _birdCount; j++) {
        _birds[i, j] = new Bird();
        _birds[i, j].obj = Instantiate(_birdPrefab);
        _birds[i, j].obj.transform.position = _playerPosition + Vector3.up * _yOffset;
        _birds[i, j].velocity = new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f).normalized;
      }
    }
  }
  
  private void FixedUpdate() {
    for (int i = 0; i < _flockCount; i++) {
      Vector3 center = new Vector3();
      for (int j = 0; j < _birdCount; j++) {
        center += _birds[i, j].obj.transform.position;
        Vector3 awayVector = Vector3.zero;
        Vector3 headingVector = Vector3.zero;
        for (int k = 0; k < _birdCount; k++) {
          awayVector -= _birds[i, k].obj.transform.position - _birds[i, j].obj.transform.position;
          headingVector += _birds[i, k].velocity;
        }
        headingVector.Normalize();
        awayVector.Normalize();
        _birds[i, j].velocity = Vector3.Lerp(_birds[i, j].velocity, headingVector, 0.5f);
        _birds[i, j].velocity = Vector3.Lerp(_birds[i, j].velocity, awayVector * 2, 0.8f);
      }
      center /= _birdCount;
      for (int j = 0; j < _birdCount; j++) {
        _birds[i, j].velocity = Vector3.Lerp(_birds[i, j].velocity, center - _birds[i, j].obj.transform.position, 0.3f);
        _birds[i, j].velocity = Vector3.Lerp(_birds[i, j].velocity, -_birds[i, j].obj.transform.position.normalized,
          Vector3.Distance(_birds[i, j].obj.transform.position, _playerPosition + Vector3.up * _yOffset) / 2500);
        _birds[i, j].obj.transform.position += _birds[i, j].velocity;
      }
    }
  }
  
}