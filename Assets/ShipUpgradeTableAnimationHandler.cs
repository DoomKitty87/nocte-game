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
		if (Input.GetMouseButtonDown(0)) {
			_startingPosition = GetMouseWorldPosition();
		}

		if (Input.GetMouseButton(0)) {
			_targetPosition = GetMouseWorldPosition() - _startingPosition;
		}
		else {
			_targetPosition = _cachedPosition - _startingPosition;
		}

		if (Input.GetMouseButtonUp(0)) {
			_cachedPosition = GetMouseWorldPosition();
		}

		LerpTowardsPoint();
	}

	private void LerpTowardsPoint() {
		Vector3 center = _mat.GetVector(Center);
		Vector3 newPosition = Vector3.Lerp(center, center + _targetPosition, 4f * Time.deltaTime);
		_mat.SetVector(Center, newPosition);
		_startingPosition += (newPosition - center);
	}

	private Vector3 GetMouseWorldPosition() {
		Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hit)) {
			return hit.point;
		}
		Debug.Log("No hit");
		return Vector3.zero;
	}
}
