using UnityEngine;

public class PlayerCameraFirstPerson : MonoBehaviour
{
    public bool _enabled;
    
    [SerializeField] private float _sensX;
    [SerializeField] private float _sensY;

    [SerializeField] private Transform _model;
    [SerializeField] private Transform _camera;
    
    private float _yRotation;
    private float _xRotation;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _xRotation = _model.eulerAngles.x;   
    }

    private void Update() {
        if (!enabled) return;
        
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * _sensX * 10f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _sensY * 10f;

        _yRotation += mouseX;
        _xRotation -= mouseY;

        _xRotation = Mathf.Clamp(_xRotation, -85, 85);

        _camera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        _model.localRotation = Quaternion.Euler(0, _yRotation, 0);
    }

    // Called after cutscene ends to reset camera position (probably doesnt work but should be easy enough to fix)
    public void ResetRotation() {
        _yRotation = _camera.eulerAngles.x;
        _xRotation = _camera.eulerAngles.y;
    }
}
