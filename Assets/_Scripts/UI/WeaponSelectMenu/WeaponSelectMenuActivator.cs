using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectMenuActivator : MonoBehaviour
{
	[SerializeField] private KeyCode _menuActivationKey;
	[SerializeField] private FadeElementInOut _menuFadeElement;
	[SerializeField] private AnimateImageFill _menuDotsEffect;

	private void Start() {
		CanvasGroup group = _menuFadeElement.gameObject.GetComponent<CanvasGroup>();
		group.blocksRaycasts = false;
		group.interactable = false;
		group.alpha = 0;
	}

	private void Update() {
		if (Input.GetKeyDown(_menuActivationKey)) {
			_menuDotsEffect.FillImage(true);
			_menuFadeElement.FadeIn(false);
			Cursor.lockState = CursorLockMode.None;
		}
		else if (Input.GetKeyUp(_menuActivationKey)) {
			_menuFadeElement.FadeOut(false);
			_menuDotsEffect.gameObject.GetComponent<Image>().fillAmount = 0;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}
}
