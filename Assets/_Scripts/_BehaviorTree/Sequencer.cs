using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
  public class Sequencer : TreeNode
  {
    // Sequencer
    //
    // Will evaluate each child in order, starting with the first,
    // and when that succeeds will call the second, and so on
    // down the list of children.
    // If any child fails it will immediately return failure to
    // the parent. If the last child in the sequence succeeds,
    // then the sequence will return success to its parent.

    public override TreeNodeState Evaluate() {
      foreach (TreeNode child in _children) {
        switch (child.Evaluate()) {
          case TreeNodeState.FAILED:
            _nodeState = TreeNodeState.FAILED;
            return _nodeState;
          case TreeNodeState.SUCCESS:
            continue;
          case TreeNodeState.RUNNING:
            _nodeState = TreeNodeState.RUNNING;
            return _nodeState;
        }
      }
      _nodeState = TreeNodeState.SUCCESS;
      return _nodeState;
    }

    // Constructors
    public Sequencer() : base() {
    }

    public Sequencer(List<TreeNode> childNodes) : base(childNodes) {
    }
  }
}