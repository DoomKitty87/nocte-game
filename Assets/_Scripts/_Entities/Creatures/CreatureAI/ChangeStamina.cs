using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class ChangeStamina : TreeNode
  {
    private EnemyController _controller;
    private float _value;

    public ChangeStamina(EnemyController controller, float value) {
      _controller = controller;
      _value = value;
    }

    public override TreeNodeState Evaluate() {
      _controller._currentStamina += _value;
      return TreeNodeState.SUCCESS;
    }
  }
}