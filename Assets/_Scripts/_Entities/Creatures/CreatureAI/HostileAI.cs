using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	// TODO: Make debug Tree node
	
	
	// Define leaf nodes here
	[RequireComponent(typeof(Transform))]
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(CreatureCombat))]
	public class HostileAI : BehaviorTree
	{
		[Header("References")]
		private Transform _transform;
		private NavMeshAgent _navMeshAgent;
		private CreatureCombat _creatureCombat;
		[SerializeField] private Transform _playerTransform;

		[Header("Settings")]
		[SerializeField] private float _distanceChase;
		[SerializeField] private float _distanceAttack;
		[SerializeField] private List<CreatureAttack> _attacks;

		protected override TreeNode SetupTree() {
			_transform = gameObject.GetComponent<Transform>();
			_navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			_creatureCombat = gameObject.GetComponent<CreatureCombat>();
			return new Selector(new List<TreeNode> {
				new Sequencer(new List<TreeNode> {
					new DistanceToPlayerLessThan(_transform, _playerTransform, _distanceAttack),
					new FaceTransform(_transform, _playerTransform, true, false, true),
					new AttackWithCreatureCombat(_creatureCombat, _attacks[Random.Range(0, _attacks.Count - 1)])
				}),
				new Sequencer(new List<TreeNode> {
					new DistanceToPlayerLessThan(transform, _playerTransform, _distanceChase),
					new SetAgentToChaseTransform(_playerTransform, _navMeshAgent)
				})
			});
		}
	}
}