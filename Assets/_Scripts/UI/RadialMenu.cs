using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

// For now, only MnK support, console could be whenever
public class RadialMenu : MonoBehaviour
{
	public InputReader _inputReader;

	public UnityEvent<int> _OnSelected = new UnityEvent<int>();
	private int _currentIndexHovered;
	
	[Header("References")] 
	[FormerlySerializedAs("_menuCenter")]
	[Tooltip("Will also be the parent of separator and selector GameObjects. MUST BE IN SCREEN CENTER")]
	[SerializeField] private GameObject _menuContainer;
	[Tooltip("Image Object used to denote selection differences")]
	[SerializeField] private GameObject _separatorGameObject;
	[SerializeField] private RectTransform _selectorTransform;
	public enum SeperatorOutwardsDirection {
		Up,
		Down,
		Left,
		Right,
	}
	[Header("Required Settings")] 
	[SerializeField][Tooltip("Used so that all separators start pointing towards the right direction")] private SeperatorOutwardsDirection _seperatorOutwardsDirection = SeperatorOutwardsDirection.Up;
	[SerializeField][Range(1, 16)] private int _selectionCount = 4;
	[Header("Settings")]
	[SerializeField] private float _separatorOffsetFromCenter = 150f;
	[SerializeField] private float _imageOffsetFromCenter;
	[SerializeField] private float _imageSize;
	[SerializeField] private SelectionType _selectionType = SelectionType.Step;
	public enum SelectionType {
		Instant,
		Continuous,
		Step,
	}
	[SerializeField][Tooltip("Unit is distance from center of menu to closest screen edge.")][Range(0, 1)] private float _selectionDeadZone = 0.2f;
	[SerializeField][Tooltip("Used for fixing visual issues with the source selector image's rotation.")] private float _selectorRotationOffsetDeg = 45;
	[Header("For Step Only")]
	[SerializeField] private float _stepEaseSpeed = 10f;

	
	private GameObject[] _separators;
	private GameObject[] _weaponImages;
	
	private void ConfigureSeparator(GameObject separator, GameObject center, float distanceOffset) {
		RectTransform rTransform = separator.GetComponent<RectTransform>();
		rTransform.SetParent(center.GetComponent<RectTransform>());
		rTransform.pivot = new Vector2(0.5f, -distanceOffset/rTransform.rect.height);
		switch (_seperatorOutwardsDirection) {
			case SeperatorOutwardsDirection.Up:
				rTransform.pivot = new Vector2(0.5f, -distanceOffset/rTransform.rect.height);
				break;
			case SeperatorOutwardsDirection.Down:
				rTransform.pivot = new Vector2(0.5f, distanceOffset/rTransform.rect.height);
				break;
			case SeperatorOutwardsDirection.Left:
				rTransform.pivot = new Vector2(distanceOffset/rTransform.rect.width, 0.5f);
				break;
			case SeperatorOutwardsDirection.Right:
				rTransform.pivot = new Vector2(-distanceOffset/rTransform.rect.width, 0.5f);
				break;
		}
		rTransform.anchoredPosition = new Vector3(0, 0, 0);
		rTransform.rotation = Quaternion.identity;
	}
	public void RemoveOldSlots() {
		// Doesn't remove initial separator
		DestroyImmediate(_weaponImages[0]);
		for (int i = 1; i < _separators.Length; i++) {
			DestroyImmediate(_separators[i]);
			DestroyImmediate(_weaponImages[i]);
		}
	}

	private void SetSeparatorRotation(int index, GameObject separator) {
		float stepDegrees = 360f / _selectionCount;
		float rotation = 0;
		switch (_seperatorOutwardsDirection) {
			case SeperatorOutwardsDirection.Up:
				rotation = stepDegrees * index - 90;
				break;
			case SeperatorOutwardsDirection.Down:
				rotation = stepDegrees * index + 90;
				break;
			case SeperatorOutwardsDirection.Left:
				rotation = stepDegrees * index - 180;
				break;
			case SeperatorOutwardsDirection.Right:
				rotation = stepDegrees * index;
				break;
		}
		separator.GetComponent<RectTransform>().Rotate(0, 0, rotation);
	}

	private void ConfigureImage(GameObject image, GameObject center) {
		image.transform.SetParent(center.transform);
		image.transform.localPosition = Vector3.zero;
		image.name = "Icon";
		image.AddComponent<RectTransform>();
		image.GetComponent<RectTransform>().sizeDelta = new Vector2(_imageSize, _imageSize);
		image.AddComponent<Image>();
	}
	private void SetImagePosition(int index, GameObject imageContainer) {
		float stepDegrees = 360f / _selectionCount;
		float stepRadians = stepDegrees * Mathf.Deg2Rad;
		Vector2 offsetFromCenter = new Vector2(Mathf.Cos(stepRadians * index + stepRadians / 2), Mathf.Sin(stepRadians * index + stepRadians / 2));
		offsetFromCenter *= _imageOffsetFromCenter;
		imageContainer.GetComponent<RectTransform>().anchoredPosition = offsetFromCenter;
	}
	
