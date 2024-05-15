using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandCenter : MonoBehaviour
{

  [SerializeField] private GameObject _lowPowerScreen;
  [SerializeField] private GameObject _fullPowerScreen;

  [SerializeField] private GameObject _mainInterface;
  
  private bool _powered = false;

  private void Start() {
    _lowPowerScreen.SetActive(true);
    _fullPowerScreen.SetActive(false);
  }

  public void EnableBackupPower() {
    _lowPowerScreen.SetActive(false);
    _fullPowerScreen.SetActive(true);
    _powered = true;
  }

  public void EnableMainInterface() {
    if (!_powered) return;
    _mainInterface.SetActive(true);
  }

}
