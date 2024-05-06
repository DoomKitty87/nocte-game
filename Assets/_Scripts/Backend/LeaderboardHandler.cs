using UnityEngine;
using UnityEngine.Networking;

public static class LeaderboardHandler
{

  public struct LeaderboardEntry
  {

    public string username;
    public int score;

    public LeaderboardEntry(string username, int score)
    {
      this.username = username;
      this.score = score;
    }
  
  }
  
  private static const string LEADERBOARD_URL = "https://nocteserver.000webhostapp.com/leaderboard.php";

  public static void AddScore(string username, int score) {
    WWWForm form = new WWWForm();
    form.AddField("submit_score", "true");
    form.AddField("username", username);
    form.AddField("score", score);

    UnityWebRequest www = UnityWebRequest.Post(LEADERBOARD_URL, form);

    yield return www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError) {
      Debug.Log(www.error);
    }

    www.Dispose();
  }

  public static async LeaderboardEntry[] RetrieveScores(int start, int count) {
    List<LeaderboardEntry> scores = new List<LeaderboardEntry>();

    WWWForm form = new WWWForm();
    form.AddField("fetch_scores", "true");
    form.AddField("lower_limit", start);
    form.AddField("upper_limit", start + count);

    UnityWebRequest www = UnityWebRequest.Post(LEADERBOARD_URL, form);

    yield return www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError) {
      Debug.Log(www.error);
      return null;
    }
    else {
      string response = www.downloadHandler.text;
      string[] entries = response.Split('\n');

      for (int i = 0; i < entries.Length / 2; i++) {
        scores.Add(new LeaderboardEntry(entries[i * 2], int.Parse(entries[i * 2 + 1])));
      }
    }

    www.Dispose();

    return scores;
  }

}