using System;
using System.Collections.Generic;
using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Entities.Creatures.Passive
{
	// Define leaf nodes here
	public class DistanceToPlayerLessThan : TreeNode
	{
		private Transform _transform;
		private Transform _playerTransform;
		private float _distance;

		public DistanceToPlayerLessThan(Transform playerTransform, float distance) {
			_transform = PassiveAI._transform;
			_playerTransform = playerTransform;
			_distance = distance;
		}

		public override TreeNodeState Evaluate() {
			Vector3 position = _transform.position;
			Vector3 playerPosition = _playerTransform.position;
			if (Vector3.Distance(position, playerPosition) < _distance)
				_nodeState = TreeNodeState.SUCCESS;
			else
				_nodeState = TreeNodeState.FAILED;
			return _nodeState;
		}
	}

	public class EvadeTransform : TreeNode
	{
		private Transform _transform;
		private Transform _transformToEvade;
		private Vector3 _lastEvasionTransfromPosition;
		private NavMeshAgent _navMeshAgent;
		private float _evadedDistance;
		public EvadeTransform(Transform transformToEvade, float evadedDistance) {
			_transform = PassiveAI._transform;
			_transformToEvade = transformToEvade;
			_navMeshAgent = PassiveAI._navMeshAgent;
			_evadedDistance = evadedDistance;
		}

		private Vector3 GetFuturePosition(Vector3 lastPosition, Vector3 currentPosition) {
			return currentPosition + (currentPosition - lastPosition);
		}
		
		// Find closest point on circle with radius evadedDistance around player 
		private Vector3 GetEvasionPosition() {
			Vector3 difference = _transformToEvade.position - _transform.position;
			return difference.normalized * -(_evadedDistance);
		}

		private bool IsNearPosition(Vector3 position, float radius) {
			if (Vector3.Distance(_transform.position, position) < radius) {
				return true;
			}
			return false;
		}
		
		public override TreeNodeState Evaluate() {
			_navMeshAgent.SetDestination(GetEvasionPosition());
			_nodeState = IsNearPosition(_navMeshAgent.destination, 0.05f) ? TreeNodeState.SUCCESS : TreeNodeState.RUNNING;
			return _nodeState;
		}
	}
	
	[RequireComponent(typeof(Transform))]
	[RequireComponent(typeof(NavMeshAgent))]
	public class PassiveAI : BehaviorTree
	{
		// Tree Wide variables
		[Header("References")]
		public static Transform _transform;
		public static NavMeshAgent _navMeshAgent;
		public Transform _playerTransform;

		[Header("Settings")]
		[SerializeField] private float _distanceEvade;
		[SerializeField] private float _distanceEvadeTo;
		
		protected override TreeNode SetupTree() {
			_transform = gameObject.GetComponent<Transform>();
			_navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			return new Sequencer(new List<TreeNode> {
				new DistanceToPlayerLessThan(_playerTransform, _distanceEvade),
				new EvadeTransform(_playerTransform, _distanceEvadeTo)
			});
		}
	}
}