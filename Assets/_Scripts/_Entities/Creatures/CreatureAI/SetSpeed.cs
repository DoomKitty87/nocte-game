using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class SetSpeed : TreeNode
  {
    private EnemyController _controller;
    private int _speed;

    public SetSpeed(EnemyController controller, int speed) {
      _controller = controller;
      _speed = speed;
    }

    public override TreeNodeState Evaluate() {
      _controller.SetMoveSpeed(_speed);
      _nodeState = TreeNodeState.SUCCESS;
      return _nodeState;
    }
  }
}