using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	[Serializable]
	public enum TreeNodeState
	{	
		SUCCESS,
		FAILED,
		RUNNING,
	}
	
	// Base Node
	[Serializable]
	public abstract class TreeNode
	{
		// State
		[SerializeField] protected TreeNodeState _nodeState;
		
		// Node Relations
		[SerializeField] public TreeNode _parent;
		[SerializeField] protected List<TreeNode> _children = new List<TreeNode>();
		public void AttachChild(TreeNode childNode) {
			childNode._parent = this;
			_children.Add(childNode);
		}
		
		// Data
		[SerializeField] private Dictionary<string, object> _localData = new Dictionary<string, object>();

		public void SetLocalData(string key, object data) {
			_localData[key] = data;
		}
		public void SetData(int nodesUpstream, string key, object data) {
			TreeNode selectedNode = this;
			for (int i = 0; i < nodesUpstream; i++) {
				if (selectedNode._parent == null) {
					Debug.LogWarning("BehaviorTree:TreeNode: Node called SetData with nodesUpstream out of range! Data was set in root node. Reduce nodesUpstream for cleanliness.");
					break;
				} 
				selectedNode = selectedNode._parent;
			}
			selectedNode.SetLocalData(key, data);
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
		
		// Anything that needs to be run the first time the node is visited
		public virtual void Init() {
			throw new NotImplementedException();
		}
		// Base Evaluation - Can be run once per frame or multiple times per frame.
		public abstract TreeNodeState Evaluate();

		// Anything that needs to be consistently run each fixed update.
		public virtual TreeNodeState FixedEvaluate() {
			throw new NotImplementedException();
		}
		// Anything that needs to be consistently run each frame
		public virtual TreeNodeState Update() {
			throw new NotImplementedException();
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