using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonManager : MonoBehaviour
{

  [System.Serializable]
  public struct Moon
  {
    public Transform moon;
    public float visibility;
    public AnimationCurve visibilityCurve;
    public float speed;
    public float distance;
    public Vector3 orbitAxis;
    public float orbitAdjustSpeed;
    public float orbitAdjustPhase;
    public float offsetAmplitude;
    public float spinOffsetAmplitude;
    public float spinSpeed;
    public Vector3 spinAxis; // Represented as an offset from its orbit axis
    public float yBias; // Higher means more likelihood for a less visible orbit
    public float phase;
    public float spinPhase;
    public Quaternion initialRotation;
  }

  [SerializeField] public Moon[] _moons;
  [SerializeField] private WorldGenerator _worldGenerator;
  [SerializeField] private OrbitVisualizer _orbitVisualizer;
  public float _distanceMultiplier = 1.0f; // Used to scale the distance of the moons from the planet

  private int _seed;

  private void Awake() {
    if (_worldGenerator == null) {
      Debug.LogWarning($"No WorldGeneratorObject found on {this.name}");
      //this.enabled = false;
      _seed = 0;
    } else {
      _seed = _worldGenerator.Seed;
    }
  }

  void Start()
  {
    for (int i = 0; i < _moons.Length; i++)
    {
      _moons[i].initialRotation = _moons[i].moon.localRotation;
      _moons[i].phase = Mathf.PerlinNoise(_seed + i * 42.235f, _seed + i * 17.532f) * 2 * Mathf.PI;
      _moons[i].orbitAxis += new Vector3(Mathf.Sin(_seed * 0.529f + i) * _moons[i].offsetAmplitude, _moons[i].yBias * Mathf.Tan(_seed * 0.185f + i), Mathf.Cos(_seed * 0.328f + i) * _moons[i].offsetAmplitude);
      _moons[i].spinPhase = Mathf.PerlinNoise(_seed + i * 12.291f, _seed + i * 91.215f) * 2 * Mathf.PI;
      _moons[i].spinAxis += new Vector3(Mathf.Sin(_seed * 5.123f + i) * _moons[i].offsetAmplitude, _moons[i].yBias * Mathf.Tan(_seed * 8.519f + i), Mathf.Cos(_seed * 2.614f + i) * _moons[i].spinOffsetAmplitude);
    }
    if (_orbitVisualizer) _orbitVisualizer.Initialize();
  }

  void Update()
  {
    for (int i = 0; i < _moons.Length; i++) {
      _moons[i].orbitAdjustPhase += Time.deltaTime * _moons[i].orbitAdjustSpeed;
      _moons[i].orbitAdjustPhase %= 2 * Mathf.PI;
      _moons[i].phase += Time.deltaTime * _moons[i].speed;
      _moons[i].phase %= 2 * Mathf.PI;
      _moons[i].spinPhase += Time.deltaTime * _moons[i].spinSpeed;
      _moons[i].spinPhase %= 2 * Mathf.PI;
      Vector3 tmpOrbitAxis = Quaternion.AngleAxis(_moons[i].orbitAdjustPhase / (2 * Mathf.PI) * 360, Vector3.Cross(_moons[i].orbitAxis, Vector3.up)) * _moons[i].orbitAxis;
      _moons[i].moon.localPosition = Quaternion.LookRotation(tmpOrbitAxis, Vector3.Cross(tmpOrbitAxis, Vector3.right)) * new Vector3(Mathf.Cos(_moons[i].phase), Mathf.Sin(_moons[i].phase), 0) * (_moons[i].distance / transform.localScale.x) * _distanceMultiplier;
      _moons[i].moon.localRotation = Quaternion.AngleAxis(_moons[i].spinPhase / (2 * Mathf.PI) * 360, Quaternion.Euler(_moons[i].spinAxis.x, _moons[i].spinAxis.y, _moons[i].spinAxis.z) * tmpOrbitAxis) * _moons[i].initialRotation;
      _moons[i].visibility = _moons[i].visibilityCurve.Evaluate(_moons[i].moon.localPosition.y / _moons[i].distance * transform.localScale.x);
      _orbitVisualizer.UpdateMoon(i);
    }
  }
}
