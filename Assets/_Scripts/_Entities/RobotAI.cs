using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RobotAI : MonoBehaviour
{

  public Transform _playerTransform;
  
  [SerializeField] private float _speed;
  [SerializeField] private float _visionRange;
  [SerializeField] private float _attackRange;
  [SerializeField] private float _attackCooldown;
  [SerializeField] private float _attackDamage;

  private float _lastAttack;

  private void Update()
  {
    _lastAttack += Time.deltaTime;
    float dist = Vector3.Distance(transform.position, _playerTransform.position);
    if (dist < _visionRange)
    {
      if (dist < _attackRange && _lastAttack > _attackCooldown)
      {
        Attack();
      }
      else
      {
        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _speed * Time.deltaTime);
      }
    }
  }

  private void Attack() {
    _lastAttack = 0;
    Debug.Log("Attacking player");
    _playerTransform.GetComponent<HealthInterface>().Damage(_attackDamage, Vector3.zero);
  }

}
