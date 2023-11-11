using System;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
	public abstract class BehaviorTree : MonoBehaviour
	{
		public enum UpdateType
		{
			Update,
			FixedUpdate,
			LateUpdate,
		}

		[SerializeField] private UpdateType _updateType; 
		private TreeNode _rootNode = null;
		
		private void Start() {
			_rootNode = SetupTree();
		}
		
		private void Update() {
			if (_updateType != UpdateType.Update) return;
			_rootNode.Evaluate();
		}

		private void FixedUpdate() {
			if (_updateType != UpdateType.FixedUpdate) return;
			_rootNode.Evaluate();
		}
		
		private void LateUpdate() {
			if (_updateType != UpdateType.LateUpdate) return;
			_rootNode.Evaluate();
		}

		protected abstract TreeNode SetupTree();
	}
}