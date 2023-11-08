using System;
using UnityEngine;

namespace _Scripts._BehaviorTree
{
  public abstract class BehaviorTree : MonoBehaviour
  {
    [SerializeField]
    private TreeNode _rootNode = null;

    private void Start() {
      _rootNode = SetupTree();
    }
    private void Update() {
      _rootNode.Evaluate();
    }

    protected abstract TreeNode SetupTree();
  }
}