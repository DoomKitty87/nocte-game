using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    public PlayerInput PlayerInput;
    static public InputReader Instance { get; private set; }

    private void OnEnable() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(this);
        }
    }
}
