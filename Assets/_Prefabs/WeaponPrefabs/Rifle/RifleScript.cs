using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.Serialization;

public class RifleScript : WeaponScript
{
  [SerializeField] private GameObject _barrelPositionMarker;
  [SerializeField] private float _damage;
  [SerializeField] private float _secBetweenShots;
  [SerializeField] private float _maxAmmo;
  [SerializeField] private float _ammo;

  private Vector3 _barrelPosition;
  private float _timeSinceLastShot;
  
  private void RaycastBullet() {
    _timeSinceLastShot = 0;
    Physics.Linecast(_barrelPosition, _instancingCombatCoreScript.GetCenterScreenWorldPosition(), out RaycastHit hit);
    Debug.DrawLine(_barrelPosition, _instancingCombatCoreScript.GetCenterScreenWorldPosition(), Color.red, Time.deltaTime);
    if (hit.collider == null) return;
    if (hit.collider.GetComponent<HealthInterface>() != null) {
      hit.collider.GetComponent<HealthInterface>().Damage(_damage);
    }
  }

  // Start is called before the first frame update
  private void Start() {
    _ammo = _maxAmmo;
    _barrelPosition = _barrelPositionMarker.transform.position;
  }

  // Update is called once per frame
  private void Update() {
    _timeSinceLastShot += Time.deltaTime;
  }

  public override void FireDown() {
    if (!(_ammo > 0 && _timeSinceLastShot > _secBetweenShots)) return;
    RaycastBullet();
    _ammo--;
  }

  public override void FireUp() {
  }

  public override void FireHold() {
    if (!(_ammo > 0 && _timeSinceLastShot > _secBetweenShots)) return;
    RaycastBullet();
    _ammo--;
  }

  public override void Fire2Down() {
  }

  public override void Fire2Up() {
  }

  public override void Fire2Hold() {
  }
}