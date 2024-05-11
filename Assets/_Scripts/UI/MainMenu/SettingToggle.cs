using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingToggle : MonoBehaviour
{
	[SerializeField] private bool _settingState;
	[SerializeField] private Button _toggleButton;
	[SerializeField] private string _textWhenOn, _textWhenOff;
	[SerializeField] private TextMeshProUGUI _toggleText;

	private void Start() {
		_toggleButton.onClick.AddListener(ToggleSetting);
		UpdateText();
	}
	public void ToggleSetting() {
		print("toggled");
		_settingState = !_settingState;
		UpdateText();
	}
	private void UpdateText() {
		_toggleText.text = _settingState ? _textWhenOn : _textWhenOff;
	}
	private void OnDisable() {
		_toggleButton.onClick.RemoveListener(ToggleSetting);
	}
}
