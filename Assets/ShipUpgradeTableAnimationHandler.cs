using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class ShipUpgradeTableAnimationHandler : MonoBehaviour {
	[SerializeField] private GameObject table;

	private Material _mat;

	private Camera _mainCamera;

	private Vector3 _startingPosition;
	private Vector3 _targetPosition;
	private Vector3 _cachedPosition;

	private Vector3 _mainPosition;

	private static readonly int Scale = Shader.PropertyToID("_Scale");
	private static readonly int Offset = Shader.PropertyToID("_Offset");
	private static readonly int Center = Shader.PropertyToID("_Center");

	private void OnEnable() {
		_mainCamera = Camera.main;
	}

	private void Start() {
		_mat = table.GetComponent<MeshRenderer>().materials[1];
	}

	private void Update() {
		Vector3 mousePosition = GetMouseWorldPosition();

		if (Input.GetMouseButtonDown(0)) {
			_startingPosition = mousePosition;
			_targetPosition = Vector3.zero;
		}

		if (Input.GetMouseButton(0)) {
			_targetPosition = mousePosition - _startingPosition;
		}
		else {
			_targetPosition = _cachedPosition - _startingPosition;
		}

		if (Input.GetMouseButtonUp(0)) {
			_cachedPosition = mousePosition;
		}

		if (Input.mouseScrollDelta.y != 0) {
			_mat.SetFloat(Scale, Mathf.Clamp(_mat.GetFloat(Scale) + -Input.mouseScrollDelta.y * 0.1f, 0.1f, 3f));
		}

		LerpTowardsPoint();

		_mat.SetVector(Center, mousePosition);
		_mat.SetVector(Offset, mousePosition - _mainPosition);
	}

	private void LerpTowardsPoint() {
		Vector3 center = _mat.GetVector(Center) - _mat.GetVector(Offset);
		Vector3 newPosition = Vector3.Lerp(center, center + _targetPosition, 4f * Time.deltaTime);
		_mainPosition = newPosition;
		_startingPosition += (newPosition - center);
	}

	private Vector3 GetMouseWorldPosition() {
		Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hit)) {
			return hit.point;
		}
		return Vector3.zero;
	}
}
