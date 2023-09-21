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
  [SerializeField] private float _knockbackAmount;
  [SerializeField] private float _cooldown;
  [SerializeField] private float _secondsSinceLastAttack;

  private void Attack() {
    Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _range);
    if (hit.collider == null) return;
    if (hit.collider.GetComponent<HealthInterface>() == null) return;
    hit.collider.GetComponent<HealthInterface>().Damage(_damage);
    if (hit.collider.GetComponent<Rigidbody>() == null) return;
    hit.collider.GetComponent<Rigidbody>().AddExplosionForce(_knockbackAmount, hit.point, 1, 2);
  }
  
  // Start is called before the first frame update
  private void Start() {
    _secondsSinceLastAttack = 0;
  }

  // Update is called once per frame
  private void Update() {
    if (Input.GetAxisRaw("Fire1") > 0 && _secondsSinceLastAttack > _cooldown) {
      Attack();
      _secondsSinceLastAttack = 0;
    }
    _secondsSinceLastAttack += Time.deltaTime;
  }

  private void OnDrawGizmos() {
    if (Input.GetAxisRaw("Fire1") == 0) return; 
    Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _range);
    Gizmos.color = Color.magenta;
    Gizmos.DrawLine(transform.position, transform.position + transform.forward * _range);
  }
}