using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class DistanceGoalLessThan : TreeNode
	{
		private EnemyController _controller;
		private Transform _transform;
		private float _distance;

		public DistanceGoalLessThan(EnemyController controller, Transform transform, float distance) {
			_controller = controller;
			_transform = transform;
			_distance = distance;
		}

		public override TreeNodeState Evaluate() {
			Vector2 goalPosition = _controller.GetGoalPos();
			Vector2 enemyPosVec = new Vector2(_transform.position.x, _transform.position.z);
			float dist = Vector2.Distance(goalPosition, enemyPosVec);
			_nodeState = dist < _distance ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
			return _nodeState;
		}
	}
}