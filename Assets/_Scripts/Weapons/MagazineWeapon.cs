
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public abstract class MagazineWeapon : WeaponScript
{
	[Header("Magazine Weapon Base ============================================================================")]
	public FireType _fireType;
	public enum FireType
	{
		SemiAuto,
		Burst,
		FullAuto
	}
	[Header("Recoil")]
	[SerializeField] protected RecoilCamera _recoilCameraCC;
	[SerializeField] protected RecoilProfile _recoilProfile;
	[Header("Magazine")]
	[SerializeField] private int _magazineSize;
	[SerializeField] private int _ammoCount;
	[SerializeField] private AnimationClip _reloadAnimation;
	[SerializeField] private float _reloadTime;

	#region MagazineFunctions

  private float MagazineMultiplier() {
    float magazineMultiplier = 1;
    if (UpgradeInfo._magSize != -1) {
      magazineMultiplier = UpgradeInfo._magSize;
    }
    return magazineMultiplier;
  }

	public override (bool, int, int) GetUsesAmmoCurrentAmmoAndMaxAmmo() {
		return (true, _ammoCount, (int)(_magazineSize * MagazineMultiplier()));
	}
	private float GetReloadTime() {
    float reloadTimeMultiplier = 1;
    if (UpgradeInfo._reloadSpeed != -1) {
      reloadTimeMultiplier =  1f / UpgradeInfo._reloadSpeed;
    }
		if (_reloadAnimation != null) {
			return _reloadAnimation.length * reloadTimeMultiplier;
		}
		return _reloadTime * reloadTimeMultiplier;
	}
	private bool _reloading;
	private IEnumerator ReloadCoroutine() {
		_reloading = true;
		_playerAnimatorCC.SetTrigger("Weapon_Reload");
		yield return new WaitForSeconds(GetReloadTime());
		_ammoCount = (int)(_magazineSize * MagazineMultiplier());
		_instancingPlayerCombatCoreScript.Weapon_RaiseAmmoChangedEvent();
		_instancingPlayerCombatCoreScript._useAimedAnimations = true;
		_reloading = false;
	}
	protected void Reload() {
		if (_reloading || _ammoCount == (int)(_magazineSize * MagazineMultiplier())) return;
		_instancingPlayerCombatCoreScript._useAimedAnimations = false;
		StartCoroutine(ReloadCoroutine());
	}
	
	#endregion

	[Header("Audio")]
	[SerializeField] private AudioClip _shootSound;
	[Header("Firetype Dependent ----------------\n\nSemi-Auto")]
	[SerializeField] private float _semiAutoMinShotCooldown;
	
	[Header("Burst")]
	[SerializeField] private int _burstCount;
	[SerializeField] private float _burstBtwnShotDelay;
	[SerializeField] private float _burstCooldown;
	
	[Header("Full-Auto")]
	[SerializeField] private float _autoPerShotDelay;
	
	// Only needs to handle damaging logic
	protected abstract void Shoot();
	private void ShootWrapper() {
		_recoilCameraCC.AddRecoil(_recoilProfile);
    OnAttack.Invoke();
		_audioSourceCC.PlayOneShot(_shootSound);
		_ammoCount--;
		_instancingPlayerCombatCoreScript.Weapon_RaiseAmmoChangedEvent();
		Shoot();
	}

	
	private bool _firingBurst = false;
	private IEnumerator BurstShootCoroutine() {
		_firingBurst = true;
		for (int i = 0; i < _burstCount; i++) {
			_secSinceLastBurst = 0;
			ShootWrapper();
			yield return new WaitForSeconds(_burstBtwnShotDelay);
		}
		_firingBurst = false;
	}
	
	private bool _lastInputIsAttackPerformed = false;
	private float _secSinceLastShoot = 0;
	private float _secSinceLastBurst = 0;
	private bool _fireAuto = false;
	private void AttackPerformed() {
		if (_ammoCount <= 0) {
			return;
		}
		switch (_fireType) {
			case FireType.SemiAuto:
				if (_lastInputIsAttackPerformed || _secSinceLastShoot < _semiAutoMinShotCooldown) break;
				_playerAnimatorCC.SetTrigger("Weapon_Fire");
				ShootWrapper();
				_secSinceLastShoot = 0;
				break;
			case FireType.Burst:
				if (_lastInputIsAttackPerformed || _secSinceLastBurst < _burstCooldown) break;
				_playerAnimatorCC.SetTrigger("Weapon_Fire");
				if (!_firingBurst) {
					StartCoroutine(BurstShootCoroutine());
				}
				break;
			case FireType.FullAuto:
				_fireAuto = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		if (!_lastInputIsAttackPerformed) {
			_lastInputIsAttackPerformed = true;
		}
	}
	private void AttackCanceled() {
		if (_lastInputIsAttackPerformed) {
			_lastInputIsAttackPerformed = false;
		}
		_fireAuto = false;
	}
	
	protected override void Start() {
		base.Start();
		_recoilCameraCC = _instancingPlayerCombatCoreScript._recoilCameraScript;
	}
	
	public override float OnEquip() {
		_playerInputCC.Player.Shoot.performed += _ => AttackPerformed();
		_playerInputCC.Player.Shoot.canceled += _ => AttackCanceled();
		_playerInputCC.Player.Reload.performed += _ => Reload();
		_instancingPlayerCombatCoreScript.Weapon_RaiseAmmoChangedEvent();
		return base.OnEquip();
	}

	public override float OnUnequip() {
		_playerInputCC.Player.Shoot.performed -= _ => AttackPerformed();
		_playerInputCC.Player.Shoot.canceled -= _ => AttackCanceled();
		_playerInputCC.Player.Reload.performed -= _ => Reload();
		return base.OnUnequip();
	}
	
	protected virtual void Update() {
		_secSinceLastShoot += Time.deltaTime;
		_secSinceLastBurst += Time.deltaTime;
		if (_fireAuto && _secSinceLastShoot >= _autoPerShotDelay && _ammoCount > 0) {
			_playerAnimatorCC.SetTrigger("Weapon_Fire");
			ShootWrapper();
			_secSinceLastShoot = 0;
		}
	}

	protected override void OnDisable() {
		base.OnDisable();
		_playerInputCC.Player.Shoot.performed -= _ => AttackPerformed();
		_playerInputCC.Player.Shoot.canceled -= _ => AttackCanceled();
		_playerInputCC.Player.Reload.performed -= _ => Reload();
	}
}