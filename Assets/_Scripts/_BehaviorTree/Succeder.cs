using System.Collections.Generic;

namespace _Scripts._BehaviorTree
{
	public class Succeder : TreeNode
	{
		public Succeder(TreeNode child) {
			AttachChild(child);
		}

		public override TreeNodeState Evaluate() {
			_children[0].Evaluate();
			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}
	}
}