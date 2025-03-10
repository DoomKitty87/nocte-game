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
	public class CoyoteAI : BehaviorTree
	{
		[Header("References")] private Transform transform;
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
			_controller = gameObject.GetComponent<EnemyController>();
			_creatureCombat = gameObject.GetComponent<CreatureCombat>();
			_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
			//_worldGenerator = WorldGenInfo._worldGenerator;
			return new Noder(new List<TreeNode> {
				new Sequencer(new List<TreeNode> {
					new Invertor(new CheckState(_controller, EnemyController.PlayerStates.Air)),
					new Invertor(new StaminaLessThan(_controller, _staminaLimit)),
					new Sequencer(new List<TreeNode> { // enemy just attacked? cannot attack again
						new CheckAttackRecent(_creatureCombat, _recencyLimit),
						new SetState(_controller, EnemyController.PlayerStates.Walking)
					}),
					new Selector(new List<TreeNode> {
						new Sequencer(new List<TreeNode> { // checks if very close to player and attacks
							new DistanceTransformLessThan(transform, _playerTransform, _distanceAttack),
							new CheckAttackRecent(_creatureCombat, _recencyLimit),
							new SetState(_controller, EnemyController.PlayerStates.Idle),
							new UseAttack(_creatureCombat, _attacks[0]),
							new ChangeStamina(_controller, -20),
							new SetState(_controller, EnemyController.PlayerStates.Idle),
						}),
						new Sequencer(new List<TreeNode> {  // checks if the enemy is close to the player and starts chasing
							new DistanceTransformLessThan(transform, _playerTransform, _distanceChase),
							new FaceTransform(transform, _playerTransform, true, false, true, false),
							new SetMovementToDirection(transform.forward, _controller),
							new Invertor(new CheckAttackRecent(_creatureCombat, _recencyLimit)),
							new SetState(_controller, EnemyController.PlayerStates.Sprinting),
							new ChangeStamina(_controller, -0.5f * Time.deltaTime),
						}),
						new Sequencer(new List<TreeNode> { // checks if the enemy is far from the player and starts roaming
							// new Invertor(new DistanceTransformLessThan(transform, _playerTransform, _distanceChase)),
							// new Invertor(new CheckGoalExists(_controller)),
							// new PlaceRandomGoal(_controller, transform, _range),
							new SetState(_controller, EnemyController.PlayerStates.Walking),
							new CheckGoalExists(_controller),
							new FaceGoal(_controller, transform, true, false, true),
							new SetMovementToDirection(transform.forward, _controller),
						}),
						new Sequencer(new List<TreeNode> {
							new CheckStuck(transform),
							//new PlaceRandomGoal(_controller, transform, _range, _worldGenerator)
						})
					})
				}),
				new Sequencer(new List<TreeNode> { // checks if the enemy is close to the goal and makes a new one
					new DistanceGoalLessThan(_controller, transform, _distanceGoal),
					//new PlaceRandomGoal(_controller, transform, _range, _worldGenerator)
				}),
				new Selector(new List<TreeNode> {
					new Sequencer(new List<TreeNode> { // keeps coyote at den to heal up before leaving
						new StaminaLessThan(_controller, _leaveLimit),
						new DistanceDenLessThan(_controller, transform, _distanceHeal),
						new FaceDen(_controller, transform, true, false, true),
						new SetMovementToDirection(transform.forward, _controller),
						new ChangeStamina(_controller, 5 * Time.deltaTime)
					}),
					new Sequencer(new List<TreeNode> { // goes to den after lack of stamina
						new StaminaLessThan(_controller, _staminaLimit),
						new FaceDen(_controller, transform, true, false, true),
						new SetMovementToDirection(transform.forward, _controller),
						new SetState(_controller, EnemyController.PlayerStates.Walking)
					})
				}),
				new Sequencer(new List<TreeNode> { // refills stamina
					new StaminaLessThan(_controller, _staminaLimit),
					new DistanceDenLessThan(_controller, transform, _distanceHeal),
					new ChangeStamina(_controller, 5 * Time.deltaTime)
				})
			});
		}
	}
}