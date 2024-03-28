using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class Settings : MonoBehaviour
{

  public static Settings Instance { get; private set; }

  public static bool EnableClouds
  {
    get
    {
      return PlayerPrefs.GetInt("EnableClouds", 1) == 1;
    }
    set
    {
      PlayerPrefs.SetInt("EnableClouds", value ? 1 : 0);
      Instance.UpdateClouds();
    }
  }

  public void LoadSettings() {
    UpdateClouds();
  }

  private void Awake() {
    Instance = this;
  }

  private void UpdateClouds() {
    if (!SceneHandler._inGame) return;
    VolumetricClouds clouds = WeatherManager.Instance.GetClouds();
    clouds.active = EnableClouds;
  }

}