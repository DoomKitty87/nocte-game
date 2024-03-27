using System;
using UnityEngine;

public class InitiatePlayer : MonoBehaviour
{
  private GameObject _gameHandler;

  [SerializeField]
  private Transform _player;

  private static Transform _playerTransform;
  public static Transform PlayerTransform { get { return _playerTransform; } }

  private void Awake() {
    if (GameObject.FindWithTag("GameHandler") == null) {
      _gameHandler = Resources.Load<GameObject>("Game Handler/GameHandler");
      Instantiate(_gameHandler);
      DontDestroyOnLoad(_gameHandler);
    }
    else {
      _gameHandler = GameObject.FindWithTag("GameHandler");
      DontDestroyOnLoad(_gameHandler);
    }
    _playerTransform = _player;
    int numberOfChildren = transform.childCount;
    for (int i = 0; i < numberOfChildren; i++) {
      transform.GetChild(0).SetParent(null);
    }
    
    Destroy(transform.gameObject);
  }
}
