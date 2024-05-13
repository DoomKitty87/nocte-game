using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Interactable : MonoBehaviour
{
  public string PromptText = "Interact";
  [SerializeField] private Collider _interactTrigger;
  [FormerlySerializedAs("InteractedWith")] public UnityEvent _InteractedWith;
  [FormerlySerializedAs("OnHoverStart")] public UnityEvent _OnHoverStart;
  [FormerlySerializedAs("OnHoverEnd")] public UnityEvent _OnHoverEnd;
  
  // Note: Errors in OnValidate are catastrophic or something maybe
  private void OnValidate() {
    if (_interactTrigger == null) {
      Debug.LogError($"Interactable is lacking a collider! Assign one in the inspector of {gameObject.name}");
      return;
    }
    _interactTrigger.isTrigger = false;
  }

  public void HoverStart() {
    _OnHoverStart?.Invoke();
    gameObject.layer = LayerMask.NameToLayer("Outlined");
  }
  
  public void HoverEnd() {
    _OnHoverEnd?.Invoke();
    gameObject.layer = LayerMask.NameToLayer("Default");
  }
  
  public void Interact() {
    print("Interacted with interactable!");
    _InteractedWith?.Invoke();
  }
}
