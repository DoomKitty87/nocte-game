using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour
{
  public string PromptText = "Interact";
  [SerializeField] private Collider _interactTrigger;
  public UnityEvent InteractedWith;
  
  // Note: Errors in OnValidate are catastrophic
  private void OnValidate() {
    if (_interactTrigger == null) {
      Debug.LogError($"Interactable is lacking a trigger collider! Assign one in the inspector of {gameObject.name}");
      return;
    }
    _interactTrigger.isTrigger = false;
  }

  public void Interact() {
    InteractedWith?.Invoke();
  }
}
