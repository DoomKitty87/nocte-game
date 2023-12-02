using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleShipButton : MonoBehaviour, IInteractable
{

    private Vector3 _startPosition;
    
    [SerializeField] private float _timeToClick;
    [SerializeField] private float _distanceToClick;

    private void Start() {
        _startPosition = transform.position;
    }

    public void Interact() {
        StartCoroutine(DoButtonThing());
    }

    private IEnumerator DoButtonThing() {
        float time = 0;
        while (time < _timeToClick / 2) {
            time += Time.deltaTime;
            transform.position = _startPosition + Vector3.down * Mathf.Lerp(
                0,
                _distanceToClick,
                time / (_timeToClick / 2)
            );
            yield return null;
        }

        // yup this is bad but this script really should only be in the test scene
        time = 0;
        while (time < _timeToClick / 2) {
            time += Time.deltaTime;
            transform.position = _startPosition + Vector3.down * Mathf.Lerp(
                _distanceToClick,
                0,
                time / (_timeToClick / 2)
            );
            yield return null;
        }

        SceneManager.LoadScene("Game Scene");
        StopAllCoroutines();
    }
}
