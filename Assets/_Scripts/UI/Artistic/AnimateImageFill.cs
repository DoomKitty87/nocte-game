using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateImageFill : MonoBehaviour
{
	[SerializeField] private Image _image;
	[SerializeField] private float _fillDuration;
	[SerializeField] private AnimationCurve _fillCurve;

	private void Start() {
		if (_image == null || _image.type != Image.Type.Filled) {
			Debug.LogWarning($"{gameObject.name} AnimateImageFill: Image is null or not of type Filled!");
		}
	}
	public void FillImage(bool resetFillAmount) {
		StartCoroutine(FillImageCoroutine(resetFillAmount));
	}
	private IEnumerator FillImageCoroutine(bool resetFillAmount) {
		float timer = 0;
		float startFillAmount = resetFillAmount ? 0 : _image.fillAmount;
		while (timer < _fillDuration) {
			timer += Time.deltaTime;
			_image.fillAmount = Mathf.Lerp(startFillAmount, 1, _fillCurve.Evaluate(timer / _fillDuration));
			yield return null;
		}
	}
	public void UnfillImage(bool resetFillAmount) {
		StartCoroutine(UnfillImageCoroutine(resetFillAmount));
	}
	private IEnumerator UnfillImageCoroutine(bool resetFillAmount) {
		float timer = 0;
		float startFillAmount = resetFillAmount ? 1 : _image.fillAmount;
		while (timer < _fillDuration) {
			timer += Time.deltaTime;
			_image.fillAmount = Mathf.Lerp(startFillAmount, 0, _fillCurve.Evaluate(timer / _fillDuration));
			yield return null;
		}
	}
}
