using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class SetAgentToChaseTransform : TreeNode
	{
		private Transform _transformToChase;
		private float _graceRadius;
		private NavMeshAgent _navMeshAgent;

		public SetAgentToChaseTransform(Transform transformToChase, NavMeshAgent navMeshAgent, float graceRadius = 0f) {
			_transformToChase = transformToChase;
			_navMeshAgent = navMeshAgent;
			_graceRadius = graceRadius;
		}

		public override TreeNodeState Evaluate() {
			_navMeshAgent.SetDestination(_transformToChase.position);
			_navMeshAgent.stoppingDistance = _graceRadius;
			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}
	}
}