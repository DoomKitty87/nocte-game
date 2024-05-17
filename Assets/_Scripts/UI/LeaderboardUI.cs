using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
  
  public GameObject entryPrefab;
  public Transform entryParent;

  private void Start() {
    LeaderboardHandler.RetrieveScores(0, 20);
    StartCoroutine(UpdateLeaderboard());
  }

  private IEnumerator UpdateLeaderboard() {
    yield return new WaitUntil(() => !LeaderboardHandler._scoresDirty);
    foreach (LeaderboardHandler.LeaderboardEntry entry in LeaderboardHandler._lastScores) {
      GameObject entryObject = Instantiate(entryPrefab, entryParent);
      // Set the entryObject's text to the entry's username and score
      var texts = entryObject.GetComponentsInChildren<TextMeshProUGUI>();
      texts[0].text = entry.username;
      texts[1].text = entry.score.ToString();
    }
  }

}