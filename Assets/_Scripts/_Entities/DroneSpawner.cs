using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DroneSpawner : MonoBehaviour
{

    [SerializeField] private GameObject _dronePrefab;
    [SerializeField] private int _droneCountMin;
    [SerializeField] private int _droneCountMax;
    [SerializeField] private float _spawnDelayMean;
    [SerializeField] private float _spawnDelayRange;
    [SerializeField] private float _spawnDistance;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _dronePackSpread;

    [SerializeField] private float _timer = 30f;
    
    private void Start() {
        
    }

    private void Update() {
        _timer -= Time.deltaTime;

        if (_timer <= 0) {
            SpawnPack();
            _timer = _spawnDelayMean + (Random.value - 0.5f) * _spawnDelayRange;
        }
    }

    private void SpawnPack() {
        int spawnCount = Mathf.RoundToInt(Random.value * (_droneCountMax - _droneCountMin) + _droneCountMin);

        float rads = Random.value * Mathf.PI * 2;
        Vector2 centerPosition = new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * _spawnDistance +
                                 new Vector2(_playerTransform.position.x, _playerTransform.position.z);

        for (int i = 0; i < spawnCount; i++) {
            rads = Random.value * Mathf.PI * 2;
            Vector2 dronePosition = centerPosition + new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * _dronePackSpread * Random.value;
            Vector3 placePosition = new Vector3(dronePosition.x, 25, dronePosition.y);
            Instantiate(_dronePrefab, placePosition, Quaternion.identity, transform);
        }
    }
    
}
