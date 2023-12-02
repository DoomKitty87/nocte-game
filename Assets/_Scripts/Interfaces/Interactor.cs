using UnityEngine;

interface IInteractable
{
  public void Interact();
}

public class Interactor : MonoBehaviour
{
  [SerializeField] private float _InteractRange;

  public KeyCode _interactKey = KeyCode.E;
  
  private void Update() {
    if (Input.GetKeyDown(_interactKey)) {
      Ray r = new Ray(transform.position, transform.forward);
      if (Physics.Raycast(r, out RaycastHit hitInfo, _InteractRange)) {
        if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
          interactObj.Interact();
        }
      }
    }
  }
}
