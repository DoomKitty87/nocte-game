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

}