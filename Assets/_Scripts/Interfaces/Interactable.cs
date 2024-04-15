using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour
{
  public string PromptText = "Interact";
  [SerializeField] private Collider _interactTrigger;
  public UnityEvent InteractedWith;
  public UnityEvent OnHoverStart;
  public UnityEvent OnHoverEnd;
  
  // Note: Errors in OnValidate are catastrophic or something maybe
  private void OnValidate() {
    if (_interactTrigger == null) {
      Debug.LogError($"Interactable is lacking a collider! Assign one in the inspector of {gameObject.name}");
      return;
    }
    _interactTrigger.isTrigger = false;
  }

  public void HoverStart() {
    OnHoverStart?.Invoke();
    gameObject.layer = LayerMask.NameToLayer("Outlined");
  }
  
  public void HoverEnd() {
    OnHoverEnd?.Invoke();
    gameObject.layer = LayerMask.NameToLayer("Default");
  }
  
  public void Interact() {
    InteractedWith?.Invoke();
  }
}
