using UnityEngine;

interface IInteractable
{
  public void Interact();
}

public class Interactor : MonoBehaviour
{
	private InputHandler _input;

  [SerializeField] private float _InteractRange;

  private void Start() {
    _input = InputHandler.Instance;
  }

  private void Update() {
    if (_input.PLAYER_Interact) {
      Ray r = new Ray(transform.position, transform.forward);
      if (Physics.Raycast(r, out RaycastHit hitInfo, _InteractRange)) {
        if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
          interactObj.Interact();
        }
      }
    }
  }
}
