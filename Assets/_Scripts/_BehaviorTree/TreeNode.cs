using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	public enum TreeNodeState
	{	
		COMPLETED,
		FAILED,
		RUNNING,
	}
	// Base Node, although can be used as a "failer", not recommended for neatness
	public abstract class TreeNode
	{
		// State
		protected TreeNodeState _nodeState;
		
		// Node Relations
		public TreeNode _parent;
		protected List<TreeNode> _children = new List<TreeNode>();
		public void AttachChild(TreeNode childNode) {
			childNode._parent = this;
			_children.Add(childNode);
		}
		
		// Data
		private Dictionary<string, object> _localData = new Dictionary<string, object>();

		public void SetData(string key, object data) {
			_localData[key] = data;
		}
		public object GetData(string key) {
			object data = null;
			if (_localData.TryGetValue(key, out data)) {
				return data;
			}
			else {
				TreeNode currentNode = _parent;
				while (currentNode != null) {
					if (currentNode._localData.TryGetValue(key, out data)) {
						return data;
					}
					currentNode = currentNode._parent;
				}
				return null;
			}
		}
		public bool ClearData(string key) {
			if (_localData.ContainsKey(key)) {
				_localData.Remove(key);
				return true;
			}
			else {
				TreeNode currentNode = _parent;
				while (currentNode != null) {
					if (currentNode._localData.ContainsKey(key)) {
						currentNode._localData.Remove(key);
						return true;
					}
					currentNode = currentNode._parent;
				}
				return false;
			}
		}
		
		// Evaluation
		public virtual TreeNodeState Evaluate() {
			Debug.LogWarning($"TreeNode: Node has no override for Evaluate! Is this intended?");
			return TreeNodeState.FAILED;
		}
		
		// Constructors
		public TreeNode() {
			_parent = null;
		}
		public TreeNode(List<TreeNode> children) {
			foreach (TreeNode child in children) {
				AttachChild(child);
			}
		}
	}
}