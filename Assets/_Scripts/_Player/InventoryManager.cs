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
    public string dialogue;
    public Dialogue dialogueScriptable;
    public string timestamp;

  }

  [SerializeField] private AudioSource _audioSource;
  [SerializeField] private Image _tapeSprite;
  [SerializeField] private GameObject _tapePlayerObject;
  
  private List<AudioTape> _audioTapes = new List<AudioTape>();

  public static InventoryManager _instance;

  private void Awake() {
    _instance = this;
  }

  private void OnDisable() {
    _instance = null;
  }

  public AudioTape[] GetOwnedTapes() {
    return _audioTapes.ToArray();
  }

  public void PickupAudioTape(string name, AudioClip clip, Sprite icon, string dialogue, Dialogue dialogueScriptable, string timestamp) {
    AudioTape tape = new AudioTape();
    tape.name = name;
    tape.clip = clip;
    tape.icon = icon;
    tape.dialogue = dialogue;
    tape.dialogueScriptable = dialogueScriptable;
    tape.timestamp = timestamp;
    _audioTapes.Add(tape);
    PlayTape(tape.dialogueScriptable);
  }

  private void EndTape() {
    _tapePlayerObject.SetActive(false);
  }

  private void PlayTape(Dialogue dialogue) {
    DialogueHandler.Instance.PlayDialogue(dialogue, false);
    //_tapePlayerObject.SetActive(true);
    //_tapeSprite.sprite = tape.icon;
    //_audioSource.clip = tape.clip;
    //_audioSource.Play();
    //if (tape.clip != null) Invoke("EndTape", tape.clip.length);
  }
  
}