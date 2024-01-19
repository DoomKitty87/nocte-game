using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class SetState : TreeNode
  {
    private EnemyController _controller;
    private EnemyController.PlayerStates _playerState;

    public SetState(EnemyController controller, EnemyController.PlayerStates playerState) {
      _controller = controller;
      _playerState = playerState;
    }

    public override TreeNodeState Evaluate() {
      _controller.SetState(_playerState);
      _nodeState = TreeNodeState.SUCCESS;
      return _nodeState;
    }
  }
}