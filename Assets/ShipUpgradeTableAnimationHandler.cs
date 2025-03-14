using TMPro;
using UnityEngine;
using UpgradeSystem;

public class ShipUpgradeTableAnimationHandler : MonoBehaviour {
	[SerializeField] private GameObject table;

	[SerializeField] private RectTransform _pannableTransform;

	[SerializeField] private float _parallaxFloat = 0.8f;
	[SerializeField] private float _gain = 0.8f;

  [SerializeField] private TextMeshProUGUI _coreCounter;

	[SerializeField] private UpgradeNode[] _upgradeNodes;

	private Material _mat;

	private Camera _mainCamera;

	private Vector3 _startingPosition;
	private Vector3 _targetPosition;
	private Vector3 _cachedPosition;

	private Vector3 _mainPosition;

	private static readonly int Offset = Shader.PropertyToID("_Offset");
	private static readonly int Center = Shader.PropertyToID("_Center");

	private void OnEnable() {
		_mainCamera = Camera.main;
	}

	private void Start() {
		_mat = table.GetComponent<MeshRenderer>().materials[1];
    _coreCounter.text = PlayerMetaProgression.Instance.AvailableCores.ToString();
	}

	private void Update() {
		Vector3 mousePosition = GetMouseWorldPosition();

		if (Input.GetMouseButtonDown(0)) {
			_startingPosition = mousePosition;
			_targetPosition = Vector3.zero;
		}

		if (Input.GetMouseButtonUp(0)) {
			_cachedPosition = mousePosition;
		}

		if (Input.GetMouseButton(0)) {
			_targetPosition = mousePosition - _startingPosition;
		}
		else {
			_targetPosition = _cachedPosition - _startingPosition;
		}

		LerpTowardsPoint();

		_mat.SetVector(Center, _mainPosition);
		_pannableTransform.anchoredPosition = new Vector2((_mainPosition.z * (1 / .0025f / _parallaxFloat)) * -1, _mainPosition.x * (1 / 0.0025f / _parallaxFloat)) * _gain; // Weird numbers are due to scaling of parent canvas and orientation of table
		// _mat.SetVector(Offset, mousePosition - _mainPosition);
	}

	public void UpdateCoreCounter() {
		_coreCounter.text = PlayerMetaProgression.Instance.AvailableCores.ToString();

		if (PlayerMetaProgression.Instance.AvailableCores == 0) {
			foreach (var upgradeNode in _upgradeNodes) {
				upgradeNode.Lock();
			}
		}
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

	public void ResetPosition() {
		_targetPosition = Vector3.zero;
		_mat.SetVector(Center, Vector3.zero);
		_pannableTransform.anchoredPosition = Vector2.zero;
		_startingPosition = Vector3.zero;
		_mainPosition = Vector3.zero;
		_cachedPosition = Vector3.zero;
		Debug.Log("Reset");
	}
}
