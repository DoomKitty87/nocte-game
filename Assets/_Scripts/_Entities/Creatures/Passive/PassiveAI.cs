using _Scripts._BehaviorTree;
using UnityEngine;

namespace _Entities.Creatures.Passive
{
  // Define leaf nodes here
  public class DistanceToPlayerLessThan : TreeNode
  {
    private Transform _transform;
    private Transform _playerTransform;
    private float _distance;
    public DistanceToPlayerLessThan(Transform transform, Transform playerTransform, float distance) {
      _transform = transform;
      _playerTransform = playerTransform;
      _distance = distance;
    }
    
    public override TreeNodeState Evaluate() {
      Vector3 position = _transform.position;
      Vector3 playerPosition = _playerTransform.position;
      if (Vector3.Distance(position, playerPosition) < _distance) {
        _nodeState = TreeNodeState.SUCCESS;
      }
      else {
        _nodeState = TreeNodeState.FAILED;
      }
      return _nodeState;
    }
  }

  public class NavigateToPosition : TreeNode
  {
    private Transform _transform;
  }
  
  public class PassiveAI : BehaviorTree
  {
    // Tree Wide variables
    [Header("References")]
    [SerializeField] private Transform _playerTransform;
    [Header("Settings")]
    public static float _distanceWatch;
    public static float _watchTime;
    public static float _distanceEvade;
    protected override TreeNode SetupTree() {
      return new Sequencer(new() {
        new DistanceToPlayerLessThan(transform, _playerTransform, _distanceEvade)
      });
    }
  }
}
