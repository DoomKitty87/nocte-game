using System;
using System.Collections.Generic;
using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	// Define leaf nodes here
	
	[RequireComponent(typeof(Transform))]
	[RequireComponent(typeof(NavMeshAgent))]
	public class HostileAI : BehaviorTree
	{
		// References to gameObject components will be static for simplicity
		// References to other GameObjects will be non-static for flexibility
		[Header("References")]
		public static Transform _transform;
		public static NavMeshAgent _navMeshAgent;
		public Transform _playerTransform;

		[Header("Settings")]
		[SerializeField] private float _distanceChase;
		[SerializeField] private float _distanceAttack;
		[SerializeField]
		
		protected override TreeNode SetupTree() {
			_transform = gameObject.GetComponent<Transform>();
			_navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			return new Sequencer(new List<TreeNode> {
				new DistanceToPlayerLessThan(transform, _playerTransform, _distanceChase)
			});
		}
	}
}