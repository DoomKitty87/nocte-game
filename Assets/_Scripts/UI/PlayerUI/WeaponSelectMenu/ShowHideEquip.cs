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
		_inputReader.PlayerInput.AlwaysOn.Enable();
		_inputReader.PlayerInput.AlwaysOn.OpenEquipMenu.performed += _ => FadeIn();
		_inputReader.PlayerInput.AlwaysOn.OpenEquipMenu.canceled += _ => FadeOut();
	}
	
	private void FadeIn() {
		_fadeElementInOut.FadeIn(false);
		_inputReader.PlayerInput.Player.Disable();
		_inputReader.PlayerInput.AlwaysOn.OpenEquipMenu.Enable();
		_inputReader.PlayerInput.UI.Enable();
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = true;
	}
	private void FadeOut() {
		_fadeElementInOut.FadeOut(false);
		_inputReader.PlayerInput.Player.Enable();
		_inputReader.PlayerInput.UI.Disable();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void OnDisable() {
		_inputReader.PlayerInput.AlwaysOn.OpenEquipMenu.performed -= _ => FadeIn();
		_inputReader.PlayerInput.AlwaysOn.OpenEquipMenu.canceled -= _ => FadeOut();
	}
}