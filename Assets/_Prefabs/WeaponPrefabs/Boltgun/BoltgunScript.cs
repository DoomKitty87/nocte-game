using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;


public class BoltgunScript : WeaponScript
{
  [Header("CombatCore Dependencies")]
  [SerializeField] private CinemachineThirdPersonAim _cinemachineThirdPersonAim;
  [SerializeField] private CinemachineThirdPersonFollow _cinemachineThirdPersonFollow;
  [SerializeField] private Animator _playerAnimator;

  [Header("Prefab Dependencies")]
  [SerializeField] private string _animationLayerName;
  [SerializeField] private AnimationClip _equipAnimation;
  [SerializeField] private AnimationClip _unequipAnimation;
  [SerializeField] private GameObject _barrelPositionMarker;
  [SerializeField] private LineRenderer _boltEffect;
  [Header("Settings")]
  [SerializeField] private float _damage;
  [SerializeField] private float _reloadTime;
  [SerializeField] private float _maxRaycastDistance = 5000f;
  [SerializeField] private LayerMask _raycastLayerMask;
  [Header("Indicators")]
  [SerializeField] private bool _ammoLoaded;
  [SerializeField] private bool _reloading;
  
  public RaycastHit GetCenterScreenWorldRaycast() {
    Camera mainCamera = _instancingPlayerCombatCoreScript._mainCamera;
    Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, _maxRaycastDistance, _raycastLayerMask);
    return hit;
  }

  public override (float, float) GetAmmo {
    get { return (_ammoLoaded ? 1 : 0, 1); }
  }
  
  private void RaycastBullet() {
    RaycastHit raycastHit = GetCenterScreenWorldRaycast(); 
    // bullet fire sound here
    StartCoroutine(PlayBoltEffect(raycastHit.point)); // bullet effect
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

  private void SoloLayerWithName(string layerName) {
    int animLayerIndex = _playerAnimator.GetLayerIndex(layerName);
    int animLayerCount = _playerAnimator.layerCount;
    // skips the base movement layer
    for (int i = 1; i < animLayerCount; i++) {
      if (i == animLayerIndex) {
        _playerAnimator.SetLayerWeight(i, 1);
      } else {
        _playerAnimator.SetLayerWeight(i, 0);
      }
    }
  }
  
  private void PlayFireAnimation() {
    SoloLayerWithName(_animationLayerName);
    _playerAnimator.SetTrigger("Fire");
  }
  
  private void PlayReloadAnimation() {
    SoloLayerWithName(_animationLayerName);
    _playerAnimator.SetTrigger("Reload");
  }
  
  private bool IsAnimatingReloading() {
    return _playerAnimator.GetCurrentAnimatorStateInfo(_playerAnimator.GetLayerIndex(_animationLayerName)).IsName("Reload");
  }
  
  private IEnumerator PlayBoltEffect(Vector3 hitPosition) {
    _boltEffect.enabled = true;
    _boltEffect.SetPosition(0, _barrelPositionMarker.transform.position);
    _boltEffect.SetPosition(1, hitPosition);
    yield return null;
    _boltEffect.enabled = false;

  }

  // Start is called before the first frame update
  private void Start() {
    _ammoLoaded = true;
    _reloading = false;
  }

  // Update is called once per frame
  private void Update() {
    if (IsAnimatingReloading()) {
      _reloading = true;
    }
  }

  public override void FireDown() {
    if (!_ammoLoaded || _reloading) return;
    RaycastBullet();
    PlayFireAnimation();
    _ammoLoaded = false;
  }

  public override void FireUp() {
    
  }

  public override void FireHold() {
    
  }

  public override void Fire2Down() {
    
  }

  public override void Fire2Up() {
    
  }
  
  public override void Fire2Hold() {
    
  }
  
  public override void ReloadDown() {
    if (_ammoLoaded) return;
    PlayReloadAnimation();
    _reloading = true;
  }
  
  public override void ReloadUp() {
    
  }
  
  public override void ReloadHold() {
    
  }

  public override float OnEquip() {
    SoloLayerWithName(_animationLayerName);
    _playerAnimator.SetTrigger("Equip");
    return _equipAnimation.length;
  }
  
  public override float OnUnequip() {
    SoloLayerWithName(_animationLayerName);
    _playerAnimator.SetTrigger("Unequip");
    return _unequipAnimation.length;
  }


  
  
}