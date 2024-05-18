using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class FindFood : TreeNode
	{
		private EnemyController _controller;
		private List<Vector2Int> _foodLocations;

		public FindFood(EnemyController controller, List<Vector2Int> foodLocations) {
			_controller = controller;
			_foodLocations = foodLocations;
		}

		public override TreeNodeState Evaluate() {
            float bestDistance = Mathf.Infinity;
            Vector2Int bestPoint;
            for (int i = 0; i < _foodLocations.Count; i++) {
                float distance = Vector2.Distance(_foodLocations[i], new Vector2(_controller.transform.position.x, _controller.transform.position.y));
                if (distance < bestDistance) {
                    bestPoint = _foodLocations[i];
                }
            }
            return TreeNodeState.SUCCESS;
		}
	}
}