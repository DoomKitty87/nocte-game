using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Interactible : MonoBehaviour
{

  public UnityEvent _onInteract;

  private void Start() {
    gameObject.tag = "Interactible";
  }

  public void DestroySelf() {
    Destroy(gameObject);
  }

  public void FindAudioTape() {
    InventoryManager inventory = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
    inventory.PickupAudioTape("Test", null, null);
  }

}