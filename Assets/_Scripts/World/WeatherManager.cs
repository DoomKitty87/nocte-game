using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

public class WeatherManager : MonoBehaviour
{

  [Tooltip("Day/night speed, in seconds per full cycle.")]
  [SerializeField] private float _dayNightCycleSpeed;

  [SerializeField] private float _cloudCycleSpeed;
  [SerializeField] private float _windCycleSpeed;
  [SerializeField] private float _rainCycleSpeed;
  [SerializeField] private int _rainMaxIntensity;
  [SerializeField] private float _maxWindSpeed;

  [Tooltip("Density of clouds over time.")]
  [SerializeField] private AnimationCurve _cloudDensityCurve;

  [SerializeField] private AnimationCurve _windSpeedCurve;
  
  [Tooltip("Density of rain over time.")]
  [SerializeField] private AnimationCurve _rainDensityCurve;
  

  [SerializeField] private WorldGenerator _worldGenerator;
  [SerializeField] private VolumeProfile _cloudVolume;
  [SerializeField] private Transform _sunTransform;
  [SerializeField] private VisualEffect _rainEffect;
  

  // X value represents day cycle, Y represents cloud density, and Z represents rain density.
  private Vector4 _weatherState = new Vector3();

  // X is cloud phase, Y is rain phase.
  private Vector3 _weatherPhases;

  private int _seed;
  private VolumetricClouds _clouds;
  private VisualEnvironment _environment;
  private Quaternion _sunInitRot;

  private void Start() {
    _sunInitRot = _sunTransform.localRotation;
    _seed = (int)_worldGenerator.GetSeedHash();
    _weatherPhases = new Vector2(Mathf.PerlinNoise(_seed, _seed), Mathf.PerlinNoise(-_seed, -_seed));
    _cloudVolume.TryGet<VolumetricClouds>(out _clouds);
    _cloudVolume.TryGet<VisualEnvironment>(out _environment);
    _environment.windOrientation.value = Mathf.PerlinNoise(_seed * 10, -_seed * 10) * 360;
  }

  private void Update() {
    _weatherState.x += Time.deltaTime / _dayNightCycleSpeed;
    _weatherState.x %= 1;
    _weatherPhases.x += Mathf.PerlinNoise(_seed + Time.time, _seed + Time.time) * _cloudCycleSpeed * Time.deltaTime;
    _weatherPhases.y += Mathf.PerlinNoise(-_seed - Time.time, -_seed - Time.time) * _rainCycleSpeed * Time.deltaTime;
    _weatherPhases.z += Mathf.PerlinNoise(_seed - Time.time, -_seed + Time.time) * _windCycleSpeed * Time.deltaTime;
    _weatherPhases.x %= 1;
    _weatherPhases.y %= 1;
    _weatherPhases.z %= 1;
    _weatherState.y = _cloudDensityCurve.Evaluate(_weatherPhases.x);
    _weatherState.z = _rainDensityCurve.Evaluate(_weatherPhases.y);
    _weatherState.w = _windSpeedCurve.Evaluate(_weatherPhases.z);
    //_clouds.shapeFactor.value = _weatherState.y;
    _environment.windSpeed.value = _weatherState.z * _maxWindSpeed;
    _sunTransform.localRotation = Quaternion.AngleAxis(_weatherState.x * 360, Vector3.right) * _sunInitRot;
    _rainEffect.SetInt("RainRate", (int) (_weatherState.z * _rainMaxIntensity));
  }
}