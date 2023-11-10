namespace _Scripts._BehaviorTree
{
	public class SetVariable : TreeNode
	{
		private string _keyToSet;
		private object _dataToSet;
		private int _nodesUpstreamToSet;

		public SetVariable(string key, object data, int nodesUpstream) {
			_keyToSet = key;
			_dataToSet = data;
			_nodesUpstreamToSet = nodesUpstream;
		}

		public override TreeNodeState Evaluate() {
			SetData(_nodesUpstreamToSet, _keyToSet, _dataToSet);
			return TreeNodeState.SUCCESS;
		}
	}
}