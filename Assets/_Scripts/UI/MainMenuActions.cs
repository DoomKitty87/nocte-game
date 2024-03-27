using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuActions : MonoBehaviour
{

  [SerializeField] private string _shipScene;

  public void ExitGame() {
    Application.Quit();
  }

  public void StartGame() {
    SceneManager.LoadScene(_shipScene);
  }

}
