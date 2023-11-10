namespace _Scripts._BehaviorTree
{
	public class TestVariable : TreeNode
	{
		private string _keyToFind;
		private object _dataToTest;

		public TestVariable(string key, object data) {
			_keyToFind = key;
			_dataToTest = data;
		}

		public override TreeNodeState Evaluate() {
			if (_dataToTest == GetData(_keyToFind)) {
				return TreeNodeState.SUCCESS;
			}
			else {
				return TreeNodeState.FAILED;
			}
		}
	}
}