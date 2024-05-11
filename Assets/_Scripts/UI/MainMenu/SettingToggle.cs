using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingToggle : MonoBehaviour
{
	[SerializeField] private Toggle _toggle;
	[SerializeField] private string _textWhenOn, _textWhenOff;
	[SerializeField] private TextMeshProUGUI _toggleText;

	private void Start() {
		_toggle.onValueChanged.AddListener(ToggleSetting);
		UpdateText(_toggle.isOn);
	}
	public void ToggleSetting(bool value) {
		UpdateText(value);
	}
	private void UpdateText(bool isToggleOn) {
		_toggleText.text =  isToggleOn ? _textWhenOn : _textWhenOff;
	}
	private void OnDisable() {
		_toggle.onValueChanged.RemoveListener(ToggleSetting);
	}
}
