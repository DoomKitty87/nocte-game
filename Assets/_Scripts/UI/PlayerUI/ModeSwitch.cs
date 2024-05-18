using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModeSwitch : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _exploration;
  [SerializeField] private TextMeshProUGUI _combat;
  [SerializeField] private Color _activeColor;
  [SerializeField] private Color _inactiveColor;

  public void Combat() {
    _exploration.color = _inactiveColor;
    _combat.color = _activeColor;
  }

  public void Exploration() {
    _exploration.color = _activeColor;
    _combat.color = _inactiveColor;
  }

}
