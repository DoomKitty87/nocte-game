using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
  
  public GameObject entryPrefab;
  public Transform entryParent;

  private void Start() {
    LeaderboardHandler.RetrieveScores().ContinueWith(task => {
      if (task.Result != null) {
        foreach (LeaderboardHandler.LeaderboardEntry entry in task.Result) {
          GameObject entryObject = Instantiate(entryPrefab, entryParent);
          entryObject.GetComponent<LeaderboardEntryUI>().SetEntry(entry);
        }
      }
    });
  }

}