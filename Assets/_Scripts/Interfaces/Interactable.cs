using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour
{
  public string PromptText = "Interact";
  [SerializeField] private Collider _interactTrigger;
  public UnityEvent InteractedWith;
  
  private void OnValidate() {
    if (_interactTrigger == null) {
      Debug.LogError($"Interactable is lacking a trigger collider! Assign one in the inspector of {gameObject.name}");
    }
    _interactTrigger.isTrigger = false;
  }

  public void Interact() {
    InteractedWith?.Invoke();
  }
}
