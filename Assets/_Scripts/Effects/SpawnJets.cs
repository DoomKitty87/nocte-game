using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnJets : MonoBehaviour
{

  [SerializeField] private GameObject _jetPrefab;
  [SerializeField] private float _spawnRate;
  [SerializeField] private float _spawnRateVariance;
  [SerializeField] private float _jetHeight;
  [SerializeField] private float _jetHeightVariance;
  [SerializeField] private float _jetSpeed;
  [SerializeField] private float _jetSpeedVariance;
  [SerializeField] private float _jetAngleVariance = 90f;

  private float _timer;

  private float _rate;

  private void Start() {
    _rate = _spawnRate + (Random.Range(-1, 1) * _spawnRateVariance);;
  }

  void Update() {
    _timer += Time.deltaTime;
    if (_timer >= _rate) {
      _timer = 0;
      _rate = _spawnRate + (Random.Range(-1, 1) * _spawnRateVariance);;
      float theta = Random.Range(0, Mathf.PI * 2);
      Vector3 position = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * 20000;
      
      float height = _jetHeight + (Random.Range(-1, 1) * _jetHeightVariance);
      
      Quaternion lookDirection = Quaternion.LookRotation(-position, Vector3.up);
      float randomShift = Random.Range(-_jetAngleVariance, _jetAngleVariance);
      Quaternion randomizedDirection = Quaternion.Euler(0f, randomShift, 0f) * lookDirection;
      GameObject jet = Instantiate(_jetPrefab, position + Vector3.up * height, randomizedDirection);
      jet.transform.SetParent(transform);

      jet.GetComponent<ConstantMove>()._speed = _jetSpeed + (Random.Range(-1, 1) * _jetSpeedVariance);

    }
  }
}
