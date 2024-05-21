using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// This should be replaced with a death screen later
public class PlayerInterface : MonoBehaviour
{
  private IEnumerator PlayerDeathCoroutine() {
    yield return new WaitForSeconds(0f);
    SubmitRunScore();
    SceneHandler.Instance.ExitToMenu();
  }
  public void PlayerDeath(float delay = 0) {
    Invoke(nameof(KillPlayer), delay);
  }

  public void KillPlayer() {
    StartCoroutine(PlayerDeathCoroutine());
  }

  public void LeaveGame() {
    SceneHandler.Instance.ExitToMenu();
  }

  private void SubmitRunScore() {
    if (!PlayerPrefs.HasKey("UID")) {
      PlayerPrefs.SetInt("UID", Random.Range(0, 1000000));
    }
    string userId = PlayerPrefs.GetInt("UID").ToString();
    int score = (int)PlayerExperience.Instance.CheckExperience();

    LeaderboardHandler.AddScore(userId, score);
  }

}
