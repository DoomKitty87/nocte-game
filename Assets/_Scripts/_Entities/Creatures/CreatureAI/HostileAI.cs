using System;
using System.Collections.Generic;
using System.Numerics;
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
		[SerializeField] private float _distanceRoam;
		[SerializeField] private float _distanceGoal;
		[SerializeField] private float _range;
		[SerializeField] private List<CreatureAttack> _attacks;

		protected override TreeNode SetupTree() {
			_transform = transform;
			_controller = gameObject.GetComponent<EnemyController>();
			_creatureCombat = gameObject.GetComponent<CreatureCombat>();
			return new Selector(new List<TreeNode> {
				new Sequencer(new List<TreeNode> {  // checks if the enemy is close to the player and starts chasing
					new DistanceTransformLessThan(_transform, _playerTransform, _distanceChase),
					new FaceTransform(_transform, _playerTransform, true, false, true),
					new SetMovementToDirection(_transform.forward, _controller)
				}), 
				new Sequencer(new List<TreeNode> { // checks if the enemy is far from the player and starts roaming
					new Invertor(new DistanceTransformLessThan(_transform, _playerTransform, _distanceRoam)),
					new PlaceRandomGoal(_controller, _transform, _range),
					new FaceGoal(_transform, true, false, true),
					new SetMovementToDirection(_transform.forward, _controller)
				}), 
				new Sequencer(new List<TreeNode> { // checks if the enemy is close to the goal and makes a new one
					new DistanceGoalLessThan(_transform, _distanceGoal),
					new PlaceRandomGoal(_controller, _transform, _range),
					new FaceGoal(_transform, true, false, true),
					new SetMovementToDirection(_transform.forward, _controller)
				})
			});
		}
	}
}