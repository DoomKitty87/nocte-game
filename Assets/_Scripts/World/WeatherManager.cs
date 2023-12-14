using UnityEngine;

public class WeatherManager : MonoBehaviour
{

  [Tooltip("Day/night speed, in seconds per full cycle.")]
  [SerializeField] private float _dayNightCycleSpeed;

  [Tooltip("Density of clouds over time.")]
  [SerializeField] private AnimationCurve _cloudDensityCurve;

  [Tooltip("Density of rain over time.")]
  [SerializeField] private AnimationCurve _rainDensityCurve;

  [SerializeField] private WorldGenerator _worldGenerator;

  // X value represents day cycle, Y represents cloud density, and Z represents rain density.
  private Vector3 _weatherState = new Vector3();

  // X is cloud phase, Y is rain phase.
  private Vector2 _weatherPhases;

  private int _seed;

  private void Start() {
    _seed = (int)_worldGenerator.GetSeedHash();
    _weatherPhases = new Vector2(Mathf.PerlinNoise(_seed, _seed), Mathf.PerlinNoise(-_seed, -_seed));
  }

  private void Update() {
    _weatherState += Vector3.right * Time.deltaTime / _dayNightCycleSpeed;
    _weatherPhases += new Vector2(Mathf.PerlinNoise(_seed + Time.time, _seed + Time.time), Mathf.PerlinNoise(-_seed - Time.time, -_seed - Time.time));
    _weatherState = new Vector3(_weatherState.x, _cloudDensityCurve.Evaluate(_weatherPhases.x), _rainDensityCurve.Evaluate(_weatherPhases.y));
  }
}