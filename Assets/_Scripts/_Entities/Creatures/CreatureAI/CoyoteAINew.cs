using System.Collections.Generic;
using System.Linq.Expressions;
using _Scripts._BehaviorTree;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	// TODO: Make debug Tree node
	// stupid arborescent behavior tree

	// Define leaf nodes here
	[RequireComponent(typeof(Transform))]
	[RequireComponent(typeof(CreatureCombat))]
	public class CoyoteAINew : BehaviorTree
	{
		[Header("References")] private Transform _transform;
		private CreatureCombat _creatureCombat;
		private EnemyController _controller;
		private Transform _playerTransform;
		//private WorldGenerator _worldGenerator;

		[Header("Settings")] 
		[SerializeField] private float _distanceAttack;
		[SerializeField] private float _distanceChase;
		[SerializeField] private float _distanceGoal;
		[SerializeField] private float _distanceHeal;
		[SerializeField] private float _range;
		[SerializeField] private float _staminaLimit;
		[SerializeField] private float _leaveLimit;
		[SerializeField] private float _recencyLimit;
		[SerializeField] private List<CreatureAttack> _attacks;

		protected override TreeNode SetupTree() {
			_transform = transform;
			_controller = gameObject.GetComponent<EnemyController>();
			_creatureCombat = gameObject.GetComponent<CreatureCombat>();
			_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
			//_worldGenerator = WorldGenInfo._worldGenerator;
			return new Noder(new List<TreeNode> {
				// firstly, choose state
				// animal should run when escaping a player (low health, low stamina)
				// animal should run when chasing a player
				// animal should walk when searching for food
				// animal should attack when close to a player
				new Sequencer(new List<TreeNode> {
					new Invertor(new CheckState(_controller, EnemyController.PlayerStates.Air)),
					new Invertor(new StaminaLessThan(_controller, _staminaLimit)),
					new Selector(new List<TreeNode> {
						new Sequencer(new List<TreeNode> { // checks if very close to player and attacks
							new DistanceTransformLessThan(_transform, _playerTransform, _distanceAttack),
							new CheckAttackRecent(_creatureCombat, _recencyLimit),
							new SetState(_controller, EnemyController.PlayerStates.Idle),
							new UseAttack(_creatureCombat, _attacks[0]),
							new ChangeStamina(_controller, -20)
						}),
						new Sequencer(new List<TreeNode> {  // checks if the enemy is close to the player and starts chasing
							new DistanceTransformLessThan(_transform, _playerTransform, _distanceChase),
							new FaceTransform(_transform, _playerTransform, true, false, true, false),
							new SetMovementToDirection(_transform.forward, _controller),
							new Invertor(new CheckAttackRecent(_creatureCombat, _recencyLimit)),
							new SetState(_controller, EnemyController.PlayerStates.Sprinting),
							new ChangeStamina(_controller, -0.5f * Time.deltaTime)
						}),
						new Sequencer(new List<TreeNode> { // checks if the enemy is far from the player and starts roaming
							new SetState(_controller, EnemyController.PlayerStates.Walking),
							new SetMovementToDirection(_transform.forward, _controller),
							// new ChangeStamina(_controller, -0.5f * Time.deltaTime),
							new Invertor(new CheckGoalExists(_controller)),
							new PlaceRandomGoal(_controller, _transform, _range),
							new FaceGoal(_controller, _transform, true, false, true),
						}),
						new Sequencer(new List<TreeNode> {
							// new CheckStuck(_transform),
							// new PlaceRandomGoal(_controller, _transform, _range)
						})
					})
				}),
				new Sequencer(new List<TreeNode> { // checks if the enemy is close to the goal and makes a new one
					new DistanceGoalLessThan(_controller, _transform, _distanceGoal),
					new PlaceRandomGoal(_controller, _transform, _range),
					new FaceGoal(_controller, _transform, true, false, true)
				}),
				new Selector(new List<TreeNode> {
					new Sequencer(new List<TreeNode> { // keeps coyote at den to heal up before leaving
						new StaminaLessThan(_controller, _leaveLimit),
						new DistanceDenLessThan(_controller, _transform, _distanceHeal),
						new FaceDen(_controller, _transform, true, false, true),
						new SetMovementToDirection(_transform.forward, _controller),
						new ChangeStamina(_controller, 5 * Time.deltaTime)
					}),
					new Sequencer(new List<TreeNode> { // goes to den after lack of stamina
						new StaminaLessThan(_controller, _staminaLimit),
						new FaceDen(_controller, _transform, true, false, true),
						new SetMovementToDirection(_transform.forward, _controller),
						new SetState(_controller, EnemyController.PlayerStates.Walking)
					})
				}),
				new Sequencer(new List<TreeNode> { // refills stamina
					new StaminaLessThan(_controller, _staminaLimit),
					new DistanceDenLessThan(_controller, _transform, _distanceHeal),
					new ChangeStamina(_controller, 5 * Time.deltaTime)
				})
			});
		}
	}
}