using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class FaceTransform : TreeNode
	{
		private Transform _transform;
		private Transform _transformToFace;
		private bool _lockX, _lockY, _lockZ, _away;

		public FaceTransform(Transform transform, Transform transformToFace, bool lockX, bool lockY, bool lockZ, bool away) {
			_transform = transform;
			_transformToFace = transformToFace;
			_lockX = lockX;
			_lockY = lockY;
			_lockZ = lockZ;
			_away = away;
		}

		public override TreeNodeState Evaluate() {
			Quaternion originalRot = _transform.rotation;
			Vector3 originalRotEuler = originalRot.eulerAngles;
			Vector3 directionToTarget = _transformToFace.position - _transform.position;
			Quaternion targetRot = Quaternion.LookRotation(directionToTarget, Vector3.up);
			Vector3 targetRotEuler = targetRot.eulerAngles;

			targetRotEuler = new Vector3(
				_lockX ? originalRotEuler.x : targetRotEuler.x,
				_lockY ? originalRotEuler.y : targetRotEuler.y,
				_lockZ ? originalRotEuler.z : targetRotEuler.z

			);

			_transform.rotation = _away ? Quaternion.Inverse(Quaternion.Euler(targetRotEuler)) : Quaternion.Euler(targetRotEuler);

			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}
	}
}