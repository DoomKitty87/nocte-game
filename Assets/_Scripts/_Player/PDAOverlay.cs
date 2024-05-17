using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDAOverlay : MonoBehaviour
{

    [SerializeField] private FadeElementInOut _pdaCanvas;
    [SerializeField] private FadeElementInOut _pdaCameraCanvas;

    private bool _isPDAOpen;
    
    private PlayerInput _input;
    
    void Start() {
        _pdaCanvas._canvasGroup.alpha = 0;
        _pdaCanvas._canvasGroup.interactable = false;
        _pdaCanvas._canvasGroup.blocksRaycasts = false;
        _pdaCameraCanvas._canvasGroup.alpha = 0;
        _pdaCameraCanvas._canvasGroup.interactable = false;
        _pdaCameraCanvas._canvasGroup.blocksRaycasts = false;
        _isPDAOpen = false;
        _input = InputReader.Instance.PlayerInput;
        _input.Player.Overlay.performed += _ => {
            if (_isPDAOpen) {
                _pdaCanvas.FadeOut();
                _pdaCameraCanvas.FadeOut();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else {
                _pdaCanvas.FadeIn();
                _pdaCameraCanvas.FadeIn();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            _isPDAOpen = !_isPDAOpen;
        };
    }
    
}
