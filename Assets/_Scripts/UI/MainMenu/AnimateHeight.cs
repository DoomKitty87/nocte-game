using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateHeight : MonoBehaviour
{
    // this is stupid and im just gonna use animations
    
    [SerializeField] private RectTransform _transformToAnimate;
    [SerializeField] private float _startHeight;
    [SerializeField] private float _endHeight;
    [SerializeField] private float _length;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] [Tooltip("Normal: Evaluates curve 0-1 as progress through animation, regardless of animation direction. Reverse: Evaluates curve as 0-1 or 1-0 depending on animation direction. ")]
    private CurveApplyMode _curveApplyMode;
    public enum CurveApplyMode
    {
        Normal, 
        Reverse, 
    }
    private float _timer;


    private void Start() {
        _transformToAnimate.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _startHeight);
    }

    private float EvaluateCurve(float progress, bool IsAnimatingForward) {
        if (_curveApplyMode == CurveApplyMode.Normal) {
            return _curve.Evaluate(progress);
        }
        return IsAnimatingForward ? _curve.Evaluate(progress) : _curve.Evaluate(1 - progress);
    }
    
    private IEnumerator AnimateHeightCoroutine(bool TargetIsEndHeight) {
        float startHeight = _transformToAnimate.rect.height;
        float endHeight = TargetIsEndHeight ? _endHeight : _startHeight;
        float timer = 0;
        while (timer < _length) {
            timer += Time.deltaTime;
            float progress = timer / _length;
            float curveValue = EvaluateCurve(progress, TargetIsEndHeight);
            _transformToAnimate.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(startHeight, endHeight, curveValue));
            yield return null;
        }
        _transformToAnimate.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, endHeight);
    }
}
