using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.VFX;


public class BoltgunScript : WeaponScript
{
  [Header("CombatCore Dependencies")]
  [SerializeField] private AudioSource _audioSource;
  [SerializeField] private Animator _playerAnimator;
  [SerializeField] private string _animationLayerName;

  [Header("Prefab Dependencies")]
  [SerializeField] private GameObject _barrelPositionMarker;
  [SerializeField] private AudioClip _fireSound;
  
  [Header("Settings")]
  [SerializeField] private float _damage;
  [SerializeField] private float _maxRaycastDistance = 5000f;
  [SerializeField] private LayerMask _raycastLayerMask;
  [SerializeField] private float _roundsPerMinute;
  [SerializeField] private float _reloadTime;
  [SerializeField] private RecoilProfile _recoilProfile;
  private float RoundsPerMinuteToWaitTime(float roundsPerMinute) {
    return 60f / roundsPerMinute;
  }
  
  [Header("Indicators")]
  [SerializeField] private bool _firing;
  [SerializeField] private bool _ammoLoaded = true;
  [SerializeField] private bool _reloading = false;
  
  public RaycastHit GetCenterScreenWorldRaycast() {
    Camera mainCamera = _instancingPlayerCombatCoreScript._mainCamera;
    Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, _maxRaycastDistance, _raycastLayerMask);
    return hit;
  }

  private void RaycastBullet() {
    RaycastHit raycastHit = GetCenterScreenWorldRaycast();
    // bullet fire sound here
    // StartCoroutine(PlayBoltEffect(raycastHit.point)); // bullet effect
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


  private IEnumerator PlayFireAnimationCoroutine() {
    _firing = true;
    _ammoLoaded = false;
    _playerAnimator.SetTrigger("Weapon_Fire");
    yield return new WaitForSeconds(RoundsPerMinuteToWaitTime(_roundsPerMinute));
    _firing = false;
  }
  private void PlayFireAnimation() {
    if (_firing) return;
    StartCoroutine(PlayFireAnimationCoroutine());
  }

  private IEnumerator PlayReloadAnimationCoroutine() {
    _reloading = true;
    _playerAnimator.SetTrigger("Weapon_Reload");
    yield return new WaitForSeconds(_reloadTime);
    _ammoLoaded = true;
    _instancingPlayerCombatCoreScript.Weapon_RaiseAmmoChangedEvent();
    _reloading = false;
  }
  private void PlayReloadAnimation() {
    if (_reloading) return;
    StartCoroutine(PlayReloadAnimationCoroutine());
  }
  
  // private IEnumerator PlayBoltEffect(Vector3 hitPosition) {
  //   _boltEffect.enabled = true;
  //   _boltEffect.SetPosition(0, _barrelPositionMarker.transform.position);
  //   _boltEffect.SetPosition(1, hitPosition);
  //   yield return null;
  //   _boltEffect.enabled = false;
  // }

  // ========================================================================================================================================
  // Main
  // ========================================================================================================================================
  
  // Start is called before the first frame update
  private void Start() {
    base.Start();
    
    _ammoLoaded = true;
    _reloading = false;
    _firing = false;
    
    _playerInputCC.Player.Shoot.performed += _ => FireDown();
    _playerInputCC.Player.Shoot.canceled += _ => FireUp();
    _playerInputCC.Player.ADS.performed += _ => Fire2Down();
    _playerInputCC.Player.ADS.canceled += _ => Fire2Up();
  }

  private void OnDisable() {
    
  }

  // Update is called once per frame
  private void Update() {
    
  }

  public void FireDown() {
    if (!_ammoLoaded || _reloading) return;
    RaycastBullet();
    _instancingPlayerCombatCoreScript._recoilCameraScript.AddRecoil();
    PlayFireAnimation();
    _instancingPlayerCombatCoreScript.Weapon_RaiseAmmoChangedEvent();
    _audioSource.PlayOneShot(_fireSound, 1f);
    _ammoLoaded = false;
  }

  public void FireUp() {
    
  }
  
  public void Fire2Down() {
   
  }

  public void Fire2Up() {
    
  }
  
  public void ReloadDown() {
    if (_ammoLoaded || _reloading) return;
    PlayReloadAnimation();
  }
  
  public void ReloadUp() {
    
  }
  
  public void ReloadHold() {
    
  }

  public override float OnEquip() {
    base.OnEquip();
    _instancingPlayerCombatCoreScript._recoilCameraScript.SetRecoilProfile(_recoilProfile);
    return _playerAnimator.GetNextAnimatorStateInfo(_playerAnimator.GetLayerIndex(_animationLayerName)).length;
  }
  
  public override float OnUnequip() {
    base.OnUnequip();
    _instancingPlayerCombatCoreScript._recoilCameraScript.SetRecoilProfile(null);
    return _playerAnimator.GetNextAnimatorStateInfo(_playerAnimator.GetLayerIndex(_animationLayerName)).length;
  }

  public override (bool, int, int) GetUsesAmmoCurrentAmmoAndMaxAmmo() {
    return (true, _ammoLoaded ? 1 : 0, 1);
  }
}