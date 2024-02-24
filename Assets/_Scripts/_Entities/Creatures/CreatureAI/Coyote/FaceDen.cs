using System.Collections.Generic;
using System.Linq;
using _Scripts._BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class FaceDen : TreeNode
  {
    private EnemyController _controller;
    private Transform _transform;
    private bool _lockX, _lockY, _lockZ;

    public FaceDen(EnemyController controller, Transform transform, bool lockX = false, bool lockY = false, bool lockZ = false) {
      _controller = controller;
      _transform = transform;
      _lockX = lockX;
      _lockY = lockY;
      _lockZ = lockZ;
    }

    public override TreeNodeState Evaluate() {
      Quaternion originalRot = _transform.rotation;
      Vector3 originalRotEuler = originalRot.eulerAngles;
      Vector3 denPosition = _controller.GetDenPos();
      Vector3 directionToTarget = new Vector3(denPosition.x, denPosition.y, denPosition.z)  - _transform.position;
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