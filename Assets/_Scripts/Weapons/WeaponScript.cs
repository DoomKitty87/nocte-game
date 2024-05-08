using UnityEngine;
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
  
  [Header("Weapon Input & Core | CC means autoassigned")]
  public PlayerCombatCore _instancingPlayerCombatCoreScript;
  public PlayerInput _playerInput;
  [Header("Weapon Animation")]
  [SerializeField] protected Animator _playerAnimatorCC;
  [SerializeField] protected AnimationClip _equipAnimation;
  [SerializeField] protected AnimationClip _unequipAnimation;
  [Header("Weapon IK")]
  [SerializeField] protected Transform _leftHandPosMarker;
  [SerializeField] protected Transform _leftHandHintMarker;
  public (Transform, Transform) GetLeftHandIKTargets() {
    return (_leftHandPosMarker, _leftHandHintMarker);
  }
  [Header("Weapon Aiming")]
  [SerializeField] protected float _aimSpeed = 6f;
  
  // float = time to wait for animations
  public virtual float OnEquip() {
    _playerAnimatorCC = _instancingPlayerCombatCoreScript._playerAnimator;
    _playerAnimatorCC.SetTrigger("Weapon_Equip");
    return _equipAnimation.length;
  }

  public virtual float OnUnequip() {
    _playerAnimatorCC.SetTrigger("Weapon_Unequip");
    return _unequipAnimation.length;
  }
  
  public abstract (bool, int, int) GetUsesAmmoCurrentAmmoAndMaxAmmo();
}