using System;
using UnityEngine;

public class SnapCamera : MonoBehaviour
{
    [SerializeField] private Transform _camera;

    // Transform is done in late update in order to allow all other operations to be performed before 
    // frame rendering starts
    private void LateUpdate() {
        var localTransform = transform;
        _camera.position = localTransform.position;
        _camera.rotation = localTransform.rotation;
    }
}
