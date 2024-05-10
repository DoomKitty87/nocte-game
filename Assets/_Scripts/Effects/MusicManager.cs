using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.PlayerLoop;

public class MusicManager : MonoBehaviour
{

  [SerializeField] private AudioClip[] _baseMusic;
  [SerializeField] private AudioSource _baseAudioSource;
  [SerializeField] private AudioSource _sectorAudioSource;
  // [SerializeField] private TextMeshProUGUI _songText;
  // [SerializeField] private float _songInfoFadeInTime;
  // [SerializeField] private float _songInfoStayTime;
  // [SerializeField] private float _songInfoFadeOutTime;
  [SerializeField] private float _songCrossfadeDuration;

  // [SerializeField] private Color _inColor = new Color(1, 1, 1, 1);
  // [SerializeField] private Color _outColor = new Color(1, 1, 1, 0);

  private bool _inSector = false;
  private MusicData _currentSector;
  private List<MusicData> _occupiedSectors = new List<MusicData>();

  public static MusicManager Instance { get; private set; }

  private void Awake() {
    Instance = this;
  }

  private void UpdateCurrentSector() {
    StopAllCoroutines();
    // Debug.Log(_inSector);
    if (_inSector) {
      FadeUp(_sectorAudioSource);
      FadeDown(_baseAudioSource);
    } else {
      FadeUp(_baseAudioSource);
      FadeDown(_sectorAudioSource);
    }
  }

  public void PauseMusic() {
    _baseAudioSource.Pause();
    _sectorAudioSource.Pause();
  }

  public void ResumeMusic() {
    _baseAudioSource.Play();
    _sectorAudioSource.Play();
  }

  private void FadeUp(AudioSource audioSource) {
    StartCoroutine(FadeVolume(audioSource, 1, _songCrossfadeDuration));
  }

  private void FadeDown(AudioSource audioSource) {
    StartCoroutine(FadeVolume(audioSource, 0, _songCrossfadeDuration));
  }

  private IEnumerator FadeVolume(AudioSource audioSource, float targetVolume, float duration) {
    if (targetVolume == 1) audioSource.Play();
    float startVolume = audioSource.volume;
    float t = 0;
    while (t < duration) {
      audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
      t += Time.deltaTime / duration;
      yield return null;
    }
    audioSource.volume = targetVolume;
    if (targetVolume == 0) audioSource.Pause();
  }

  private void ChangeSector() {
    StopAllCoroutines();
    StartCoroutine(FadeInOut(_sectorAudioSource, _songCrossfadeDuration, _currentSector._songs[Mathf.CeilToInt(Random.value * _currentSector._songs.Length) - 1]));
  }

  private IEnumerator FadeInOut(AudioSource audioSource, float duration, MusicData.Song newSong) {
    float startVolume = audioSource.volume;
    float t = 0;
    while (t < duration) {
      audioSource.volume = Mathf.Lerp(startVolume, 0, t);
      t += Time.deltaTime / duration;
      yield return null;
    }
    audioSource.volume = 0;
    audioSource.clip = newSong.clip;
    audioSource.Play();
    t = 0;
    while (t < duration) {
      audioSource.volume = Mathf.Lerp(0, 1, t);
      t += Time.deltaTime / duration;
      yield return null;
    }
    audioSource.volume = 1;
  }

  private void Update() {
    if (_inSector) {
      if (!_sectorAudioSource.isPlaying) {
        MusicData.Song newSong = _currentSector._songs[Mathf.CeilToInt(Random.value * _currentSector._songs.Length) - 1];
        _sectorAudioSource.clip = newSong.clip;
        _sectorAudioSource.Play();
      }
    } else {
      if (!_baseAudioSource.isPlaying) {
        _baseAudioSource.clip = _baseMusic[Mathf.CeilToInt(Random.value * _baseMusic.Length) - 1];
        _baseAudioSource.Play();
      }
    }
  }

  // private IEnumerator SongInfo(string name, string artist) {
  //   _songText.text = name + "\n" + artist;
  //   _songText.color = _outColor;
  //   float t = 0;
  //   while (t < _songInfoFadeInTime) {
  //     _songText.color = Color.Lerp(_outColor, _inColor, t);
  //     t += Time.deltaTime / _songInfoFadeInTime;
  //     yield return null;
  //   }
  //   _songText.color = _inColor;
  //   yield return new WaitForSeconds(_songInfoStayTime);
  //   t = 0;
  //   while (t < _songInfoFadeOutTime) {
  //     _songText.color = Color.Lerp(_inColor, _outColor, t);
  //     t += Time.deltaTime / _songInfoFadeInTime;
  //     yield return null;
  //   }
  //   _songText.color = _outColor;
  // }

  public void EnterSector(MusicData sector) {
    _inSector = true;
    _currentSector = sector;
    _occupiedSectors.Add(_currentSector);
    UpdateCurrentSector();
  }

  public void ExitSector(MusicData sector) {
    _occupiedSectors.Remove(sector);
    // Debug.Log(_occupiedSectors.Count);
    if (_occupiedSectors.Count == 0) {
      _inSector = false;
      UpdateCurrentSector();
    } else {
      _currentSector = _occupiedSectors[_occupiedSectors.Count - 1];
      ChangeSector();
    }
  }

}
