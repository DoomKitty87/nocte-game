using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoonOrbits : MonoBehaviour
{

  [Serializable]
  private struct Moon
  {
    public Transform moon;
    public float orbitSpeed;
    public float orbitRoamSpeed;
    public float spinSpeed;
    [HideInInspector] public float moonOrbit;
    [HideInInspector] public float moonPhase;
    [HideInInspector] public float moonRotation;
  }

  [SerializeField] private Moon[] _moons;

  private void Start()
  {
    for (int i = 0; i < _moons.Length; i++) {
      _moons[i].moonOrbit = Random.value * 360;
      _moons[i].moonPhase = Random.value;
      _moons[i].moonRotation = Random.value * 360;
    }
  }

  private void Update() {
    for (int i = 0; i < _moons.Length; i++) {
      _moons[i].moonPhase += Time.deltaTime * _moons[i].orbitSpeed;
      _moons[i].moonPhase %= 1;
      _moons[i].moonOrbit += Time.deltaTime * _moons[i].orbitRoamSpeed;
      _moons[i].moonOrbit %= 360;
      _moons[i].moonRotation += Time.deltaTime * _moons[i].spinSpeed;
      _moons[i].moonRotation %= 360;
      _moons[i].moon.localRotation = Quaternion.Euler(new Vector3(_moons[i].moonPhase * 360, _moons[i].moonOrbit,  _moons[i].moonRotation));
    }
  }

}