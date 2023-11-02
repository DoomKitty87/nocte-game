using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraFirstPerson : MonoBehaviour
{
    [SerializeField] private float _sensX;
    [SerializeField] private float _sensY;

    [SerializeField] private Transform _model;
    [SerializeField] private Transform _camera;

    private float xRotation;
    private float yRotation;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * _sensX * 10f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _sensY * 10f;

        // Yeah it's stupid
        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -85, 85);

        _camera.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        _model.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
