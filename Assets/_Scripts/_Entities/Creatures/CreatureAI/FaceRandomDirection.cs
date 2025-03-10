using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class FaceRandomDirection : TreeNode
  {
    private Transform _transform;
    private bool _lockX, _lockY, _lockZ;

    public FaceRandomDirection(Transform transform, bool lockX = false, bool lockY = false, bool lockZ = false) {
      _transform = transform;
      _lockX = lockX;
      _lockY = lockY;
      _lockZ = lockZ;
    }

    public override TreeNodeState Evaluate() {
      Quaternion originalRot = _transform.rotation;
      Vector3 originalRotEuler = originalRot.eulerAngles;
      Vector3 directionToTarget = new Vector3(Random.value, Random.value, Random.value);
      Quaternion targetRot = Quaternion.LookRotation(directionToTarget, Vector3.up);
      Vector3 targetRotEuler = targetRot.eulerAngles;

      targetRotEuler = new Vector3(
        _lockX ? originalRotEuler.x : targetRotEuler.x,
        _lockY ? originalRotEuler.y : targetRotEuler.y,
        _lockZ ? originalRotEuler.z : targetRotEuler.z

      );

      _transform.rotation = Quaternion.Euler(targetRotEuler);

      _nodeState = TreeNodeState.SUCCESS;
      return _nodeState;
    }
  }
}