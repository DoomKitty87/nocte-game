using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class CheckDenExists : TreeNode
  {
    private EnemyController _controller;

    public CheckDenExists(EnemyController controller) {
      _controller = controller;
    }

    public override TreeNodeState Evaluate() {
      _nodeState = _controller.GetDenPos() != Vector3.zero ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
      return _nodeState;
    }
  }
}