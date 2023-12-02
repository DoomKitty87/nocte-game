using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HudOverlays : MonoBehaviour
{

  [System.Serializable]
  public struct Overlay
  {
    public GameObject reference;
    public KeyCode key;
  }

  [SerializeField] private Overlay[] _overlays;

  private void Update() {
    foreach (Overlay overlay in _overlays) {
      if (Input.GetKeyDown(overlay.key)) {
        overlay.reference.SetActive(!overlay.reference.activeSelf);
      }
    }
  }

}