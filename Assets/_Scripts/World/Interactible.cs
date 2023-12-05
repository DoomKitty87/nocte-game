using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Interactible : MonoBehaviour
{

  public UnityEvent _onInteract;

  // For audio tapes
  [SerializeField] private string _audioTapeName;
  [SerializeField] private AudioClip _audioTapeClip;
  [SerializeField] private Sprite _audioTapeIcon;

  private void Start() {
    gameObject.tag = "Interactible";
    gameObject.name = _audioTapeName;
  }

  public void DestroySelf() {
    Destroy(gameObject);
  }

  public void FindAudioTape() {
    InventoryManager inventory = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
    inventory.PickupAudioTape(_audioTapeName, _audioTapeClip, _audioTapeIcon);
  }

}