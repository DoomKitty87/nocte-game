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
	private void Start() {
		_overlay.position = Camera.main.WorldToScreenPoint(_target.position) + _offset;	
	}

	private void Update() {
		_overlay.position = Camera.main.WorldToScreenPoint(_target.position) + _offset;
	}
}
