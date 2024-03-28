using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour
{

  [SerializeField] private Toggle _cloudsToggle;

  private void Start() {
    LoadSettingsValues();
  }

  private void LoadSettingsValues() {
    _cloudsToggle.isOn = Settings.EnableClouds;
  }

}
