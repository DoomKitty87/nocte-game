using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{

  public struct AudioTape
  {

    public string name;
    public AudioClip clip;
    public Sprite icon;

  }

  [SerializeField] private AudioSource _audioSource;
  
  private List<AudioTape> _audioTapes = new List<AudioTape>();

  public void PickupAudioTape(string name, AudioClip clip, Sprite icon) {
    AudioTape tape = new AudioTape();
    tape.name = name;
    tape.clip = clip;
    tape.icon = icon;
    _audioTapes.Add(tape);
    PlayTape(tape);
  }

  private void PlayTape(AudioTape tape) {
    _audioSource.clip = tape.clip;
    _audioSource.Play();
  }
  
}