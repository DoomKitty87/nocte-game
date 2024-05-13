
using System;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("Weapons/Melee Weapon")]
public class MeleeWeapon : WeaponScript
{
	[Header("Melee Base ============================================================================")]
	[SerializeField] private float _damage;
	[SerializeField] private float _attackCooldown;
	[SerializeField] private AnimationClip _attackAnimation;
	private float _attackCooldownTimer;
	
	public override (bool, int, int) GetUsesAmmoCurrentAmmoAndMaxAmmo() {
		return (false, -1, -1);
	}

	private void Attack() {
		if (_attackCooldownTimer > 0) return;	
		_attackCooldownTimer = _attackCooldown;
		_playerAnimatorCC.SetTrigger("Weapon_Fire");
	}
	
	protected override void Start() {
		base.Start();
		_attackCooldownTimer = 0;
	}

	public override float OnEquip() {
		_playerInputCC.Player.Shoot.performed += _ => Attack();
		return base.OnEquip();
	}
	
	private void Update() {
		_attackCooldownTimer -= Time.deltaTime;
	}

	protected override void OnDisable() {
		base.OnDisable();
		_playerInputCC.Player.Shoot.performed -= _ => Attack();
	}
}