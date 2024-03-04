using System;
using UnityEngine;

namespace _Scripts._Entities.Creatures
{
	[Serializable]
	public class CreatureAttack
	{
		public float _attackDistance;
		public float _attackDamage;
		public float _attackRepeatSeconds;
	}

	public class CreatureCombat : MonoBehaviour
	{
		[SerializeField] private LayerMask _layerMask;
		public float _timeOfLastAttack;
		
		public bool Attack(CreatureAttack attack) {
			Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, attack._attackDistance, _layerMask);
			if (hit.collider != null) {
				GameObject hitObject = hit.collider.gameObject;
				Debug.Log("attacked");
				if (hitObject.TryGetComponent<HealthInterface>(out HealthInterface healthInterface)) {
					healthInterface.Damage(attack._attackDamage, hit.point);
					_timeOfLastAttack = Time.time;
					return true;
				}
			}
			return false;
		}
	}
}