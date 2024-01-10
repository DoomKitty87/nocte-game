using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class CheckStuck : TreeNode
  {
    private EnemyController _controller;

    public CheckStuck(EnemyController controller) {
      _controller = controller;
    }

    public override TreeNodeState Evaluate() {
      _nodeState = TreeNodeState.SUCCESS;
      return _nodeState;
    }
  }
}