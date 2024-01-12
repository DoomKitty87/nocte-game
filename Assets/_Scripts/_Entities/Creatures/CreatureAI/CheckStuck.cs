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
      Physics.Raycast(_transform.position, _transform.forward, out RaycastHit hit);
      float angle = Mathf.Atan(hit.distance / 2);
      _nodeState = angle > 20 ? TreeNodeState.FAILED : TreeNodeState.SUCCESS;
      return _nodeState;
    }
  }
}