using UnityEngine;

public class GameHandler : MonoBehaviour
{
  public static GameHandler Instance { get; private set; }

  private void Awake()
  {
    if (Instance == null) {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
  }

}