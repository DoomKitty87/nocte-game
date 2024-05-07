using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

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

  public static LeaderboardEntry[] _lastScores;
  public static bool _scoresDirty = true;
  
  private const string LEADERBOARD_URL = "https://nocteserver.000webhostapp.com/leaderboard.php";

  public static async void AddScore(string username, int score) {
    WWWForm form = new WWWForm();
    form.AddField("submit_score", "true");
    form.AddField("username", username);
    form.AddField("score", score);

    UnityWebRequest www = UnityWebRequest.Post(LEADERBOARD_URL, form);

    await www.SendWebRequest();

    if (www.result == UnityWebRequest.Result.ConnectionError) {
      Debug.Log(www.error);
    }

    // Debug.Log(www.responseCode);
    // Debug.Log(www.downloadHandler.text);

    www.Dispose();
  }

  public static async void RetrieveScores(int start, int count) {
    _scoresDirty = true;
    List<LeaderboardEntry> scores = new List<LeaderboardEntry>();

    WWWForm form = new WWWForm();
    form.AddField("fetch_scores", "true");
    form.AddField("lower_limit", start);
    form.AddField("upper_limit", start + count);

    UnityWebRequest www = UnityWebRequest.Post(LEADERBOARD_URL, form);

    await www.SendWebRequest();

    if (www.result == UnityWebRequest.Result.ConnectionError) {
      Debug.Log(www.error);
    }
    else {
      string response = www.downloadHandler.text;
      string[] entries = response.Split('\n');

      for (int i = 0; i < entries.Length / 2; i++) {
        scores.Add(new LeaderboardEntry(entries[i * 2], int.Parse(entries[i * 2 + 1])));
      }
    }

    www.Dispose();

    _lastScores = scores.ToArray();
    _scoresDirty = false;
  }

}