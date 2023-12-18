using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class SetMovementToChaseTransform : TreeNode
	{
		private Transform _currentTransform;
		private Transform _transformToChase;
		private EnemyController _controller;

		public SetMovementToChaseTransform(Transform currentTransform, Transform transformToChase, EnemyController controller) {
			_currentTransform = currentTransform;
			_transformToChase = transformToChase;
			_controller = controller;
		}

		public override TreeNodeState Evaluate() {
			_controller.SetInputVector(Vector3.forward);
			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}
	}
}