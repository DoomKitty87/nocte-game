using System.Collections.Generic;
using _Scripts._BehaviorTree;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	// TODO: Make debug Tree node
	
	
	// Define leaf nodes here
	[RequireComponent(typeof(Transform))]
	[RequireComponent(typeof(CreatureCombat))]
	public class SquirrelAI : BehaviorTree
	{
		[Header("References")] 
		private Transform _transform;
		private CreatureCombat _creatureCombat;
		private EnemyController _controller;
		[SerializeField] private Transform _playerTransform;

		[Header("Settings")] 
		[SerializeField] private float _distanceEvade;
		[SerializeField] private float _distanceGoal;
		[SerializeField] private float _range;
		[SerializeField] private List<CreatureAttack> _attacks;

		protected override TreeNode SetupTree() {
			_transform = transform;
			_controller = gameObject.GetComponent<EnemyController>();
			_creatureCombat = gameObject.GetComponent<CreatureCombat>();
			return new Noder(new List<TreeNode> {
				new Selector(new List<TreeNode> {
					new Sequencer(new List<TreeNode> {  // checks if the enemy is close to the player and starts chasing
						new DistanceTransformLessThan(_transform, _playerTransform, _distanceEvade),
						new FaceTransform(_transform, _playerTransform, true, false, true, true),
						new SetMovementToDirection(_transform.forward, _controller),
						new SetState(_controller, EnemyController.PlayerStates.Sprinting),
					}), 
					new Sequencer(new List<TreeNode> { // checks if the enemy is far from the player and starts roaming
						new Invertor(new DistanceTransformLessThan(_transform, _playerTransform, _distanceEvade)),
						new Invertor(new CheckGoalExists(_controller)),
						new PlaceRandomGoal(_controller, _transform, _range),
						new SetState(_controller, EnemyController.PlayerStates.Walking),
					})
				}),
				new Sequencer(new List<TreeNode> { // checks if the enemy is close to the goal and makes a new one
					new CheckGoalExists(_controller),
					new DistanceGoalLessThan(_controller, _transform, _distanceGoal),
					new PlaceRandomGoal(_controller, _transform, _range)
				}),
				new Sequencer(new List<TreeNode> { // checks if the enemy is far from the goal and makes him move closer
					new Invertor(new DistanceTransformLessThan(_transform, _playerTransform, _distanceEvade)),
					new SetState(_controller, EnemyController.PlayerStates.Walking),
					new CheckGoalExists(_controller),
					new FaceGoal(_controller, _transform, true, false, true),
					new SetMovementToDirection(_transform.forward, _controller),
					new CheckStuck(_transform),
					new PlaceRandomGoal(_controller, _transform, _range)
				})
			});
		}
	}
}