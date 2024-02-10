using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneAsync : MonoBehaviour
{
    [SerializeField] private string _sceneToLoad;
    [SerializeField] private bool _unload;

    private void LoadScenes() {
        if (IsSceneCurrentlyLoaded()) return;
        SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
    }

    private void UnloadScenes() {
        if (!IsSceneCurrentlyLoaded()) return;
        SceneManager.UnloadSceneAsync(_sceneToLoad);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (!_unload)
                LoadScenes();
            else 
                UnloadScenes();
        }
    }

    private bool IsSceneCurrentlyLoaded() {
        Scene sceneToGet = SceneManager.GetSceneByName(_sceneToLoad);
        int numberOfScenes = SceneManager.loadedSceneCount;

        for (int i = 0; i < numberOfScenes; i++) {
            if (SceneManager.GetSceneAt(i) == sceneToGet) return true;
        }

        return false;
    }

}
