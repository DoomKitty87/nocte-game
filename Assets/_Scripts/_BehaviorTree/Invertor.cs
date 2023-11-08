using System.Collections;
using System.Collections.Generic;
using _Scripts._BehaviorTree;
using UnityEngine;

public class Invertor : TreeNode
{
  public override TreeNodeState Evaluate() {
    switch (_children[0].Evaluate()) {
      case TreeNodeState.FAILED:
        _nodeState = TreeNodeState.SUCCESS;
        break;
      case TreeNodeState.SUCCESS:
        _nodeState = TreeNodeState.FAILED;
        break;
      case TreeNodeState.RUNNING:
        _nodeState = TreeNodeState.RUNNING;
        break;
    }
    return _nodeState;
  }

  // Constructors
  public Invertor() : base() {
  }

  public Invertor(TreeNode childNode) : base(new List<TreeNode>() {childNode}) {
  }
}
