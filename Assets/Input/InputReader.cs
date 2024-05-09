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
    PlayerInput.Driving.Disable();
    PlayerInput.UI.Disable();
    PlayerInput.Player.Enable();
    PlayerInput.Flying.Disable();
  }

  public void EnableFlying() {
    PlayerInput.Player.Disable();
    PlayerInput.UI.Disable();
    PlayerInput.Flying.Enable();
    PlayerInput.Driving.Disable();
  }

  public void EnableDriving() {
    PlayerInput.Player.Disable();
    PlayerInput.UI.Disable();
    PlayerInput.Driving.Enable();
    PlayerInput.Flying.Disable();
  }

  public void EnableUI() {
    PlayerInput.Player.Disable();
    PlayerInput.Driving.Disable();
    PlayerInput.UI.Enable();
    PlayerInput.Flying.Disable();
  }
}
