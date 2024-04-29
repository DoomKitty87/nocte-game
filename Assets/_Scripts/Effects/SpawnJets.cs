using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnJets : MonoBehaviour
{

  [SerializeField] private GameObject _jetPrefab;
  [SerializeField] private float _spawnRate;
  [SerializeField] [Range(0, 1)] private float _spawnChance;
  [SerializeField] private float _jetHeight;
  [SerializeField] private float _jetHeightVariance;
  [SerializeField] private float _jetSpeed;
  [SerializeField] private float _jetSpeedVariance;

  private float _timer;

  void Update() {
    _timer += Time.deltaTime;
    if (_timer >= _spawnRate) {
      _timer = 0;
      if (Random.value > _spawnChance) return;
      float theta = Random.Range(0, Mathf.PI * 2);
      Vector3 position = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * 20000;
      
      float height = _jetHeight + (Random.Range(-1, 1) * _jetHeightVariance);
      
      Quaternion lookDirection = Quaternion.LookRotation(-position, Vector3.up);
      float randomShift = Random.Range(-90f, 90f);
      Quaternion randomizedDirection = Quaternion.Euler(0f, randomShift, 0f) * lookDirection;
      GameObject jet = Instantiate(_jetPrefab, position + Vector3.up * height, randomizedDirection);
      jet.transform.SetParent(transform);

      jet.GetComponent<ConstantMove>()._speed = _jetSpeed + (Random.Range(-1, 1) * _jetSpeedVariance);

    }
  }
}
