using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class CheckGoalExists : TreeNode
  {
    private Transform _transform;
    private float _distance;

    public CheckGoalExists() {
      // :)
    }

    public override TreeNodeState Evaluate() {
      _nodeState = GetData("goal") == null ? TreeNodeState.FAILED : TreeNodeState.SUCCESS;
      return _nodeState;
    }
  }
}