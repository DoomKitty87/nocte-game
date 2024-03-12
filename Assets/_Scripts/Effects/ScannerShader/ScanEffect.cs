using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScanEffect : MonoBehaviour
{

    [Header("Dependencies")]
    [SerializeField] private Volume _scanEffectVolume;
    [SerializeField] private Transform _scanOrigin;
    private ScannerEffectPostProcessVolume _effect;
    [Header("Settings")]
    [SerializeField] private KeyCode _scanKey = KeyCode.Q;
    [SerializeField] private float _scanDistance = 10f;
    [SerializeField] private float _scanDuration = 1.5f;
    [SerializeField] private float _scanHangDuration = 2;
    [SerializeField] private AnimationCurve _scanSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _scanFadeOutCurve;
    [SerializeField] private float _scanFadeOutDuration;
    [SerializeField] private float _scanCooldown = 3f;

    private bool _scanning = false;
    private IEnumerator Scan() {
        _scanning = true;
        _effect._intensity.value = 1;
        _effect._scannerCenterPosition.value = _scanOrigin.position + -_scanOrigin.forward * 3;
        _effect._scanDirectionXZ.value = new Vector2(_scanOrigin.forward.x, _scanOrigin.forward.z);
        _effect._scanDistance.value = 0;
        yield return new WaitForSeconds(0.5f);
        float durationLeft = _scanDuration;
        while (durationLeft > 0) {
            float t = 1 - durationLeft / _scanDuration;
            _effect._scanDistance.value = _scanDistance * _scanSpeedCurve.Evaluate(t);
            durationLeft -= Time.deltaTime;
            yield return null;
        }
        durationLeft = _scanHangDuration;
        while (durationLeft > 0) {
            durationLeft -= Time.deltaTime;
            yield return null;
        }
        durationLeft = _scanFadeOutDuration;
        while (durationLeft > 0) {
            float t =  1 - durationLeft / _scanFadeOutDuration;
            _effect._intensity.value = _scanFadeOutCurve.Evaluate(t);
            durationLeft -= Time.deltaTime;
            yield return null;
        }
        _effect._intensity.value = 0;
        _scanning = false;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        if (_scanEffectVolume.profile.TryGet<ScannerEffectPostProcessVolume>(out var effect)) {
            _effect = effect;
        } 
        else {
            Debug.LogError("ScannerEffectPostProcessVolume not found in the _scanEffect volume.");
        }
    }

    private float _timeSinceLastScan = 0;
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(_scanKey) && !_scanning && _timeSinceLastScan > _scanCooldown) {
            _timeSinceLastScan = 0;
            StartCoroutine(Scan());
        }
        _timeSinceLastScan += Time.deltaTime;
    }

    private void OnDestroy() {
        
    }
}
