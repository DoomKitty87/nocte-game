using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDAOverlay : MonoBehaviour
{

    [SerializeField] private GameObject _pdaCanvas;

    private PlayerInput _input;
    
    void Start() {
        _input = InputReader.Instance.PlayerInput;
        _input.Player.Overlay.performed += _ => {
            _pdaCanvas.SetActive(!_pdaCanvas.activeSelf);
            if (_pdaCanvas.activeSelf) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        };
    }
    
}