	public void GenerateSeparators() {
		if (_separators != null) {
			RemoveOldSlots();
		}
		
		_separators = new GameObject[_selectionCount];
		_weaponImages = new GameObject[_selectionCount];
		
		for (int i = 0; i < _selectionCount; i++) {
			GameObject separator;
			GameObject imageContainer = new GameObject();
			ConfigureImage(imageContainer, _menuContainer);
			if (i == 0) {
				separator = _separatorGameObject;
				ConfigureSeparator(separator, _menuContainer, _separatorOffsetFromCenter);
			}
			else {
				separator = Instantiate(_separatorGameObject, _menuContainer.transform);
				ConfigureSeparator(separator, _menuContainer, _separatorOffsetFromCenter);
			}
			SetSeparatorRotation(i, separator);
			_separators[i] = separator;
			SetImagePosition(i, imageContainer);
			_weaponImages[i] = imageContainer;
		}
	}
	
	private void ConfigureSelector(GameObject selector, GameObject center) {
		RectTransform selectorTransform = selector.GetComponent<RectTransform>();
		selectorTransform.SetParent(center.GetComponent<RectTransform>());
		selectorTransform.pivot = new Vector2(0, 0);
		selectorTransform.anchoredPosition = new Vector3(0, 0, 0);
		selectorTransform.rotation = Quaternion.identity;
	}
	
	private Vector2 GetMouseUV() {
		Vector2 mousePosition = Input.mousePosition;
		Vector2 uv = mousePosition / new Vector2(Screen.width, Screen.height);
		uv -= new Vector2(0.5f, 0.5f);
		uv *= 2;
		uv = new Vector2(uv.x * Screen.width / Screen.height, uv.y);
		return uv;
	}
	
	
	
	// Lower bound, Upper Bound
	private (float, float) GetCurrentStepBounds(float currentDegrees) {
		float parentRotation = _menuContainer.GetComponent<RectTransform>().rotation.eulerAngles.z;
		float stepDegrees = 360f / _selectionCount;
		float stepsPassed = (currentDegrees - parentRotation) / stepDegrees;
		(float, float) nearestStepBounds = (Mathf.Floor(stepsPassed) * stepDegrees + parentRotation, Mathf.Ceil(stepsPassed) * stepDegrees + parentRotation);
		return nearestStepBounds;
	}
	
	private int GetCurrentStepIndex(float currentDegrees) {
		float parentRotation = _menuContainer.GetComponent<RectTransform>().rotation.eulerAngles.z;
		float stepDegrees = 360f / _selectionCount;
		float stepsPassed = (currentDegrees - parentRotation) / stepDegrees;
		return Mathf.FloorToInt(stepsPassed);
	}
	
	public void UpdateSelector() {
		Vector2 mouseUV = GetMouseUV();
		Vector2 mousefromCenter = mouseUV.normalized;
		if (mouseUV.magnitude < _selectionDeadZone) {
			return;
		}
		float rotationDegrees = Mathf.Atan2(mousefromCenter.y, mousefromCenter.x) * Mathf.Rad2Deg;
		(float lowerBound, float upperBound) = GetCurrentStepBounds(rotationDegrees);
		_currentIndexHovered = GetCurrentStepIndex(rotationDegrees);
		switch (_selectionType) {
			case SelectionType.Instant:
				_selectorTransform.rotation = Quaternion.Euler(0, 0, (lowerBound + upperBound) / 2 + _selectorRotationOffsetDeg);
				break;
			case SelectionType.Continuous:
				_selectorTransform.rotation = Quaternion.Euler(0, 0, rotationDegrees);
				break;
			case SelectionType.Step:
				_selectorTransform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(_selectorTransform.rotation.eulerAngles.z, (lowerBound + upperBound) / 2 + _selectorRotationOffsetDeg, Time.deltaTime * _stepEaseSpeed));
				break;
		}
		// print($"{mousefromCenter.ToString()} | {rotationDegrees} | {(lowerBound + upperBound) / 2}");
	}

	private void OnSelect() {
		_OnSelected.Invoke(_currentIndexHovered);
	}
	
	private void Start() {
		if (_menuContainer == null || _separatorGameObject == null || _selectorTransform == null) {
			Debug.LogError($"{gameObject.name} RadialMenu: Missing references!");
			return;
		}
		GenerateSeparators();
		ConfigureSelector(_selectorTransform.gameObject, _menuContainer);
		_inputReader = InputReader.Instance;
		_inputReader.PlayerInput.UI.Click.performed += _ => OnSelect();
	}
	
	private void Update() {
		UpdateSelector();
	}

	private void OnDisable() {
		_inputReader.PlayerInput.UI.Click.performed -= _ => OnSelect();
	}
}