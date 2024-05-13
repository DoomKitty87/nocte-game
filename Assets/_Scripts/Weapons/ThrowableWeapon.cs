
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("Weapons/Throwable Weapon")]
public class ThrowableWeapon : WeaponScript
{
	[Header("Throwable Base ============================================================================")]
	[SerializeField] private MeshRenderer _holdingRenderer;
	[SerializeField] private GameObject _prefabToThrow;
	private float _throwForce = 1000f;
	[SerializeField] private float _secsBeforeThrow = 0.05f;
	[SerializeField] private float _throwCooldown;
	[SerializeField] private AnimationClip _throwAnimation;
	private float _throwCooldownTimer = 0;

	[Header("Throwable Ammo")]
	[SerializeField] private int _throwsLeft = 3;
	
	public override (bool, int, int) GetUsesAmmoCurrentAmmoAndMaxAmmo() {
		return (true, _throwsLeft, -1);
	}

	private void ThrowPrefab(GameObject prefab, Vector3 position, Vector3 direction, float force) {
		Instantiate(prefab, position, Quaternion.LookRotation(direction)).GetComponent<Rigidbody>().AddForce(direction * force);
	}
	
	private bool _attacking = false;
	private IEnumerator AttackCoroutine() {
		_attacking = true;
		_playerAnimatorCC.SetTrigger("Weapon_Fire");
		yield return new WaitForSeconds(_secsBeforeThrow);
		ThrowPrefab(_prefabToThrow, _holdingRenderer.transform.position, _instancingPlayerCombatCoreScript._mainCamera.transform.forward, _throwForce);
		_holdingRenderer.enabled = false;
		yield return new WaitForSeconds(_throwAnimation.length - _secsBeforeThrow);
		if (_throwsLeft <= 0) {
			_instancingPlayerCombatCoreScript.UnequipCurrentWeapon();
		}
		_holdingRenderer.enabled = true;
		_playerAnimatorCC.SetTrigger("Weapon_Equip");
		yield return new WaitForSeconds(_equipAnimation.length);
		_attacking = false;
	}
	private void Attack() {
		if (_throwCooldownTimer > 0 || _throwsLeft <= 0) return;	
		_throwCooldownTimer = _throwCooldown;
		_throwsLeft--;
		if (!_attacking) StartCoroutine(AttackCoroutine());
	}

	public override float OnEquip() {
		float baseOutput = base.OnEquip();
		_playerInputCC.Player.Shoot.performed += _ => Attack();
		_holdingRenderer.enabled = true;
		_throwCooldownTimer = _throwCooldown;
		return baseOutput;
	}
	
	private void Update() {
		_throwCooldownTimer -= Time.deltaTime;
	}

	protected override void OnDisable() {
		base.OnDisable();
		_playerInputCC.Player.Shoot.performed -= _ => Attack();
	}
}