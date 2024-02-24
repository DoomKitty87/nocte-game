using System.Collections.Generic;
using UnityEngine;
using _Scripts._BehaviorTree;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class DistanceDenLessThan : TreeNode
  {
    private EnemyController _controller;
    private Transform _transform;
    private float _distance;

    public DistanceDenLessThan(EnemyController controller, Transform transform, float distance) {
      _controller = controller;
      _transform = transform;
      _distance = distance;
    }

    public override TreeNodeState Evaluate() {
      Vector3 denPosition = _controller.GetDenPos();
      Vector3 enemyPosVec = new Vector3(_transform.position.x, _transform.position.y, _transform.position.z);
      float dist = Vector3.Distance(denPosition, enemyPosVec);
      _nodeState = dist < _distance ? TreeNodeState.SUCCESS : TreeNodeState.FAILED;
      return _nodeState;
    }
  }
}