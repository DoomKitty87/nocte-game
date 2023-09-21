using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

[RequireComponent(typeof(HealthInterface))]
public class CombatCore : MonoBehaviour
{
  [SerializeField] private float _raycastLength;
  [SerializeField] private float _damage;
  [SerializeField] private float _range;
  [SerializeField] private float _cooldown;
  private float _secondsSinceLastAttack;
  // Start is called before the first frame update
  private void Start() {
    _secondsSinceLastAttack = 0;
  }

  // Update is called once per frame
  private void Update() {
    if (Input.GetAxisRaw("Fire1") > 0 && _secondsSinceLastAttack > _cooldown) {
      Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _range);
      if (hit.collider != null) {
        hit.collider.GetComponent<HealthInterface>().Damage(_damage);
      }
      _secondsSinceLastAttack = 0;
    }
    _secondsSinceLastAttack += Time.deltaTime;
  }

  private void OnDrawGizmos() {
    Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _range);
    Gizmos.color = Color.magenta;
    Gizmos.DrawLine(transform.position, transform.position + transform.forward * _range);
  }
}