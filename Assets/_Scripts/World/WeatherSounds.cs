using UnityEngine;
using System.Collections;

public class WeatherSounds : MonoBehaviour
{

  [System.Serializable]
  public struct WeatherLevel
  {
    public AudioClip clip;
    public float volume;
    public float threshold;
  }

  [SerializeField] private WeatherLevel[] _rainWeatherLevels;

  [SerializeField] private AudioSource _rainAudioSource;
  private float _currentVolume;

  public static WeatherSounds Instance { get; private set; }

  private void Awake() {
    Instance = this;
  }

  public void UpdateWeather(float rainLevel) {
    for (int i = 0; i < _rainWeatherLevels.Length; i++) {
      if (rainLevel >= _rainWeatherLevels[i].threshold) {
        _rainAudioSource.clip = _rainWeatherLevels[i].clip;
        _rainAudioSource.volume = _rainWeatherLevels[i].volume;
        _currentVolume = _rainWeatherLevels[i].volume;
        if (!_rainAudioSource.isPlaying) StartCoroutine(FadeInRain());
        return;
      }
    }
    StartCoroutine(FadeOutRain());
  }

  private IEnumerator FadeInRain() {
    _rainAudioSource.Play();
    _rainAudioSource.volume = 0;
    while (_rainAudioSource.volume < _currentVolume) {
      _rainAudioSource.volume += Time.deltaTime;
      yield return null;
    }
  }

  private IEnumerator FadeOutRain() {
    while (_rainAudioSource.volume > 0) {
      _rainAudioSource.volume -= Time.deltaTime;
      yield return null;
    }
    _rainAudioSource.Stop();
  }

}