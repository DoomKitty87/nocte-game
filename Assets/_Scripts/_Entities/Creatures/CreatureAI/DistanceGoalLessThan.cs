using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class DistanceGoalLessThan : TreeNode
	{
		private Transform _transform;
		private float _distance;

		public DistanceGoalLessThan(Transform transform, float distance) {
			_transform = transform;
			_distance = distance;
		}

		public override TreeNodeState Evaluate() {
			Vector3 position = _transform.position;
			List<float> goalPosition = (List<float>) GetData("goal");
			Vector2 goalPosVec = new Vector2(goalPosition[0], goalPosition[1]);
			Debug.Log(goalPosVec);
			Vector2 enemyPosVec = new Vector2(_transform.position.x, _transform.position.z);
			float dist = Vector2.Distance(goalPosVec, enemyPosVec);
			_nodeState = dist < _distance ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
			return _nodeState;
		}
	}
}