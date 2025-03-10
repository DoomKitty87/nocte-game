using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Connection : MonoBehaviour
{

  [SerializeField] private Image _connection;
  [SerializeField] private float _maxDistance;
  [SerializeField] private HealthInterface _playerHealth;
  [SerializeField] private float _damageRate;
  [SerializeField] private Transform _playerTransform;

  public static Vector2 _centerPosition;

  private void Update() {
    float distance = Vector2.Distance(new Vector2(_playerTransform.position.x, _playerTransform.position.z), _centerPosition) / _maxDistance;
    _connection.fillAmount = 1 - Mathf.Clamp(distance, 0, 1);
    if (distance > 1) {
      _playerHealth.Damage(_damageRate * Time.deltaTime, Vector3.zero);
    }
  }

}
