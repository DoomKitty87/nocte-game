using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class CheckStuck : TreeNode
  {
    private Transform _transform;

    public CheckStuck(Transform transform) {
      _transform = transform;
    }

    public override TreeNodeState Evaluate() {
      Physics.Raycast(_transform.position, _transform.forward, out RaycastHit hit, 10f);
      float angle = Mathf.Atan(hit.distance / 2) * Mathf.Rad2Deg;
      Debug.Log(angle);
      _nodeState = angle > 30 ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
      return _nodeState;
    }
  }
}