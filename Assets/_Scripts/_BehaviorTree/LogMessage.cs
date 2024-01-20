using UnityEngine;

namespace _Scripts._BehaviorTree
{
	public class LogMessage : TreeNode
	{
		private string _message;
		private LogType _type;
		
		public enum LogType
		{
			Info,
			Warning,
			Error,
		}
		
		public LogMessage(LogType type, string message) {
			_type = type;
			_message = message;
		}	
		
		public override TreeNodeState Evaluate() {
			switch (_type) {
				case LogType.Info:
					// Debug.Log(_message);
					break;
				case LogType.Warning:
					// Debug.LogWarning(_message);
					break;
				case LogType.Error:
					// Debug.LogError(_message);
					break;
			}
			_nodeState = TreeNodeState.SUCCESS;
			return _nodeState;
		}
	}
}