using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitVisualizer : MonoBehaviour
{

  [SerializeField] private MoonManager _moonManager;
  [SerializeField] private LineRenderer[] _lineRenderers;
  [SerializeField] private int _resolution;

  public void Initialize() {
    for (int i = 0; i < _moonManager._moons.Length; i++) {
      Vector3[] positions = new Vector3[_resolution];
      for (int j = 0; j < _resolution; j++) {
        float radians = Mathf.PI * 2 / _resolution * (j + 1);
        positions[j] = Quaternion.LookRotation(_moonManager._moons[i].orbitAxis) * new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * (_moonManager._moons[i].distance);
      }
      _lineRenderers[i].SetPositions(positions);
    }
  }

}