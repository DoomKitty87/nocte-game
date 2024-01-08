using System.Collections.Generic;
using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	// Define leaf nodes here
	
	[RequireComponent(typeof(Transform))]
	[RequireComponent(typeof(NavMeshAgent))]
	public class PassiveAI : BehaviorTree
	{
		[Header("References")]
		private Transform _transform;
		private NavMeshAgent _navMeshAgent;
		[SerializeField] private Transform _playerTransform;

		[Header("Settings")]
		[SerializeField] private float _distanceEvade;
		[SerializeField] private float _distanceEvadeTo;
		
		protected override TreeNode SetupTree() {
			_transform = gameObject.GetComponent<Transform>();
			_navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			return new Selector(new List<TreeNode> {
				new Sequencer(new List<TreeNode> {
					new DistanceTransformLessThan(transform, _playerTransform, _distanceEvade),
					new SetAgentToEvadeTransform(_navMeshAgent, transform, _playerTransform, _distanceEvadeTo)
				})
			});
		}
	}
}