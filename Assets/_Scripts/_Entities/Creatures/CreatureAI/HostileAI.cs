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
	[RequireComponent(typeof(CreatureCombat))]
	public class HostileAI : BehaviorTree
	{
		[Header("References")] private Transform _transform;
		private CreatureCombat _creatureCombat;
		private EnemyController _controller;
		[SerializeField] private Transform _playerTransform;

		[Header("Settings")]
		[SerializeField] private float _distanceChase;
		[SerializeField] private float _distanceAttack;
		[SerializeField] private List<CreatureAttack> _attacks;

		protected override TreeNode SetupTree() {
			_transform = transform;
			_controller = gameObject.GetComponent<EnemyController>();
			_creatureCombat = gameObject.GetComponent<CreatureCombat>();
			return new Selector(new List<TreeNode> {
				new Sequencer(new List<TreeNode> {
					new DistanceToPlayerLessThan(_transform, _playerTransform, _distanceAttack),
					new FaceTransform(_transform, _playerTransform, true, false, true),
					// new Succeder(
					// 	new AttackWithCreatureCombat(_creatureCombat, _attacks[Random.Range(0, 1)])
					// )
				}),
				new Sequencer(new List<TreeNode> {
					new DistanceToPlayerLessThan(transform, _playerTransform, _distanceChase),
					new FaceTransform(_transform, _playerTransform, true, false, true),
					new SetMovementToChaseTransform(_transform, _playerTransform, _controller)
				})
			});
		}
	}
}