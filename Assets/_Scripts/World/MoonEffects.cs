using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoonEffects : MonoBehaviour
{

  [SerializeField] private MoonManager _moonManager;

  private void Update() {
    for (int i = 0; i < _moonManager._moons.Length; i++) {
      float visibility = _moonManager._moons[i].visibility;
      switch (i) {
        case 0:
          break;
        case 1:
          break;
        case 2:
          break;
        case 3:
          break;
        case 4:
          break;
      }
    }
  }

}