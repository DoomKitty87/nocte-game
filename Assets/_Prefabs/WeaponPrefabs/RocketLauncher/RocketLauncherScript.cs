using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncherScript : WeaponScript
{
  [SerializeField] private GameObject _barrelPositionMarker;
  [SerializeField] private LineRenderer _bulletEffect;
  [SerializeField] private float _damage;
  [SerializeField] private float _secBetweenShots;
  [SerializeField] private float _maxAmmo;
  [SerializeField] private float _ammo;
  [SerializeField] private float _maxRaycastDistance = 5000f;
  [SerializeField] private LayerMask _raycastLayerMask;
  [SerializeField] private GameObject _rocketPrefab;
  [SerializeField] private GameObject _endOfBarrelEmpty;
  
  private float _timeSinceLastShot;
  
  public Vector3 GetCenterScreenWorldPosition() {
    Camera mainCamera = _instancingCombatCoreScript._mainCamera;
    if (mainCamera != null) {
      // Maybe make a variable for max distance here later?
      Physics.Raycast(mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2)), mainCamera.transform.forward, out RaycastHit hit, _maxRaycastDistance, _raycastLayerMask);
      if (hit.collider != null) {
        return hit.point;
      }
      return mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2)) + mainCamera.transform.forward * _maxRaycastDistance;
    }
    Debug.LogError($"{gameObject.name} RocketLauncherScript: Could not find mainCamera!");
    return Vector3.zero;
  }
  
  private void FireRocket() {
    _timeSinceLastShot = 0;
    Vector3 hitPosition = GetCenterScreenWorldPosition();
    Vector3 barrelPosition = _barrelPositionMarker.transform.position;
    Instantiate(_rocketPrefab, _endOfBarrelEmpty.transform.position, Quaternion.LookRotation(this.transform.forward, this.transform.up));
  }

  private void OnValidate() {
    _ammo = _maxAmmo;
  }

  private void Start() {
    _ammo = _maxAmmo;
  }

  private void Update() {
    _timeSinceLastShot += Time.deltaTime;
  }

  public override void FireDown() {
    if (!(_ammo > 0 && _timeSinceLastShot > _secBetweenShots)) return;
    FireRocket();
    _ammo--;
  }

  public override void FireUp() {
  }

  public override void FireHold() {
    if (!(_ammo > 0 && _timeSinceLastShot > _secBetweenShots)) return;
    FireRocket();
    _ammo--;
  }

  public override void Fire2Down() {
  }

  public override void Fire2Up() {
  }

  public override void Fire2Hold() {
  }
}
