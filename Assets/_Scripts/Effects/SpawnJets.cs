using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnJets : MonoBehaviour
{

  [SerializeField] private GameObject _jetPrefab;
  [SerializeField] private float _spawnTimer;
  [SerializeField][Range(0, 1)] private float _spawnChance;
  [SerializeField] private float _jetHeight;

  private float _timer;

  void Update() {
    _timer += Time.deltaTime;
    if (_timer >= _spawnTimer) {
      _timer = 0;
      if (Random.value <= _spawnChance) {
        float theta = Random.Range(0, Mathf.PI * 2);
        Vector3 position = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * 5000;
        GameObject jet = Instantiate(_jetPrefab, position + Vector3.up * _jetHeight, Quaternion.LookRotation(-position, Vector3.up));
        jet.transform.SetParent(transform);
      }
    }
  }
}
