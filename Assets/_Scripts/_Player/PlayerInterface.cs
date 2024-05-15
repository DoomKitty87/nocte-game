using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// This should be replaced with a death screen later
public class PlayerInterface : MonoBehaviour
{
  private IEnumerator PlayerDeathCoroutine() {
    yield return new WaitForSeconds(5f);
    SceneHandler.Instance.ExitToMenu();
  }
  public void PlayerDeath() {
    StartCoroutine(PlayerDeathCoroutine());
  }

}
