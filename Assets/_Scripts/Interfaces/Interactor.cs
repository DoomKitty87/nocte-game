using UnityEngine;

interface IInteractable
{
  public void Interact();
}

public class Interactor : MonoBehaviour
{
	private PlayerInput _input;

  [SerializeField] private float _InteractRange;

  private void Start() {
    _input = InputReader.Instance.PlayerInput;

    _input.Player.Interact.performed += _ => Interact();
  }

  private void OnDisable() {
    _input.Player.Interact.performed -= _ => Interact();
  }

  private void Interact() {
    Ray r = new Ray(transform.position, transform.forward);
    if (Physics.Raycast(r, out RaycastHit hitInfo, _InteractRange)) {
      if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
        interactObj.Interact();
      }
    }
  }
}
