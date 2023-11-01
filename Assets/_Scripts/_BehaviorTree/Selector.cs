using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	// Selector
	//
	// Will return a success if any of its children succeed and
	// not process any further children. It will process the first
	// child, and if it fails will process the second, and if that
	// fails will process the third, until a success is reached,
	// at which point it will instantly return success. It will
	// fail if all children fail.
	public class Selector : TreeNode
	{
		public override TreeNodeState Evaluate() {
			foreach (TreeNode child in _children) {
				switch (child.Evaluate()) {
					case TreeNodeState.FAILED:
						continue;
					case TreeNodeState.COMPLETED:
						_nodeState = TreeNodeState.COMPLETED;
						return _nodeState;
					case TreeNodeState.RUNNING:
						_nodeState = TreeNodeState.RUNNING;
						return _nodeState;
					default:
						Debug.LogError($"TreeNode:Selector: Child {child.GetType()} did not return a TreeNodeState! Selector will continue to next child.");
						continue;
				}
			}
			_nodeState = TreeNodeState.FAILED;
			return _nodeState;
		}

		// Constructors
		public Selector() : base() {
		}

		public Selector(List<TreeNode> childNodes) : base(childNodes) {
		}

	}
}