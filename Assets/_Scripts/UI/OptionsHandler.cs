using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour
{

  [SerializeField] private Slider _masterVolumeSlider;
  [SerializeField] private Slider _musicVolumeSlider;
  [SerializeField] private Slider _sfxVolumeSlider;


  [SerializeField] private Toggle _cloudsToggle;
  [SerializeField] private Dropdown _foliageDropdown;
  [SerializeField] private Dropdown _terrainDropdown;


  [SerializeField] private Toggle _invertMouseToggle;
  [SerializeField] private Slider _mouseSensitivitySlider;
  [SerializeField] private Slider _adsMultiplierSlider;


  [SerializeField] private Toggle _vsyncToggle;
  [SerializeField] private Toggle _fullscreenToggle;
  [SerializeField] private Slider _brightnessSlider;


  private void Start() {
    LoadSettingsValues();
  }

  private void LoadSettingsValues() {
    _masterVolumeSlider.value = Settings.MasterVolume;
    _musicVolumeSlider.value = Settings.MusicVolume;
    _sfxVolumeSlider.value = Settings.SfxVolume;

    _cloudsToggle.isOn = Settings.EnableClouds;
    _foliageDropdown.value = Settings.FoliageQuality;
    _terrainDropdown.value = Settings.TerrainQuality;

    _invertMouseToggle.isOn = Settings.InvertMouse;
    _mouseSensitivitySlider.value = Settings.MouseSensitivity;
    _adsMultiplierSlider.value = Settings.AdsMultiplier;

    _vsyncToggle.isOn = Settings.EnableVsync;
    _fullscreenToggle.isOn = Settings.Fullscreen;
    _brightnessSlider.value = Settings.Brightness;
  }

}
