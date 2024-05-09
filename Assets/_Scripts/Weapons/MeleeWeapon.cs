
using System;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeapon : WeaponScript
{
	[Header("Melee Base")]
	[SerializeField] private float _damage;
	[SerializeField] private float _attackCooldown;
	[SerializeField] private List<AnimationClip> _attackAnimations;
	private float _attackCooldownTimer;
	
	public override (bool, int, int) GetUsesAmmoCurrentAmmoAndMaxAmmo() {
		return (false, -1, -1);
	}

	private void Attack() {
		if (_attackCooldownTimer > 0) return;	
		_attackCooldownTimer = _attackCooldown;
		_playerAnimatorCC.SetTrigger("Weapon_Fire");
		_playerAnimatorCC.pla
	}
	
	protected override void Start() {
		base.Start();
		_playerInputCC.Player.Shoot.performed += _ => Attack();
		_attackCooldownTimer = 0;
	}

	private void Update() {
		_attackCooldownTimer -= Time.deltaTime;
	}

	private void OnDisable() {
		_playerInputCC.Player.Shoot.performed -= _ => Attack();
	}
}