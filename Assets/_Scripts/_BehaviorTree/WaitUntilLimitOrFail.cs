using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public class WaitUntilLimitOrFail : TreeNode
	{
		// Runs under the assumption that Evaluate will be called each frame.
		// Gonna have to be changed somehow if this isn't the case
		private const int MAX_ITERATIONS = 10000;
		private float _timeLimit;
		public WaitUntilLimitOrFail(float limitSeconds, TreeNode childNode) {
			_timeLimit = limitSeconds;
			AttachChild(childNode);
		}

		public override TreeNodeState Evaluate() {
			if (IsOverTimeLimit()) {
				_nodeState = TreeNodeState.SUCCESS;
				return _nodeState;
			}
			switch (_children[0].Evaluate()) {
				case TreeNodeState.FAILED:
					_nodeState = TreeNodeState.SUCCESS;
					return _nodeState;
				case TreeNodeState.SUCCESS:
					_nodeState = TreeNodeState.RUNNING;
					return _nodeState;
				case TreeNodeState.RUNNING:
					_nodeState = TreeNodeState.RUNNING;
					return _nodeState;
			}
			throw new Exception("BehaviorTree:WaitUntilLimitOrFail: Evaluated child node beyond MAX_ITERATIONS! If this is intended, increase MAX_ITERATIONS in _Scripts/_BehaviorTree/WaitUntilLimitOrFail.cs");
		}

		private bool IsOverTimeLimit() {
			if (GetData("timePassed") == null) SetLocalData("timePassed", 0);
			
			float timePassed = (float)GetData("timePassed");
			if (timePassed < _timeLimit) {
				SetLocalData("timePassed", timePassed + Time.deltaTime);
				return false;
			}
			// else
			SetLocalData("timePassed", null);
			return true;

		}
	}
}