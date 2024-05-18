using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScanEffect : MonoBehaviour
{
	private PlayerInput _input;

    [Header("Dependencies")]
    [SerializeField] private Volume _scanEffectVolume;
    public Transform _scanOrigin;
    private ScannerEffectPostProcessVolume _effect;
    [Header("Settings")]
    public float _scanDistance = 10f;
    public float _scanDuration = 1.5f;
    [SerializeField] private float _scanHangDuration = 2;
    [SerializeField] private AnimationCurve _scanSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _scanFadeOutCurve;
    [SerializeField] private float _scanFadeOutDuration;
    [SerializeField] private float _scanCooldown = 3f;

    public static event Action OnScan;

    private bool _scanning = false;

    public static ScanEffect Instance { get; private set; }

    public AnimationCurve _inverseSpeedCurve;

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
            float scanDistanceMultiplier = 1;
            if (!UpgradeInfo.GetUpgrade("Scan Range").isLocked) {
                scanDistanceMultiplier = UpgradeInfo.GetUpgrade("Scan Range").value + 1;
            }
            _effect._scanDistance.value = _scanDistance * _scanSpeedCurve.Evaluate(t) * scanDistanceMultiplier;
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

        _inverseSpeedCurve = new AnimationCurve();
        for (int i = 0; i < _scanSpeedCurve.keys.Length; i++) {
            _inverseSpeedCurve.AddKey(new Keyframe(_scanSpeedCurve.keys[i].value, _scanSpeedCurve.keys[i].time));
        }

        Instance = this;
        _input = InputReader.Instance.PlayerInput;

        _input.Player.Scan.performed += _ => TriggerScan();
        _input.Driving.Scan.performed += _ => TriggerScan();

        if (_scanEffectVolume.profile.TryGet<ScannerEffectPostProcessVolume>(out var effect)) {
            _effect = effect;
        } 
        else {
            Debug.LogError("ScannerEffectPostProcessVolume not found in the _scanEffect volume.");
        }
    }

    void OnDisable()
    {
        _input.Player.Scan.performed -= _ => TriggerScan();
        _input.Driving.Scan.performed -= _ => TriggerScan();
    }

    private float _timeSinceLastScan = 0;
    // Update is called once per frame
    private void Update()
    {
        _timeSinceLastScan += Time.deltaTime;
    }

    private void TriggerScan() {
        float scanCooldownMultiplier = 1; 
        if (!UpgradeInfo.GetUpgrade("Scan Cooldown").isLocked) {
            scanCooldownMultiplier = 1f / (UpgradeInfo.GetUpgrade("Scan Cooldown").value + 1);
        }
        if (!_scanning && _timeSinceLastScan > _scanCooldown * scanCooldownMultiplier) {
            _timeSinceLastScan = 0;
            OnScan?.Invoke();
            StartCoroutine(Scan());
        }
    }
}
