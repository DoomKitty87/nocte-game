using System;
using UnityEngine.Events;
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
    [SerializeField] private UnityEvent OnSuccessDamage;
  
		public float _timeOfLastAttack;
		
		public bool Attack(CreatureAttack attack) {
			Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, attack._attackDistance, _layerMask);
			if (hit.collider != null) {
				GameObject hitObject = hit.collider.gameObject;
				_timeOfLastAttack = Time.time;
				if (hitObject.TryGetComponent<HealthInterface>(out HealthInterface healthInterface)) {
					healthInterface.Damage(attack._attackDamage, hit.point);
          OnSuccessDamage?.Invoke();
					return true;
				}
			}
			return false;
		}
	}
}