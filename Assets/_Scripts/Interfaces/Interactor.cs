using TMPro;
using UnityEngine;

interface IInteractable
{

  public string interactPrompt { get; }
  public void Interact();
}

public class Interactor : MonoBehaviour
{
	private PlayerInput _input;

  [SerializeField] private float _InteractRange;
  [SerializeField] private TextMeshProUGUI _promptText;
  [SerializeField] private LayerMask _interactLayerMask;

  private void Start() {
    _input = InputReader.Instance.PlayerInput;

    _input.Player.Interact.performed += _ => Interact();
  }

  private void Update() {
    Ray r = new Ray(transform.position, transform.forward);
    if (Physics.Raycast(r, out RaycastHit hitInfo, _InteractRange, _interactLayerMask)) {
      if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
        _promptText.text = interactObj.interactPrompt;
      } else {
        _promptText.text = "";
      }
    } else {
      _promptText.text = "";
    }
  }

  private void OnDisable() {
    _input.Player.Interact.performed -= _ => Interact();
  }

  private void Interact() {
    Ray r = new Ray(transform.position, transform.forward);
    if (Physics.Raycast(r, out RaycastHit hitInfo, _InteractRange, _interactLayerMask)) {
      if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
        interactObj.Interact();
      }
    }
  }
}
