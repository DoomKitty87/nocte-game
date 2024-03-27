using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InteractHandler : MonoBehaviour
{
  private PlayerInput _input;

  [SerializeField] private TextMeshProUGUI _promptText;
  
  public KeyCode _interactKey;
  
  private List<GameObject> _interactibles = new List<GameObject>();

  void Start()
  {
    _input = InputReader.Instance.PlayerInput;

    _input.Player.Interact.performed += _ => TryInteract();
    _input.Driving.Leave.performed += _ => TryInteract();
  }

  void OnDisable()
  {
    _input.Player.Interact.performed -= _ => TryInteract();
    _input.Driving.Leave.performed -= _ => TryInteract();
  }

  private void TryInteract() {
    if (_interactibles.Count > 0) {
      _interactibles[0].GetComponent<Interactible>()._onInteract.Invoke();
      _interactibles.RemoveAt(0);
      if (_promptText == null) return;
      if (_interactibles.Count > 0) _promptText.text = "Press " + _interactKey.ToString() + " to interact with " + _interactibles[0].name;
      else _promptText.text = "";
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Interactible")) {
      _interactibles.Add(other.gameObject);
      if (_promptText != null) _promptText.text = "Press " + _interactKey.ToString() + " to interact with " + other.gameObject.name;
    }

    if (other.CompareTag("Core")) {
      Destroy(other.gameObject);
      PlayerMetaProgression.Instance.AddCore();
    }
  }

  private void OnTriggerExit(Collider other) {
    if (other.CompareTag("Interactible")) {
      _interactibles.Remove(other.gameObject);
      if (_promptText == null) return;
      if (_interactibles.Count > 0) _promptText.text = "Press " + _interactKey.ToString() + " to interact with " + _interactibles[0].name;
      else _promptText.text = "";
    }
  }

}