using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDAOverlay : MonoBehaviour
{
    [SerializeField] private FadeElementInOut _pdaCanvas;
    [SerializeField] private FadeElementInOut _pdaCameraCanvas;
    [SerializeField] private FadeElementInOut _mainCanvas;

    [SerializeField] private FadeElementInOut _canvas1;
	[SerializeField] private FadeElementInOut _canvas2;
	[SerializeField] private FadeElementInOut _canvas3;
	[SerializeField] private FadeElementInOut _canvas4;

	private PlayerInput _input;
    
    void Start() {
        _pdaCanvas._canvasGroup.alpha = 0;
        _pdaCanvas._canvasGroup.interactable = false;
        _pdaCanvas._canvasGroup.blocksRaycasts = false;
        _pdaCameraCanvas._canvasGroup.alpha = 0;
        _pdaCameraCanvas._canvasGroup.interactable = false;
        _pdaCameraCanvas._canvasGroup.blocksRaycasts = false;
        _mainCanvas._canvasGroup.alpha = 1;
        _mainCanvas._canvasGroup.interactable = true;
        _mainCanvas._canvasGroup.blocksRaycasts = true;
        _input = InputReader.Instance.PlayerInput;

        _input.Player.Overlay.performed += _ => {
						InputReader.Instance.EnableUI();
						_mainCanvas.FadeOut();
	          _pdaCanvas.FadeIn();
	          _pdaCameraCanvas.FadeIn();
	          Cursor.lockState = CursorLockMode.None;
	          Cursor.visible = true;

	          _canvas1.FadeIn();
	          _canvas2.FadeOut();
	          _canvas3.FadeOut();
	          _canvas4.FadeOut();
				};

        _input.UI.ClosePDA.performed += _ => {
						InputReader.Instance.EnablePlayer();
						_mainCanvas.FadeIn();
						_pdaCanvas.FadeOut();
						_pdaCameraCanvas.FadeOut();
						Cursor.lockState = CursorLockMode.Locked;
						Cursor.visible = false;
        };
		}
};  
