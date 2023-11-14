using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class RifleScript : WeaponScript
{
  [SerializeField] private GameObject _barrelPositionMarker;
  [SerializeField] private LineRenderer _bulletEffect;
  [SerializeField] private float _damage;
  [SerializeField] private float _secBetweenShots;
  [SerializeField] private float _maxAmmo;
  [SerializeField] private float _ammo;
  [SerializeField] private float _maxRaycastDistance = 5000f;
  [SerializeField] private LayerMask _raycastLayerMask;
  
  private float _timeSinceLastShot;
  
  public Vector3 GetCenterScreenWorldPosition() {
    Camera mainCamera = _instancingPlayerCombatCoreScript._mainCamera;
    if (mainCamera != null) {
      // Maybe make a variable for max distance here later?
      Physics.Raycast(mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2)), mainCamera.transform.forward, out RaycastHit hit, _maxRaycastDistance, _raycastLayerMask);
      if (hit.collider != null) {
        return hit.point;
      }
      return mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2)) + mainCamera.transform.forward * _maxRaycastDistance;
    }
    Debug.LogError($"{gameObject.name} RifleScript: Could not find mainCamera!");
    return Vector3.zero;
  }
  
  private void RaycastBullet() {
    _timeSinceLastShot = 0;
    Vector3 hitPosition = GetCenterScreenWorldPosition();
    StartCoroutine(PlayBullet(hitPosition));
    Vector3 barrelPosition = _barrelPositionMarker.transform.position;
    Physics.Linecast(barrelPosition, hitPosition, out RaycastHit hit);
    Debug.DrawLine(barrelPosition, hitPosition, Color.red, 1f);
    if (hit.collider == null) return;
    if (hit.collider.GetComponent<HealthInterface>() != null) {
      hit.collider.GetComponent<HealthInterface>().Damage(_damage, hit.point);
    }
  }

  private IEnumerator PlayBullet(Vector3 hitPosition) {
    _bulletEffect.enabled = true;
    _bulletEffect.SetPosition(0, _barrelPositionMarker.transform.position);
    _bulletEffect.SetPosition(1, hitPosition);
    yield return null;
    _bulletEffect.enabled = false;

  }

  private void OnValidate() {
    _ammo = _maxAmmo;
  }

  // Start is called before the first frame update
  private void Start() {
    _ammo = _maxAmmo;
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