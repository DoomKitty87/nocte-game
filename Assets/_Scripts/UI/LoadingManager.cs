using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{

  [SerializeField] private String _loadingScene;
  [SerializeField] private Camera _disableDuringLoad;

  public void Awake() {
    _disableDuringLoad.enabled = false;
    SceneManager.LoadSceneAsync(_loadingScene, LoadSceneMode.Additive);
    WorldGenerator.GenerationComplete += FinishLoad;
  }

  private void OnDisable() {
    WorldGenerator.GenerationComplete -= FinishLoad;
  }

  public void FinishLoad() {
    _disableDuringLoad.enabled = true;
    SceneManager.UnloadSceneAsync(_loadingScene);
  }

}