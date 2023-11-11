using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class DistanceToPlayerLessThan : TreeNode
	{
		private Transform _transform;
		private Transform _playerTransform;
		private float _distance;

		public DistanceToPlayerLessThan(Transform transform, Transform playerTransform, float distance) {
			_transform = transform;
			_playerTransform = playerTransform;
			_distance = distance;
		}

		public override TreeNodeState Evaluate() {
			Vector3 position = _transform.position;
			Vector3 playerPosition = _playerTransform.position;
			_nodeState = Vector3.Distance(position, playerPosition) < _distance ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
			return _nodeState;
		}
	}
}