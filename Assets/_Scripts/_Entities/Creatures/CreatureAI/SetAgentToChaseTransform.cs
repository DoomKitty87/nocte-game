using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class SetAgentToChaseTransform : TreeNode
	{
		private Transform _transformToChase;
		private NavMeshAgent _navMeshAgent;

		public SetAgentToChaseTransform(Transform transformToChase, NavMeshAgent navMeshAgent) {
			_transformToChase = transformToChase;
			_navMeshAgent = navMeshAgent;
		}

		public override TreeNodeState Evaluate() {
			_navMeshAgent.SetDestination(_transformToChase.position);
			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}
	}
}