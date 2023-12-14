using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// For now, only MnK support, console could be whenever
public class RadialMenu : MonoBehaviour
{

	[Header("References")] 
	[SerializeField] private GameObject _menuCenter;
	[SerializeField] private GameObject _separatorGameObject;
	[SerializeField] private RectTransform _selectorTransform;
	[Header("Settings")] 
	[SerializeField] private int _selectionCount = 4;
	[SerializeField] private float _separatorOffset = 5f;

	private void ConfigureSeparator(GameObject separator, GameObject center, float distanceOffset) {
		RectTransform rTransform = separator.GetComponent<RectTransform>();
		
		rTransform.pivot = new Vector2(0.5f, distanceOffset/rTransform.rect.height);
		rTransform.rotation = Quaternion.identity;
		rTransform.position = new Vector3(0, -distanceOffset, 0);
	}
	
	private void GenerateSeparators() {
		for (int i = 0; i < _selectionCount; i++) {
			if (i == 0) {
				ConfigureSeparator(_separatorGameObject, _menuCenter, _separatorOffset);
			}
		}
	}
	
	
	private void Start() {
		
	}

}