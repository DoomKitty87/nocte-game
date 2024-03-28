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
  
  public RaycastHit GetCenterScreenWorldRaycast() {
    Camera mainCamera = _instancingPlayerCombatCoreScript._mainCamera;
    // Maybe make a variable for max distance here later?
    Physics.Raycast(mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2)), mainCamera.transform.forward, out RaycastHit hit, _maxRaycastDistance, _raycastLayerMask);
    return hit;
  }

  public override (float, float) GetAmmo {
    get { return (_ammo, _maxAmmo); }
  }
  
  private void RaycastBullet() {
    _timeSinceLastShot = 0;
    RaycastHit raycastHit = GetCenterScreenWorldRaycast(); 
    // bullet fire sound here
    StartCoroutine(PlayBullet(raycastHit.point)); // bullet effect
    Vector3 barrelPosition = _barrelPositionMarker.transform.position;
    Debug.DrawLine(_instancingPlayerCombatCoreScript._mainCamera.transform.position, raycastHit.point, Color.red, 1f);
    if (raycastHit.collider == null) return;
    if (raycastHit.collider.GetComponent<HealthInterface>() != null) {
      raycastHit.collider.GetComponent<HealthInterface>().Damage(_damage, raycastHit.point);
    }
    if (raycastHit.collider.GetComponent<BulletInteract>() != null) {
      raycastHit.collider.GetComponent<BulletInteract>().Interact(_damage, raycastHit.point);
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