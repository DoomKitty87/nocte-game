using System;
using System.Diagnostics;
using System.Collections.Generic;
using _Scripts._BehaviorTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector3 = System.Numerics.Vector3;

namespace _Scripts._Entities.Creatures.CreatureAI
{
  public class PlaceRandomGoal : TreeNode
  {
    private EnemyController _controller;
    private Transform _transform;
    private float _range;
    
    public PlaceRandomGoal(EnemyController controller, Transform transform, float range) {
      _controller = controller;
      _transform = transform;
      _range = range;
    }

    public override TreeNodeState Evaluate() {
      float r = Mathf.Abs(_range * Mathf.Sqrt(Random.value));
      float theta = Random.value * 2 * Mathf.PI;
      float x = _transform.position.x + r * Mathf.Cos(theta);
      float z = _transform.position.z + r * Mathf.Sin(theta); 
      _controller.SetGoalPos(new Vector2(x, z));
      
      _nodeState = TreeNodeState.SUCCESS;
      return _nodeState;
    }
  }
}