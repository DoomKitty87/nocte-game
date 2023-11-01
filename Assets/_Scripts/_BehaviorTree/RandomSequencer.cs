using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	public class RandomSequencer : TreeNode
	{
		// RandomSequencer
		//
		// Will evaluate each child in a random order, starting with the first,
		// and when that succeeds will call the second, and so on
		// down the list of children.
		// If any child fails it will immediately return failure to
		// the parent. If the last child in the sequence succeeds,
		// then the sequence will return success to its parent.

		public override TreeNodeState Evaluate() {
			_children = RandomizeNodeList(_children);
			foreach (TreeNode child in _children) {
				switch (child.Evaluate()) {
					case TreeNodeState.FAILED:
						_nodeState = TreeNodeState.FAILED;
						return _nodeState;
					case TreeNodeState.COMPLETED:
						continue;
					case TreeNodeState.RUNNING:
						_nodeState = TreeNodeState.RUNNING;
						return _nodeState;
				}
			}
			_nodeState = TreeNodeState.COMPLETED;
			return _nodeState;
		}
		
		// Helper Functions
		private List<TreeNode> RandomizeNodeList(List<TreeNode> list) {
			throw new System.NotImplementedException();
		}
		
		// Constructors
		public RandomSequencer() : base() {
		}

		public RandomSequencer(List<TreeNode> childNodes) : base(childNodes) {
		}
	}
}
