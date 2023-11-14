using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class SetAgentToEvadeTransform : TreeNode
	{
		private Transform _transform;
		private Transform _transformToEvade;
		private Vector3 _lastEvasionTransfromPosition = Vector3.negativeInfinity;
		private NavMeshAgent _navMeshAgent;
		private float _evadedDistance;

		public SetAgentToEvadeTransform(NavMeshAgent navMeshAgent, Transform transform, Transform transformToEvade, float evadedDistance) {
			_transform = transform;
			_transformToEvade = transformToEvade;
			_navMeshAgent = navMeshAgent;
			_evadedDistance = evadedDistance;
		}

		private Vector3 GetFuturePosition(Vector3 lastPosition, Vector3 currentPosition) {
			return currentPosition + (currentPosition - lastPosition);
		}

		// Find closest point on circle with radius evadedDistance around player 
		// TODO: Weird behavior in some situations, remember to fix later.
		private Vector3 GetEvasionPosition(Vector3 positionToEvade) {
			Vector3 difference = positionToEvade - _transform.position;
			return difference.normalized * -_evadedDistance;
		}

		private bool IsNearPosition(Vector3 position, float radius) {
			if (Vector3.Distance(_transform.position, position) < radius) return true;
			return false;
		}

		public override TreeNodeState Evaluate() {
			Vector3 evasionPosition;
			// Comparing to Vector3.negativeInfinity doesn't work for some reason,
			// but testing if the x component is not finite does??
			if (!float.IsFinite(_lastEvasionTransfromPosition.x)) {
				_lastEvasionTransfromPosition = _transformToEvade.position;
				evasionPosition = GetEvasionPosition(_transformToEvade.position);
			}
			else {
				evasionPosition = GetEvasionPosition(GetFuturePosition(_lastEvasionTransfromPosition, _transformToEvade.position));
			}
			_navMeshAgent.SetDestination(evasionPosition);
			_lastEvasionTransfromPosition = _transformToEvade.position;
			if (IsNearPosition(_navMeshAgent.destination, 0.05f)) {
				_nodeState = TreeNodeState.SUCCESS;
				_lastEvasionTransfromPosition = Vector3.negativeInfinity;
			}
			else {
				_nodeState = TreeNodeState.RUNNING;
			}
			return _nodeState;
		}
	}
}