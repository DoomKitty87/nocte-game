using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class InteractHandler : MonoBehaviour
{
  private PlayerInput _input;

  [Header("Critical Settings")]
  [SerializeField] private RectTransform _interactPromptOverlay;
  [SerializeField] private CanvasGroup _interactCanvasGroup;
  [SerializeField] private float _interactWorldRange;
  [SerializeField][Range(0, 90)] private float _lookingAngleDifference = 5f;
  [SerializeField] private LayerMask _interactLayerMask;

  [Header("Visual Settings")] 
  [SerializeField] private float _timeFadeInOut = 0.2f;
  [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
  
  [SerializeField] private List<GameObject> _interactibles = new List<GameObject>();
  private bool _shown = false;
  
  private List<GameObject> ReturnInteractablesInRange(float range) {
    List<GameObject> _objects = new List<GameObject>();
    foreach (Collider collider in Physics.OverlapSphere(transform.position, range, _interactLayerMask).ToList()) {
      if (collider.gameObject.TryGetComponent<Interactable>(out _)) {
        _objects.Add(collider.gameObject);
      }
    }
    return _objects;
  }

  private (bool, float) LookingAtInteractable(GameObject interactable, float allowedAngleDifference) {
    Vector3 camPos = Camera.main.transform.position;
    Vector3 interPos = interactable.transform.position;
    Vector3 dirToInter = (interPos - camPos).normalized;
    float difference = Vector3.Dot(Camera.main.transform.forward, dirToInter);
    float angle = (1 - difference) * 180;
    if (angle < allowedAngleDifference) {
      return (true, angle);
    }
    return (false, -1);
  }

  private int GetClosestToViewInteractableIndex() {
    int closestAngleIndex = -1;
    float closestAngle = float.PositiveInfinity;
    for (int i = 0; i < _interactibles.Count; i++) {
      GameObject interactible = _interactibles[i];
      (bool, float) output = LookingAtInteractable(interactible, _lookingAngleDifference);
      if (output.Item1 == false) continue;
      if (output.Item2 < closestAngle) {
        closestAngleIndex = i;
        closestAngle = output.Item2;
      }
    }
    return closestAngleIndex;
  }

  private void UpdateOverlayPosition(Vector3 worldPosOfInteractable) {
    _interactPromptOverlay.position = Camera.main.WorldToScreenPoint(worldPosOfInteractable);
  }
  
  private void TryInteract() {
    _interactibles = ReturnInteractablesInRange(_interactWorldRange);
    if (_interactibles.Count < 1) return;
    GameObject selected = _interactibles[GetClosestToViewInteractableIndex()];
    
  }

  private IEnumerator FadeInOverlay() {
    if (_shown) yield break;
    _shown = true;
    float t = 0;
    while (t <= _timeFadeInOut) {
      _interactCanvasGroup.alpha = _fadeCurve.Evaluate(t / _timeFadeInOut);
      t += Time.deltaTime;
      yield return null;
    }
  }
  
  private IEnumerator FadeOutOverlay() {
    if (!_shown) yield break;
    _shown = false;
    float t = 0;
    while (t <= _timeFadeInOut) {
      _interactCanvasGroup.alpha = 1 - _fadeCurve.Evaluate(t / _timeFadeInOut);
      t += Time.deltaTime;
      yield return null;
    }
  }
  
  
  void Start()
  {
    _input = InputReader.Instance.PlayerInput;

    _input.Player.Interact.performed += _ => TryInteract();
    _input.Driving.Leave.performed += _ => TryInteract();

    _interactCanvasGroup.alpha = 0;
  }

  private void Update() {
    _interactibles = ReturnInteractablesInRange(_interactWorldRange);
    if (_interactibles.Count < 1) {
      _interactCanvasGroup.alpha = 0;
      return;
    }
    int closestIndex = GetClosestToViewInteractableIndex();
    if (closestIndex == -1) {
      _interactCanvasGroup.alpha = 0;
      return;
    }
    GameObject selected = _interactibles[closestIndex];
    UpdateOverlayPosition(selected.transform.position);
    _interactCanvasGroup.alpha = 1;
  }

  private void OnDrawGizmos() {
    foreach (var inter in _interactibles) {
      Gizmos.DrawLine(transform.position, inter.transform.position);
    }
  }

  void OnDisable()
  {
    _input.Player.Interact.performed -= _ => TryInteract();
    _input.Driving.Leave.performed -= _ => TryInteract();
  }
}