using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class DistanceTransformLessThan : TreeNode
	{
		private Transform _transform;
		private Transform _secondTransform;
		private float _distance;

		public DistanceTransformLessThan(Transform transform, Transform secondTransform, float distance) {
			_transform = transform;
			_secondTransform = secondTransform;
			_distance = distance;
		}

		public override TreeNodeState Evaluate() {
			Vector3 position = _transform.position;
			Vector3 secondPosition = _secondTransform.position;
			_nodeState = Vector3.Distance(position, secondPosition) < _distance ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
			return _nodeState;
		}
	}
}