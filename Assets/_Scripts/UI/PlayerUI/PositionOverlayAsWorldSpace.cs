using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionOverlayAsWorldSpace : MonoBehaviour
{
	[Header("Overlay must be direct child of Canvas")]
	[SerializeField] private Transform _target;
	[SerializeField] private RectTransform _overlay;
	[SerializeField] private Vector3 _offset;
	[SerializeField] private Vector3 _damping;

  [SerializeField] private RectTransform _overlayConnection;
  [SerializeField] private Vector3 _connectionOffset;
  
	private void Start() {
		_overlay.position = Camera.main.WorldToScreenPoint(_target.position) + _offset;
    _overlayConnection.position = Camera.main.WorldToScreenPoint(_target.position) + _connectionOffset;
	}

  public void ResetPosition() {
    _overlay.position = Camera.main.WorldToScreenPoint(_target.position) + _offset;
    _overlayConnection.position = Camera.main.WorldToScreenPoint(_target.position) + _connectionOffset;
  }

	private void Update() {
		Vector3 targetPosition = Camera.main.WorldToScreenPoint(_target.position) + _offset;
    Vector3 connectionPosition = Camera.main.WorldToScreenPoint(_target.position) + _connectionOffset;
    Vector3 newPosition = Vector3.Lerp(_overlay.position, targetPosition, _damping.x * Time.deltaTime);
    Vector3 newConnectionPosition = Vector3.Lerp(_overlayConnection.position, connectionPosition, _damping.x * Time.deltaTime);
    _overlay.position = newPosition;
    _overlayConnection.position = newConnectionPosition;
	}
}
