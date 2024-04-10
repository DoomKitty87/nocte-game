using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InteractHandler : MonoBehaviour
{
  private PlayerInput _input;

  [SerializeField] private RectTransform _interactPromptOverlay;
  [SerializeField] private CanvasGroup _interactCanvasGroup;
  [SerializeField] private float _interactWorldRange;
  [SerializeField][Range(0, 90)] private float _lookingAngleDifference;
  [SerializeField] private LayerMask _interactLayerMask;
    
  private List<GameObject> _interactibles = new List<GameObject>();

  private List<GameObject> ReturnInteractablesInRange(float range) {
    List<GameObject> _objects = new List<GameObject>();
    foreach (Collider collide in Physics.OverlapSphere(transform.position, range, _interactLayerMask)) {
      _objects.Add(collide.gameObject);
    }
    return _objects;
  }

  private (bool, float) LookingAtInteractable(GameObject interactable, float allowedAngleDifference) {
    Vector3 camPos = Camera.main.transform.position;
    Vector3 interPos = interactable.transform.position;
    Vector3 dirToInter = (interPos - camPos).normalized;
    float difference = Vector3.Dot(Camera.main.transform.forward, dirToInter);
    float angle = difference * 180;
    if (angle < allowedAngleDifference) {
      return (true, angle);
    }
    return (false, -1);
  }

  private void TryInteract() {
    _interactibles = ReturnInteractablesInRange(_interactWorldRange);
    if (_interactibles.Count < 1) return;
    float closestAngleIndex = 10000000;
    for (int i = 0; i < _interactibles.Count; i++) {
      GameObject interactible = _interactibles[i];
      (bool, float) output = LookingAtInteractable(interactible, _lookingAngleDifference);
      if (output.Item1 == false) continue;
      if (output.Item2 < closestAngleIndex) {
        closestAngleIndex = i;
      }
    }
  }
  
  
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
}