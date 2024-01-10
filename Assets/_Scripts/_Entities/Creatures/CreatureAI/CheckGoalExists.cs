using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class CheckGoalExists : TreeNode
  {
    private EnemyController _controller;

    public CheckGoalExists(EnemyController controller) {
      _controller = controller;
    }

    public override TreeNodeState Evaluate() {
      _nodeState = _controller.GetGoalPos() != null ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
      return _nodeState;
    }
  }
}