using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadeImageAlpha : MonoBehaviour
{
  [Header("References")]
  public Image _image;
  [Header("Settings")]
  [SerializeField] private AnimationCurve _easingCurve;
  [SerializeField] private float _inDuration;
  [SerializeField] private float _outDuration;
  [Header("Events")]
  public UnityEvent _OnFadeComplete;

  void OnValidate() {
    if (_image == null) _image = GetComponent<Image>();
  }

  public void FadeIn(bool resetAlpha) {
    if (resetAlpha) {
      StartCoroutine(FadeElementInOutCoroutine(0, 1, _inDuration));
    } else {
      StartCoroutine(FadeElementInOutCoroutine(_image.color.a, 1, _inDuration));
    }
  }
  public void FadeOut(bool resetAlpha) {
    if (resetAlpha) {
      StartCoroutine(FadeElementInOutCoroutine(1, 0, _outDuration));
    } else {
      StartCoroutine(FadeElementInOutCoroutine(_image.color.a, 0, _outDuration));
    }
  }
  private IEnumerator FadeElementInOutCoroutine(float startAlpha, float targetAlpha, float duration) {
    float time = 0;
    while (time < duration) {
      time += Time.unscaledDeltaTime;
      Color c = _image.color;
      _image.color = new Color(c.r, c.g, c.b, Mathf.Lerp(startAlpha, targetAlpha, _easingCurve.Evaluate(time / duration)));
      yield return null;
    }

    _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, targetAlpha);
    _OnFadeComplete?.Invoke();
  }
}
