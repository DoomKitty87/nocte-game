using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTape : MonoBehaviour
{
  
  [SerializeField] private string name;
  [SerializeField] private AudioClip audio;
  [SerializeField] private Sprite icon;
  [SerializeField] private string dialogue;

  [SerializeField] private GameObject deleteParent;

  public void Pickup() {
    if (audio == null) {
      InventoryManager._instance.PickupAudioTape(name, null, icon, dialogue);
    } else {
      InventoryManager._instance.PickupAudioTape(name, audio, icon, dialogue);
    }
    Destroy(deleteParent);
  }
  
}
