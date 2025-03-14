using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class WeaponScript : MonoBehaviour
{
  // Weapon Script should contain the bare minimum needed to work with the PlayerCombatCore, aim in, aim out
  // Bare minimum weapon function
  // Grab InputManager
  // Animate Equip, Unequip, Fire
  // IK for aiming two handed weapons
  
  // Sound handled by TOPLEVEL classes
  
  // Control logic handled by SECONDLEVEL classes
  // Ammo, Fire Rate, Reload Time, Recoil, Damage, Raycast, Bullet Interact
  
  // Inheritance Structure
  //
  //                        WeaponScript
  // TOPLEVEL: MeleeWeapon, RangedWeapon, ThrowableWeapon
  // 
  // SECONDLEVEL: RangedWeapon --> ChargeShot, MagazineWeapon, CustomProjectileWeapon
  
  [Header("Weapon Input & Core | CC means auto created or assigned instance")]
  public PlayerCombatCore _instancingPlayerCombatCoreScript;
  [SerializeField] protected PlayerInput _playerInputCC;
  [Header("Weapon Animation")]
  [SerializeField] protected Animator _playerAnimatorCC;
  [SerializeField] protected AnimatorOverrideController _playerAnimatorOverride;
  [SerializeField] protected AnimationClip _equipAnimation;
  [SerializeField] protected AnimationClip _unequipAnimation;
  [SerializeField] protected UnityEvent OnAttack;
  [Header("Weapon IK")]
  [SerializeField] protected Transform _leftHandPosMarker;
  [SerializeField] protected Transform _leftHandHintMarker;
  public (Transform, Transform) GetLeftHandIKTargets() {
    return (_leftHandPosMarker, _leftHandHintMarker);
  }
  [Header("Weapon Audio")]
  [SerializeField] protected AudioSource _audioSourceCC;
  
  
  protected virtual void Start() {
    InitalizeReferences();
  }

  private void InitalizeReferences() {
    _playerInputCC = InputReader.Instance.PlayerInput;
    _audioSourceCC = _instancingPlayerCombatCoreScript._weaponFXAudioSource;
    _playerAnimatorCC = _instancingPlayerCombatCoreScript._playerAnimator;
  }
  
  // float = time to wait for animations
  public virtual float OnEquip() {
    InitalizeReferences();
    _instancingPlayerCombatCoreScript._useAimedAnimations = true;
    _playerAnimatorCC.runtimeAnimatorController = _playerAnimatorOverride;
    _playerAnimatorCC.SetTrigger("Weapon_Equip");
    return _equipAnimation.length;
  }

  public virtual float OnUnequip() {
    _playerAnimatorCC.SetTrigger("Weapon_Unequip");
    return _unequipAnimation.length;
  }
  
  public abstract (bool, int, int) GetUsesAmmoCurrentAmmoAndMaxAmmo();

  protected virtual void OnDisable() {
    if (_playerInputCC == null) {
      InitalizeReferences();
    }
  }
}