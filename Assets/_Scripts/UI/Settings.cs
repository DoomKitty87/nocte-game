using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class Settings : MonoBehaviour
{

  public static Settings Instance { get; private set; }

  [SerializeField] private AudioMixer _audioMixer;
  [SerializeField] private VolumeProfile _globalVolumeProfile;

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
    _audioMixer.SetFloat("MusicVolume", Mathf.Log10(MusicVolume) * 20);
  }

  private void UpdateSfxVolume() {
    _audioMixer.SetFloat("SFXVolume", Mathf.Log10(SfxVolume) * 20);
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
      return PlayerPrefs.GetInt("FoliageQuality", 0);
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
      return PlayerPrefs.GetInt("TerrainQuality", 0);
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
    WorldGenInfo._foliageQuality = FoliageQuality;
  }

  private void UpdateTerrainQuality() {
    WorldGenInfo._terrainQuality = TerrainQuality;
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
      return PlayerPrefs.GetFloat("Brightness", 0f);
    }
    set
    {
      PlayerPrefs.SetFloat("Brightness", value);
      Instance.UpdateBrightness();
    }
  }

  public static int ColorblindMode
  {
    get
    {
      return PlayerPrefs.GetInt("ColorblindMode", 0);
    }
    set
    {
      PlayerPrefs.SetInt("ColorblindMode", value);
      Instance.UpdateColorblindMode();
    }
  }

  private void UpdateVsync() {
    QualitySettings.vSyncCount = EnableVsync ? 1 : 0;
  }

  private void UpdateFullscreen() {
    Screen.fullScreen = Fullscreen;
  }

  private void UpdateBrightness() {
    _globalVolumeProfile.TryGet(out LiftGammaGain liftGammaGain);
    liftGammaGain.gamma.value = new Vector4(Brightness, Brightness, Brightness, 1);
  }

  private void UpdateColorblindMode() {
    _globalVolumeProfile.TryGet(out ChannelMixer channelMixer);
    switch (ColorblindMode) {
      case 0:
        channelMixer.redOutRedIn.overrideState = false;
        channelMixer.greenOutGreenIn.overrideState = false;
        channelMixer.blueOutBlueIn.overrideState = false;
        break;
      case 1:
        // Red-Green (Protanopia)
        channelMixer.redOutRedIn.overrideState = true;
        channelMixer.greenOutGreenIn.overrideState = true;
        channelMixer.blueOutBlueIn.overrideState = false;
        channelMixer.redOutRedIn.value = 150;
        channelMixer.greenOutGreenIn.value = 50;
        break;
      case 2:
        // Blue-Yellow (Tritanopia)
        channelMixer.redOutRedIn.overrideState = true;
        channelMixer.greenOutGreenIn.overrideState = true;
        channelMixer.blueOutBlueIn.overrideState = true;
        channelMixer.blueOutBlueIn.value = 150;
        channelMixer.redOutRedIn.value = 75;
        channelMixer.greenOutGreenIn.value = 75;
        break;
    }
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
    UpdateBrightness();
    UpdateColorblindMode();

  }

  private void Awake() {
    Instance = this;
  }

}