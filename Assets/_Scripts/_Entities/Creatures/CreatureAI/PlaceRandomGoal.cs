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
    private WorldGenerator _worldGenerator;
    
    public PlaceRandomGoal(EnemyController controller, Transform transform, float range) {
      _controller = controller;
      _transform = transform;
      _range = range;
     _worldGenerator = WorldGenInfo._worldGenerator;
    }

    public override TreeNodeState Evaluate() {
      Vector2 location = ChooseLocation();
      // do {
      //   location = ChooseLocation();
      // } while (_worldGenerator.GetWaterHeight(location) == -1);
      _controller.SetGoalPos(location);
      
      _nodeState = TreeNodeState.SUCCESS;
      return _nodeState;
    }

    private Vector2 ChooseLocation() {
      float r = Mathf.Abs(100 + _range * Mathf.Sqrt(Random.value));
      float theta = Random.value * 2 * Mathf.PI;
      float x = _transform.position.x + r * Mathf.Cos(theta);
      float z = _transform.position.z + r * Mathf.Sin(theta);
      return new Vector2(x, z);
    }
  }
}