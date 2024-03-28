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

  private AudioSource _audioSource;

  public static WeatherSounds Instance { get; private set; }

  private void Awake() {
    Instance = this;
  }

  private void Start() {
    _audioSource = GetComponent<AudioSource>();
  }

  public void UpdateWeather(float rainLevel) {
    for (int i = 0; i < _rainWeatherLevels.Length; i++) {
      if (rainLevel >= _rainWeatherLevels[i].threshold) {
        _audioSource.clip = _rainWeatherLevels[i].clip;
        _audioSource.volume = _rainWeatherLevels[i].volume;
        if (!_audioSource.isPlaying) StartCoroutine(FadeInAudio());
        return;
      }
    }
    StartCoroutine(FadeOutAudio());
  }

  private IEnumerator FadeInAudio() {
    _audioSource.Play();
    _audioSource.volume = 0;
    while (_audioSource.volume < _audioSource.volume) {
      _audioSource.volume += Time.deltaTime;
      yield return null;
    }
  }

  private IEnumerator FadeOutAudio() {
    while (_audioSource.volume > 0) {
      _audioSource.volume -= Time.deltaTime;
      yield return null;
    }
    _audioSource.Stop();
  }

}