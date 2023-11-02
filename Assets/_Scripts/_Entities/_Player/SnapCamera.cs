using System;
using UnityEngine;

public class SnapCamera : MonoBehaviour
{
    [SerializeField] private Transform _camera;

    private void Update() {
        var localTransform = transform;
        _camera.position = localTransform.position;
        _camera.rotation = localTransform.rotation;
    }
}
