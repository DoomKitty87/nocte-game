using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FadeElementInOut))]
public class ShowHideEquip : MonoBehaviour
{
	private InputReader _inputReader;
	private FadeElementInOut _fadeElementInOut;
	
	private void Start() {
		_inputReader = InputReader.Instance;
		_fadeElementInOut = GetComponent<FadeElementInOut>();
		// i don't know why its the function isnt being called even though its literally right fucking here
		// @Elliot help me out here with the stupid new input
		_inputReader.PlayerInput.UI.OpenEquipMenu.started += _ => FadeIn();
		_inputReader.PlayerInput.UI.OpenEquipMenu.performed += _ => FadeOut();
	}
	
	private void FadeIn() {
		print("fadein");
		_fadeElementInOut.FadeIn(false);
	}
	private void FadeOut() {
		_fadeElementInOut.FadeOut(false);
		print("fadeout");
	}

	private void OnDisable() {
		_inputReader.PlayerInput.UI.OpenEquipMenu.started -= _ => FadeIn();
		_inputReader.PlayerInput.UI.OpenEquipMenu.performed -= _ => FadeOut();
	}
}