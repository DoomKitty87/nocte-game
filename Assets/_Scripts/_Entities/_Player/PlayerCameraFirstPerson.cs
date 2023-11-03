using UnityEngine;

public class PlayerCameraFirstPerson : MonoBehaviour
{
    [SerializeField] private float _sensX;
    [SerializeField] private float _sensY;

    [SerializeField] private Transform _model;
    [SerializeField] private Transform _camera;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * _sensX * 10f;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * _sensY * 10f;

        if (mouseX != 0.0f) _model.Rotate(Vector3.up, mouseX);
        if (mouseY != 0.0f) _camera.Rotate(Vector3.right, -mouseY);
    }
}
