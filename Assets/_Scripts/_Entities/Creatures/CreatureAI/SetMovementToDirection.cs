using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class SetMovementToDirection : TreeNode
	{
		private Vector3 _vector;
		private EnemyController _controller;

		public SetMovementToDirection(Vector3 vector, EnemyController controller) {
			_vector = vector;
			_controller = controller;
		}

		public override TreeNodeState Evaluate() {
			_controller.SetInputVector(_vector);
			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}
	}
}