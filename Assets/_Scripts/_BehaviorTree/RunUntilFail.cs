using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	public class RunUntilFail : TreeNode
	{
		// Yeah yeah, this type of node shouldn't ever return TreeNodeState.FAILED, but
		// I really don't want to run into an undebugable infinite while loop.
		private const int MAX_ITERATIONS = 10000;
		
		public RunUntilFail(TreeNode childNode) {
			AttachChild(childNode);
		}

		public override TreeNodeState Evaluate() {
			for (int i = 0; i < MAX_ITERATIONS; i++) {
				switch (_children[0].Evaluate()) {
					case TreeNodeState.FAILED:
						_nodeState = TreeNodeState.SUCCESS;
						return _nodeState;
					case TreeNodeState.SUCCESS:
						continue;
					case TreeNodeState.RUNNING:
						_nodeState = TreeNodeState.RUNNING;
						return _nodeState;
				}
			}
			throw new Exception("BehaviorTree:RunUntilFail: Evaluated child node beyond MAX_ITERATIONS! If this is intended, increase MAX_ITERATIONS in _Scripts/_BehaviorTree/RunUntilFail.cs");
		}
	}
}