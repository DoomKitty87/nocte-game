using System;
using UnityEngine;

public class InitiatePlayer : MonoBehaviour
{
  private GameObject _gameHandler;

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

    int numberOfChildren = transform.childCount;
    for (int i = 0; i < numberOfChildren; i++) {
      transform.GetChild(0).SetParent(null);
    }
    
    Destroy(transform.gameObject);
  }
}
