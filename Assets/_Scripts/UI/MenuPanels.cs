using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanels : MonoBehaviour
{

  [SerializeField] private GameObject[] _panels;

  public void OpenPanel(int panelIndex) {
    for (int i = 0; i < _panels.Length; i++) {
      _panels[i].SetActive(i == panelIndex);
    }
  }

}
