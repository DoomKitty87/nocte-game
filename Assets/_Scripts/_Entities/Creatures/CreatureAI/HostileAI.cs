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
	public class HostileAI : BehaviorTree
	{
		[Header("References")] private Transform _transform;
		private CreatureCombat _creatureCombat;
		private EnemyController _controller;
		[SerializeField] private Transform _playerTransform;

		[Header("Settings")]
		[SerializeField] private float _distanceChase;
		[SerializeField] private float _distanceGoal;
		[SerializeField] private float _range;
		[SerializeField] private List<CreatureAttack> _attacks;

		protected override TreeNode SetupTree() {
			_transform = transform;
			_controller = gameObject.GetComponent<EnemyController>();
			_creatureCombat = gameObject.GetComponent<CreatureCombat>();
			return new Noder(new List<TreeNode> {
				new Sequencer(new List<TreeNode> {  // checks if the enemy is close to the player and starts chasing
					new LogMessage(LogMessage.LogType.Info, "gonna check if los enemies is close to player"),
					new DistanceTransformLessThan(_transform, _playerTransform, _distanceChase),
					new LogMessage(LogMessage.LogType.Info, "wowowowow he was very close by"),
					new FaceTransform(_transform, _playerTransform, true, false, true),
					new SetMovementToDirection(_transform.forward, _controller),
					new SetState(_controller, EnemyController.PlayerStates.Sprinting),
				}), 
				new Sequencer(new List<TreeNode> { // checks if the enemy is far from the player and starts roaming
					new LogMessage(LogMessage.LogType.Info, "gonna check if los enemies is far from player"),
					new Invertor(new DistanceTransformLessThan(_transform, _playerTransform, _distanceChase)),
					new PlaceRandomGoal(_controller, _transform, _range),
					new SetState(_controller, EnemyController.PlayerStates.Walking),
				}), 
				new Sequencer(new List<TreeNode> { // checks if the enemy is close to the goal and makes a new one
					new LogMessage(LogMessage.LogType.Info, "gonna check if los enemies is close to goal and make a new one"),
					new CheckGoalExists(_controller),
					new DistanceGoalLessThan(_controller, _transform, _distanceGoal),
					new PlaceRandomGoal(_controller, _transform, _range)
				}),
				new Sequencer(new List<TreeNode> { // checks if the enemy is far from the goal and makes him move closer
					new LogMessage(LogMessage.LogType.Info, "gonna check if los enemies is far from goal and move closer"),
					new SetState(_controller, EnemyController.PlayerStates.Walking),
					new CheckGoalExists(_controller),
					new FaceGoal(_controller, _transform, true, false, true),
					new SetMovementToDirection(_transform.forward, _controller)
				}),
				new Sequencer(new List<TreeNode> { // checks if stuck and moves goal
					new LogMessage(LogMessage.LogType.Info, "gonna check if los enemies is stuck"),
					new CheckStuck(_transform),
					new PlaceRandomGoal(_controller, _transform, _range)
				})
			});
		}
	}
}