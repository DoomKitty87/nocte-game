using UnityEngine;

public class GameHandler : MonoBehaviour
{
  public static GameHandler Instance { get; private set; }

  private void Start() {
	  Invoke(nameof(Initialize), 0.5f);
  }

  private void Initialize() {
		UpgradeInfo.Initialize();
	}

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