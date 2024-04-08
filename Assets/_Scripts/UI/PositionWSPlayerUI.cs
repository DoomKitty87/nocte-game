using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionWSPlayerUI : MonoBehaviour
{

    [SerializeField] private Transform _followTarget;
    [SerializeField] private float _horizontalArm;
    [SerializeField] private float _verticalArm;
    [SerializeField] private float _depthOffset;
    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private float _rotationSpeed = 1f;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;



    private void CalculateTargetPosition() {
        _targetPosition = _followTarget.position + transform.right.normalized * _horizontalArm + Vector3.up * _verticalArm + new Vector3(_followTarget.transform.forward.x, 0, _followTarget.transform.forward.z).normalized * _depthOffset;
        _targetRotation = Quaternion.Euler(0, _followTarget.rotation.eulerAngles.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        CalculateTargetPosition();
        _canvasTransform.position = Vector3.Lerp(_canvasTransform.position, _targetPosition, Time.deltaTime * _movementSpeed);
        _canvasTransform.rotation = Quaternion.RotateTowards(_canvasTransform.rotation, _targetRotation, _rotationSpeed);
    }
}
