using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class Settings : MonoBehaviour
{

  public static Settings Instance { get; private set; }

  #region Audio

  public static float MasterVolume
  {
    get
    {
      return PlayerPrefs.GetFloat("MasterVolume", 1f);
    }
    set
    {
      PlayerPrefs.SetFloat("MasterVolume", value);
      Instance.UpdateMasterVolume();
    }
  }

  public static float MusicVolume
  {
    get
    {
      return PlayerPrefs.GetFloat("MusicVolume", 1f);
    }
    set
    {
      PlayerPrefs.SetFloat("MusicVolume", value);
      Instance.UpdateMusicVolume();
    }
  }

  public static float SfxVolume
  {
    get
    {
      return PlayerPrefs.GetFloat("SfxVolume", 1f);
    }
    set
    {
      PlayerPrefs.SetFloat("SfxVolume", value);
      Instance.UpdateSfxVolume();
    }
  }

  private void UpdateMasterVolume() {
    AudioListener.volume = MasterVolume;
  }

  private void UpdateMusicVolume() {
    // Update music volume
  }

  private void UpdateSfxVolume() {
    // Update sfx volume
  }

  #endregion

  #region Graphics
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

  public static int FoliageQuality
  {
    get
    {
      return PlayerPrefs.GetInt("FoliageQuality", 2);
    }
    set
    {
      PlayerPrefs.SetInt("FoliageQuality", value);
      Instance.UpdateFoliageQuality();
    }
  }

  public static int TerrainQuality
  {
    get
    {
      return PlayerPrefs.GetInt("TerrainQuality", 2);
    }
    set
    {
      PlayerPrefs.SetInt("TerrainQuality", value);
      Instance.UpdateTerrainQuality();
    }
  }

  private void UpdateClouds() {
    if (!SceneHandler._inGame) return;
    VolumetricClouds clouds = WeatherManager.Instance.GetClouds();
    clouds.active = EnableClouds;
  }

  private void UpdateFoliageQuality() {
    // Update foliage quality
  }

  private void UpdateTerrainQuality() {
    // Update terrain quality
  }

  #endregion

  #region Controls

  public static bool InvertMouse
  {
    get
    {
      return PlayerPrefs.GetInt("InvertMouse", 0) == 1;
    }
    set
    {
      PlayerPrefs.SetInt("InvertMouse", value ? 1 : 0);
      Instance.UpdateInvertMouse();
    }
  }

  public static float MouseSensitivity
  {
    get
    {
      return PlayerPrefs.GetFloat("MouseSensitivity", 1f);
    }
    set
    {
      PlayerPrefs.SetFloat("MouseSensitivity", value);
      Instance.UpdateMouseSensitivity();
    }
  }

  public static float AdsMultiplier
  {
    get
    {
      return PlayerPrefs.GetFloat("AdsMultiplier", 0.5f);
    }
    set
    {
      PlayerPrefs.SetFloat("AdsMultiplier", value);
      Instance.UpdateAdsMultiplier();
    }
  }

  private void UpdateInvertMouse() {
    // Update mouse input
  }

  private void UpdateMouseSensitivity() {
    // Update mouse input
  }

  private void UpdateAdsMultiplier() {
    // Update mouse input
  }

  #endregion

  #region Video

  public static bool EnableVsync
  {
    get
    {
      return PlayerPrefs.GetInt("EnableVsync", 1) == 1;
    }
    set
    {
      PlayerPrefs.SetInt("EnableVsync", value ? 1 : 0);
      Instance.UpdateVsync();
    }
  }

  public static bool Fullscreen
  {
    get
    {
      return PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }
    set
    {
      PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
      Instance.UpdateFullscreen();
    }
  }

  public static float Brightness
  {
    get
    {
      return PlayerPrefs.GetFloat("Brightness", 1f);
    }
    set
    {
      PlayerPrefs.SetFloat("Brightness", value);
      Instance.UpdateBrightness();
    }
  }

  private void UpdateVsync() {
    QualitySettings.vSyncCount = EnableVsync ? 1 : 0;
  }

  private void UpdateFullscreen() {
    Screen.fullScreen = Fullscreen;
  }

  private void UpdateBrightness() {
    // Update brightness
  }

  #endregion

  public void LoadSettings() {
    // Audio
    UpdateMasterVolume();
    UpdateMusicVolume();
    UpdateSfxVolume();

    // Graphics
    UpdateClouds();
    UpdateFoliageQuality();
    UpdateTerrainQuality();

    // Controls
    UpdateInvertMouse();
    UpdateMouseSensitivity();
    UpdateAdsMultiplier();

    // Video
    UpdateVsync();
    UpdateFullscreen();

  }

  private void Awake() {
    Instance = this;
  }

}