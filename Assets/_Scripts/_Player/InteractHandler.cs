using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InteractHandler : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _promptText;
  
  public KeyCode _interactKey;
  
  private List<GameObject> _interactibles = new List<GameObject>();

  private void Update() {
    if (Input.GetKeyDown(_interactKey) && _interactibles.Count > 0) {
      _interactibles[0].GetComponent<Interactible>()._onInteract.Invoke();
      _interactibles.RemoveAt(0);
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Interactible")) {
      _interactibles.Add(other.gameObject);
      _promptText.gameObject.SetActive(true);    
    }
  }

  private void OnTriggerExit(Collider other) {
    if (other.CompareTag("Interactible")) {
      _interactibles.Remove(other.gameObject);
      if (_interactibles.Count > 0) _promptText.gameObject.SetActive(true);
      else _promptText.gameObject.SetActive(false);
    }
  }

}