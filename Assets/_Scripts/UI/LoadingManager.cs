using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{

  [SerializeField] private String _loadingScene;

  private void Awake() {
    SceneManager.LoadScene(_loadingScene, LoadSceneMode.Additive);
  }

  private void Start() {
    WorldGenerator.GenerationComplete += FinishLoad;
  }

  private void OnDisable() {
    WorldGenerator.GenerationComplete -= FinishLoad;
  }

  public void FinishLoad() {
    SceneManager.UnloadSceneAsync(_loadingScene);
  }

}