using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class PlaceDen : TreeNode
  {
    private EnemyController _controller;
    private Transform _transform;

    public PlaceDen(EnemyController controller, Transform transform) {
      _controller = controller;
      _transform = transform;
    }

    public override TreeNodeState Evaluate() {
      _controller.SetDenPos(_transform.position);
      GameObject den = GameObject.CreatePrimitive(PrimitiveType.Cube);
      den.transform.position = _transform.position;
      return TreeNodeState.SUCCESS;
    }
  }
}