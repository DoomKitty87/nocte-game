using System;
using UnityEngine;

public class SnapCamera : MonoBehaviour
{
    private Transform _camera;

    private void Start() {
        _camera = GameObject.FindWithTag("MainCamera").transform;
    }

    // Transform is done in late update in order to allow all other operations to be performed before 
    // frame rendering starts
    private void Update() {
        var localTransform = transform;
        _camera.position = localTransform.position;
        _camera.rotation = localTransform.rotation;
    }
}
