using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	// Noder
    //
    // Does what I want. I do not care about behavior trees.
	public class Noder : TreeNode
	{
		public override TreeNodeState Evaluate() {
			foreach (TreeNode child in _children) {
				child.Evaluate();
			}
			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}

		// Constructors
		public Noder() : base() {
		}

		public Noder(List<TreeNode> childNodes) : base(childNodes) {
		}

	}
}