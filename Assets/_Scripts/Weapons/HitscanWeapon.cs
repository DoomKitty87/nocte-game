
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("Weapons/Hitscan Weapon")]
public class HitscanWeapon : MagazineWeapon
{
	[Header("Hitscan Base ============================================================================")]
	[SerializeField] private Camera _playerCameraCC;
	[SerializeField] private float _damage;
	[SerializeField] private float _maxDistance;
	[SerializeField] private LayerMask _layerMask;

	protected override void Start() {
		base.Start();
		_playerCameraCC = _instancingPlayerCombatCoreScript._mainCamera;
	}

	protected override void Shoot() {
		bool hitSomething = Physics.Raycast(_playerCameraCC.transform.position, _playerCameraCC.transform.forward, out RaycastHit hit, _maxDistance, _layerMask);
		if (hitSomething) {
      float damageMultiplier = 1;
      if (!UpgradeInfo.GetUpgrade("Damage").isLocked) {
        damageMultiplier = UpgradeInfo.GetUpgrade("Damage").value;
      }

      float critChance = 0;
      if (!UpgradeInfo.GetUpgrade("Crit Chance").isLocked) {
        critChance = UpgradeInfo.GetUpgrade("Crit Chance").value;
      }
      if (UnityEngine.Random.value < critChance) {
        damageMultiplier *= 2;
      }
			if (hit.collider.GetComponent<HealthInterface>() != null) {
				hit.collider.GetComponent<HealthInterface>().Damage(_damage * damageMultiplier, hit.point);
			}
			if (hit.collider.GetComponent<BulletInteract>() != null) {
				hit.collider.GetComponent<BulletInteract>().Interact(_damage * damageMultiplier, hit.point);
			}
		}
	}
	
	protected override void Update() {
		base.Update();
	}
}