using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class CheckAttackRecent : TreeNode
  {
    private CreatureCombat _creatureCombat;
    private float _recencyLimit;

    public CheckAttackRecent(CreatureCombat creatureCombat, float recencyLimit) {
      _creatureCombat = creatureCombat;
      _recencyLimit = recencyLimit;
    }

    public override TreeNodeState Evaluate() {
      float timeSinceLastAttack = Time.time - _creatureCombat._timeOfLastAttack;
      _nodeState = timeSinceLastAttack > _recencyLimit ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
      return _nodeState;
    }
  }
}