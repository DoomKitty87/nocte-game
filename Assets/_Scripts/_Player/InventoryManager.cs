using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{

  public struct AudioTape
  {

    public string name;
    public AudioClip clip;
    public Sprite icon;

  }

  [SerializeField] private AudioSource _audioSource;
  [SerializeField] private Image _tapeSprite;
  [SerializeField] private GameObject _tapePlayerObject;
  
  private List<AudioTape> _audioTapes = new List<AudioTape>();

  public void PickupAudioTape(string name, AudioClip clip, Sprite icon) {
    AudioTape tape = new AudioTape();
    tape.name = name;
    tape.clip = clip;
    tape.icon = icon;
    _audioTapes.Add(tape);
    PlayTape(tape);
  }

  private void EndTape() {
    _tapePlayerObject.SetActive(false);
  }

  private void PlayTape(AudioTape tape) {
    _tapePlayerObject.SetActive(true);
    _tapeSprite.sprite = tape.icon;
    _audioSource.clip = tape.clip;
    _audioSource.Play();
    Invoke(EndTape, tape.clip.length);
  }
  
}