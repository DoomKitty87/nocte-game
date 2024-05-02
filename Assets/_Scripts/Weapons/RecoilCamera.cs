using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class RecoilProfile
{
    public float _verticalRecoilDegPerAttack;
    public float _horizontalRecoilDegPerAttack;
    public float _rotationEffectDegPerAttack;
    public float _recoilRecoveryDegPerSecond;
    public float _recoilSnappiness;
}

public class RecoilCamera : MonoBehaviour
{
    [SerializeField] private Transform _recoilTransform;
    [SerializeField] private RecoilProfile _currentProfile;
    [SerializeField] private Vector3 _underlyingRotation;
    [SerializeField] private Vector3 _currentRotation;
    [SerializeField] private Vector3 _targetRotation;

    private void Start() {
        _underlyingRotation = Vector3.zero;
        _targetRotation = Vector3.zero;
    }

    // Update is called once per frame
    void Update() {
        if (_currentProfile == null) {
            return;
        }
        _underlyingRotation = _recoilTransform.rotation.eulerAngles;
        _currentRotation = Vector3.Lerp(_currentRotation, _targetRotation, _currentProfile._recoilSnappiness * Time.deltaTime);
        _recoilTransform.rotation = Quaternion.Euler(_underlyingRotation + _currentRotation);
        _targetRotation = Vector3.Slerp(_targetRotation, Vector3.zero, _currentProfile._recoilRecoveryDegPerSecond * Time.deltaTime);
        _underlyingRotation = _recoilTransform.rotation.eulerAngles - _currentRotation;
    }

    public void SetRecoilProfile(RecoilProfile profile) {
        _currentProfile = profile;
    }
    
    public void AddRecoil() {
        RecoilProfile r = _currentProfile;
        _targetRotation += new Vector3 (
            r._verticalRecoilDegPerAttack,
            Random.Range(-r._horizontalRecoilDegPerAttack, r._horizontalRecoilDegPerAttack), 
            Random.Range(-r._rotationEffectDegPerAttack, r._rotationEffectDegPerAttack)
        );
    }
}
