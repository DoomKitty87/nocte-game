using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HudOverlays : MonoBehaviour
{
  private PlayerInput _input;

  [System.Serializable]
  public struct Overlay
  {
    public GameObject reference;
    public KeyCode key;
  }

  [SerializeField] private Overlay[] _overlays;

  void Start()
  {
    _input = InputReader.Instance.PlayerInput;

    _input.Player.Overlay.performed += _input => ActivateOverlay();
  }

  void OnDisable()
  {
    _input.Player.Overlay.performed -= _input => ActivateOverlay();
  }

  private void ActivateOverlay() {
    foreach (Overlay overlay in _overlays) {
        overlay.reference.SetActive(true);
    }
  }

}