using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class MusicManager : MonoBehaviour
{

  [SerializeField] private AudioClip[] _baseMusic;
  [SerializeField] private AudioSource _audioSource;
  [SerializeField] private TextMeshProUGUI _songText;
  [SerializeField] private float _songInfoFadeInTime;
  [SerializeField] private float _songCrossfadeDuration;

  private bool _inSector = false;
  private MusicData _currentSector;
  private List<MusicData> _occupiedSectors = new List<MusicData>();

  private void UpdateCurrentSector() {
    MusicData.Song newSong = _currentSector._songs[Mathf.CeilToInt(Random.value * _currentSector._songs.Length) - 1];
    _audioSource.clip = newSong.clip;
    _audioSource.Play();
    StopCoroutine("SongInfo");
    StartCoroutine(SongInfo(newSong.name, newSong.artist));
  }

  public void PauseMusic() {
    _audioSource.Pause();
  }

  public void ResumeMusic() {
    _audioSource.Resume();
  }

  private IEnumerator SongInfo(string name, string artist) {
    _songText.text = name + "\n" + artist;
    _songText.vertexColor.alpha = 0;
    float t = 0;
    while (t < 1) {
      _songText.vertexColor.alpha = Mathf.SmoothStep(0, 1, t);
      t += Time.deltaTime / _songInfoFadeInTime;
      yield return null;
    }
    _songText.vertexColor.alpha = 1;
  }

  private void OnTriggerEnter(Collider other) {
    if (!other.gameObject.CompareTag("SoundTrigger")) return;
    _inSector = true;
    _currentSector = other.gameObject.GetComponent<MusicData>();
    _occupiedSectors.Add(_currentSector);
    UpdateCurrentSector();
  }

  private void OnTriggerExit(Collider other) {
    if (!other.gameObject.CompareTag("SoundTrigger")) return;
    _occupiedSectors.Remove(other.gameObject.GetComponent<MusicData>());
    if (_occupiedSectors.Count == 0) {
      _inSector = false;
    } else {
      _currentSector = _occupiedSectors[_occupiedSectors.Count - 1];
    }
    UpdateCurrentSector();
  }
}