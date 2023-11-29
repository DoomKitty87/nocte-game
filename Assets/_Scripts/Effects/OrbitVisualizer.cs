using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitVisualizer : MonoBehaviour
{

  [SerializeField] private MoonManager _moonManager;
  [SerializeField] private LineRenderer[] _lineRenderers;
  [SerializeField] private int _resolution;

  public void Initialize() {
    for (int i = 0; i < _moonManager.moons.Length; i++) {
      Vector3[] positions = new Vector3[Mathf.Pow(2, _resolution + 1) * 4];
      positions[0] = Quaternion.LookRotation(_moonManager._moons[i].orbitAxis) * new Vector3(1, 0, 0) * (_moonManager._moons[i].distance);
      positions[1] = Quaternion.LookRotation(_moonManager._moons[i].orbitAxis) * new Vector3(0, 1, 0) * (_moonManager._moons[i].distance);
      positions[2] = Quaternion.LookRotation(_moonManager._moons[i].orbitAxis) * new Vector3(-1, 0, 0) * (_moonManager._moons[i].distance);
      positions[3] = Quaternion.LookRotation(_moonManager._moons[i].orbitAxis) * new Vector3(0, -1, 0) * (_moonManager._moons[i].distance);
      int n = 3;
      for (int j = 1; j < _resolution; j++) {
        for (int k = 0, c = n; k < c; k++) {
          positions[n] = (positions[k] + positions[k == c - 1 ? 0 : k + 1]) / 2;
          n++;
        }
      }
      _lineRenderers[i].SetPositions(positions);
    }
  }

}