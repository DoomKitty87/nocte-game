using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTape : MonoBehaviour
{
  
  [SerializeField] private string name;
  [SerializeField] private AudioClip audio;
  [SerializeField] private Sprite icon;
  [SerializeField] private string dialogue;
  [SerializeField] private Dialogue dialogueScriptable;

  [SerializeField] private GameObject deleteParent;

  public void Pickup() {
    InventoryManager._instance.PickupAudioTape(name, audio, icon, dialogue, dialogueScriptable);
    Destroy(deleteParent);
  }
  
}
