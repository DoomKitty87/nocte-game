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
  [SerializeField] private Dropdown _colorblindDropdown;


  private void Start() {
    LoadSettingsValues();
    AddListeners();
  }

  private void LoadSettingsValues() {
    _masterVolumeSlider.value = Settings.MasterVolume;
    _musicVolumeSlider.value = Settings.MusicVolume;
    _sfxVolumeSlider.value = Settings.SfxVolume;

    _cloudsToggle.isOn = Settings.EnableClouds;
    //_foliageDropdown.value = Settings.FoliageQuality;
    //_terrainDropdown.value = Settings.TerrainQuality;

    _invertMouseToggle.isOn = Settings.InvertMouse;
    _mouseSensitivitySlider.value = Settings.MouseSensitivity;
    _adsMultiplierSlider.value = Settings.AdsMultiplier;

    _vsyncToggle.isOn = Settings.EnableVsync;
    _fullscreenToggle.isOn = Settings.Fullscreen;
    _brightnessSlider.value = Settings.Brightness;
    //_colorblindDropdown.value = Settings.ColorblindMode;
  }

  private void AddListeners() {
    _masterVolumeSlider.onValueChanged.AddListener((value) => Settings.MasterVolume = value);
    _musicVolumeSlider.onValueChanged.AddListener((value) => Settings.MusicVolume = value);
    _sfxVolumeSlider.onValueChanged.AddListener((value) => Settings.SfxVolume = value);

    _cloudsToggle.onValueChanged.AddListener((value) => Settings.EnableClouds = value);
    //_foliageDropdown.onValueChanged.AddListener((value) => Settings.FoliageQuality = value);
    //_terrainDropdown.onValueChanged.AddListener((value) => Settings.TerrainQuality = value);

    _invertMouseToggle.onValueChanged.AddListener((value) => Settings.InvertMouse = value);
    _mouseSensitivitySlider.onValueChanged.AddListener((value) => Settings.MouseSensitivity = value);
    _adsMultiplierSlider.onValueChanged.AddListener((value) => Settings.AdsMultiplier = value);

    _vsyncToggle.onValueChanged.AddListener((value) => Settings.EnableVsync = value);
    _fullscreenToggle.onValueChanged.AddListener((value) => Settings.Fullscreen = value);
    _brightnessSlider.onValueChanged.AddListener((value) => Settings.Brightness = value);
    //_colorblindDropdown.onValueChanged.AddListener((value) => Settings.ColorblindMode = value);
  }

}
