using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleScroller : MonoBehaviour
{
    private Scrollbar _scrollbar;
    private readonly WaitForEndOfFrame _waitForEndOfFrame;

    private bool _isClosing;

    private void Awake() => _scrollbar = GetComponent<Scrollbar>();

    private void OnEnable() {
        _isClosing = false;
        StartCoroutine(ResetScroll());
    }

    private void OnDisable() => _isClosing = true;

    public void MoveDown() {
        if (!_isClosing) {
            StartCoroutine(ResetScroll());
        }
    }

    IEnumerator ResetScroll() {
        // For reasons this is necessary 
        yield return _waitForEndOfFrame;
        yield return _waitForEndOfFrame;
        
        _scrollbar.value = 0;
    }
}
