using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

// For now, only MnK support, console could be whenever
public class RadialMenu : MonoBehaviour
{

	[Header("References")] 
	[Tooltip("Will be the parent of separator and selector GameObjects.")][SerializeField] private GameObject _menuCenter;
	[SerializeField] private GameObject _separatorGameObject;
	[SerializeField] private RectTransform _selectorTransform;
	[Header("Settings")] 
	[SerializeField][Range(1, 16)] private int _selectionCount = 4;
	[SerializeField] private float _separatorOffset = 5f;
	[SerializeField] private SelectionType _selectionType = SelectionType.Step;
	[Serializable]
	private enum SelectionType {
		Instant,
		Continuous,
		Step,
	}
	[SerializeField] private float _selectDeadZone = 5f;
	
	private GameObject[] _separators;
	
	private void ConfigureSeparator(GameObject separator, GameObject center, float distanceOffset) {
		RectTransform rTransform = separator.GetComponent<RectTransform>();
		rTransform.SetParent(center.GetComponent<RectTransform>());
		rTransform.pivot = new Vector2(0.5f, -distanceOffset/rTransform.rect.height);
		rTransform.anchoredPosition = new Vector3(0, 0, 0);
		rTransform.rotation = Quaternion.identity;
	}
	public void RemoveOldSeparators() {
		// Doesn't remove initial separator
		for (int i = 1; i < _separators.Length; i++) {
			DestroyImmediate(_separators[i]);
		}
	}
	public void GenerateSeparators() {
		if (_separators != null) {
			RemoveOldSeparators();
		}
		
		_separators = new GameObject[_selectionCount];
		
		for (int i = 0; i < _selectionCount; i++) {
			GameObject separator;
			if (i == 0) {
				separator = _separatorGameObject;
				ConfigureSeparator(separator, _menuCenter, _separatorOffset);
			}
			else {
				separator = Instantiate(_separatorGameObject, _menuCenter.transform);
				ConfigureSeparator(separator, _menuCenter, _separatorOffset);
			}
			_separators[i] = separator;
			separator.GetComponent<RectTransform>().Rotate(0, 0, 360f / _selectionCount * i);
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
		return uv;
	}
	public void UpdateSelector() {
		Vector2 mouseUV = GetMouseUV();
		Vector2 mousefromCenter = mouseUV - Vector2.zero;
		if (mousefromCenter.magnitude < _selectDeadZone) {
			return;
		}
		switch (_selectionType) {
			case (SelectionType.Instant):
				// TODO: Oh god i have to review unit circle stuff
				throw new NotImplementedException();
		}
	}
	
	private void Start() {
		if (_menuCenter == null || _separatorGameObject == null || _selectorTransform == null) {
			Debug.LogError($"{gameObject.name} RadialMenu: Missing references!");
			return;
		}
		GenerateSeparators();
	}

}