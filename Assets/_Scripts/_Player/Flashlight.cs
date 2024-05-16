using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{

  [SerializeField] private Light flashlight;

  private void Start() {
    InputReader.Instance.PlayerInput.Player.Flashlight.performed += ToggleFlashlight;
  }

  private void OnDisable() {
    InputReader.Instance.PlayerInput.Player.Flashlight.performed -= ToggleFlashlight;
  }

  private void ToggleFlashlight(InputAction.CallbackContext context) {
    flashlight.enabled = !flashlight.enabled;
  }

}
