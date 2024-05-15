using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CommandCenter : MonoBehaviour
{

  [SerializeField] private GameObject _lowPowerScreen;
  [SerializeField] private GameObject _fullPowerScreen;

  [SerializeField] private TextMeshProUGUI _distanceText;

  [SerializeField] private GameObject _mainInterface;
  
  private bool _powered = false;

  private void Start() {
    _lowPowerScreen.SetActive(true);
    _fullPowerScreen.SetActive(false);
    int nearestSite = (int) PlaceStructures.Instance.GetNearestSite(transform.position);
    _distanceText.text = $"Nearest Site: {nearestSite}m";
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
