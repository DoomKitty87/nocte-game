using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonManager : MonoBehaviour
{

  [System.Serializable]
  private struct Moon
  {
    public Transform moon;
    public float speed;
    public float distance;
    public Vector3 orbitAxis;
    public float offsetAmplitude;
    public float yBias; // Higher means more likelihood for a less visible orbit
    public float phase;
    public Quaternion initialRotation;
  }

  [SerializeField] private Moon[] _moons;
  [SerializeField] private WorldGenerator _worldGenerator;

  void Start()
  {
    for (int i = 0; i < _moons.Length; i++)
    {
      _moons[i].initialRotation = _moons[i].moon.localRotation;
      _moons[i].phase = Mathf.PerlinNoise(_worldGenerator._seed + i * 42.235f, _worldGenerator._seed + i * 17.532f) * 2 * Mathf.PI;
      _moons[i].orbitAxis += new Vector3(Mathf.Sin(_worldGenerator._seed * 0.529f + i) * _moons[i].offsetAmplitude, _moons[i].yBias * Mathf.Tan(_worldGenerator._seed * 0.185f + i), Mathf.Cos(_worldGenerator._seed * 0.328f + i) * _moons[i].offsetAmplitude);
    }
  }

  void Update()
  {
    for (int i = 0; i < _moons.Length; i++) {
      _moons[i].phase += Time.deltaTime * _moons[i].speed;
      _moons[i].phase %= 2 * Mathf.PI;
      _moons[i].moon.localPosition = Quaternion.LookRotation(_moons[i].orbitAxis) * new Vector3(Mathf.Cos(_moons[i].phase), Mathf.Sin(_moons[i].phase), 0) * (_moons[i].distance / transform.localScale.x);
      _moons[i].moon.localRotation = Quaternion.LookRotation(_moons[i].moon.localPosition, Vector3.up) * _moons[i].initialRotation;
    }
  }
}
