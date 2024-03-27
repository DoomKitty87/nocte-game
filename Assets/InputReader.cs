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

        PlayerInput = new PlayerInput();
    }

    public void EnablePlayer() {
        Debug.Log("Enable Player");
        PlayerInput.Driving.Disable();
        PlayerInput.UI.Disable();
        PlayerInput.Player.Enable();
    }

    public void EnableDriving() {
        Debug.Log("Enable Driving");
        PlayerInput.Player.Disable();
        PlayerInput.UI.Disable();
        PlayerInput.Driving.Enable();
    }

    public void EnableUI() {
        PlayerInput.Player.Disable();
        PlayerInput.Driving.Disable();
        PlayerInput.UI.Enable();
    }
}
