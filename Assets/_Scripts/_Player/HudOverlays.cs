using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HudOverlays : MonoBehaviour
{
  private InputHandler _input;

  [System.Serializable]
  public struct Overlay
  {
    public GameObject reference;
    public KeyCode key;
  }

  [SerializeField] private Overlay[] _overlays;

  void Start()
  {
    _input = InputHandler.Instance;
  }

  private void Update() {
    foreach (Overlay overlay in _overlays) {
      if (_input.Overlay) {
        overlay.reference.SetActive(!overlay.reference.activeSelf);
      }
    }
  }

}