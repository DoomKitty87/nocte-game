using UnityEngine;

public class RunScore : MonoBehaviour
{
  
  private void Start() {
    SubmitScore();
  }

  public void SubmitScore() {
    LeaderboardHandler.AddScore("Player", 100);
  }

}