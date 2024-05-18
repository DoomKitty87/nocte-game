using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnKeyPress : MonoBehaviour
{
    [SerializeField] private KeyCode _key;
    [SerializeField] private UnityEngine.Events.UnityEvent _onKeyDown;
    [SerializeField] private UnityEngine.Events.UnityEvent _onKeyUp;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(_key)) {
            _onKeyDown.Invoke();
        }
        if (Input.GetKeyUp(_key)) {
            _onKeyUp.Invoke();
        }
    }
}
