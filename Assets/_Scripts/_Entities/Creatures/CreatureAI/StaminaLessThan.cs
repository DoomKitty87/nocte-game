using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class StaminaLessThan : TreeNode
  {
    private EnemyController _controller;
    private float _limit;

    public StaminaLessThan(EnemyController controller, float limit) {
      _controller = controller;
      _limit = limit;
    }

    public override TreeNodeState Evaluate() {
      _nodeState = _controller._currentStamina < _limit ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
      return _nodeState;
    }
  }
}