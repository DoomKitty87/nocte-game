using _Scripts._BehaviorTree;
using UnityEngine;

namespace _Scripts._Entities.Creatures.CreatureAI
{
	public class AttackWithCreatureCombat : TreeNode
	{
		private CreatureCombat _creatureCombat;
		private CreatureAttack _creatureAttack;
		public AttackWithCreatureCombat(CreatureCombat creatureCombat, CreatureAttack creatureAttack) {
			_creatureCombat = creatureCombat;
			_creatureAttack = creatureAttack;
		}

		public override TreeNodeState Evaluate() {
			float timeSinceLastAttack;
			if (GetData("timeSinceLastAttack") == null) {
				SetLocalData("timeSinceLastAttack", 0f);
				timeSinceLastAttack = 0f;
			}
			else {
				// unless 0 is defined as a float with 0f this cast is "invalid"
				timeSinceLastAttack = (float)GetData("timeSinceLastAttack");
			}
			if (timeSinceLastAttack > _creatureAttack._attackRepeatSeconds) {
				_creatureCombat.Attack(_creatureAttack);
				SetLocalData("timeSinceLastAttack", 0f);
				_nodeState = TreeNodeState.SUCCESS;
				return _nodeState;
			}
			SetLocalData("timeSinceLastAttack", timeSinceLastAttack + Time.deltaTime);
			_nodeState = TreeNodeState.FAILED;
			return _nodeState;
		}
	}
}