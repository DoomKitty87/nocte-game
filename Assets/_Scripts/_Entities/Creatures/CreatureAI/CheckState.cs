using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class CheckState : TreeNode
  {
    private EnemyController _controller;
    private EnemyController.PlayerStates _playerState;

    public CheckState(EnemyController controller, EnemyController.PlayerStates playerState) {
      _controller = controller;
      _playerState = playerState;
    }

    public override TreeNodeState Evaluate() {
      if (_controller._state == _playerState) {
        return TreeNodeState.SUCCESS;
      } 
      return TreeNodeState.FAILED;
    }
  }
}