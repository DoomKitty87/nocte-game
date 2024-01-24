using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class UseAttack : TreeNode
  {
    private CreatureCombat _creatureCombat;
    private CreatureAttack _attack;

    public UseAttack(CreatureCombat creatureCombat, CreatureAttack attack) {
      _creatureCombat = creatureCombat;
      _attack = attack;
    }

    public override TreeNodeState Evaluate() {
      _creatureCombat.Attack(_attack);
      return TreeNodeState.SUCCESS;
    }
  }
}