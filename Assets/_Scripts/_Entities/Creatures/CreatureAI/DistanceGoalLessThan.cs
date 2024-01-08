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
			Debug.Log(goalPosition);
			float xDiff = position.x - goalPosition[0];
			float zDiff = position.z - goalPosition[1];
			float dist = Mathf.Sqrt((xDiff * xDiff) + (zDiff * zDiff));
			_nodeState = dist < _distance ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
			return _nodeState;
		}
	}
}