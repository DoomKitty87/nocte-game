using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipMenuNavigation : MonoBehaviour
{
  
  [SerializeField] private string _mainMenuScene;
  
  public void EnterGame() {
    SceneHandler.Instance.EnterGame();
  }

  public void ExitToMenu() {
    SceneManager.LoadScene(_mainMenuScene);
  }

}
