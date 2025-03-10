using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
  // Ideally this should not handle scene loading - loadSceneSmoothly should.
  public static bool _inGame { get; private set; } = false;

  [SerializeField] private string _menuScene;
  [SerializeField] private string _gameScene;

  private static SceneHandler _instance;

  public static SceneHandler Instance { get {return _instance; } }

  private void OnEnable() {
    if (_instance == null) {
      _instance = this;
    } else {
      Destroy(this);
    }

    if (SceneManager.GetActiveScene().name == _gameScene) {
      _inGame = true;
    }
  }

  public void EnterGame() {
    _inGame = true;
    PlayerMetaProgression.Instance.SaveData();
    SceneManager.LoadScene(_gameScene);
  }

  public void ExitToMenu() {
    Debug.Log("Exiting to menu");
    PlayerMetaProgression.Instance.SaveData();
    StartCoroutine(ToMenu());
  }

  private IEnumerator ToMenu() {
    if (_inGame) {
      // if (CheckJobs()) {
      //   yield return new WaitUntil(() => !CheckJobs());
      // }
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
      _inGame = false;
      SceneManager.LoadScene(_menuScene);
      yield return null;
    }
  }

  private bool CheckJobs() {
    return WorldGenInfo._worldGenerator.IsGenerating();
  }
}
