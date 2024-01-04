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
  [SerializeField] private float _maxSpaceIntensity;
  [SerializeField] private float _maxAsteroidRate;
  [SerializeField] private float _spaceCycleSpeed;
  [SerializeField] private AnimationCurve _spaceCycleCurve;

  [Tooltip("Density of clouds over time.")]
  [SerializeField] private AnimationCurve _cloudDensityCurve;

  [SerializeField] private AnimationCurve _windSpeedCurve;
  
  [Tooltip("Density of rain over time.")]
  [SerializeField] private AnimationCurve _rainDensityCurve;
  

  [SerializeField] private WorldGenerator _worldGenerator;
  [SerializeField] private VolumeProfile _cloudVolume;
  [SerializeField] private Transform _sunTransform;
  [SerializeField] private VisualEffect _rainEffect;
  [SerializeField] private VisualEffect _asteroidEffect;
  [SerializeField] private int _frameUpdateDelay;
  [SerializeField] private int _frameUpdateDelay2;
  

  // X value represents day cycle, Y represents cloud density, and Z represents rain density.
  private Vector4 _weatherState = new Vector3();

  // X is cloud phase, Y is rain phase.
  private Vector3 _weatherPhases;

  private int _seed;
  private VolumetricClouds _clouds;
  private VisualEnvironment _environment;
  private Quaternion _sunInitRot;
  private PhysicallyBasedSky _physicalSky;
  private Vector3 _spaceRotationAxis;
  private float _spacePhase;
  private float _spacePhaseMajor;
  private int _updateCounter;
  private int _updateCounter2;

  private void Start() {
    _sunInitRot = _sunTransform.localRotation;
    _seed = (int)_worldGenerator.GetSeedHash();
    _weatherPhases = new Vector2(Mathf.PerlinNoise(_seed, _seed), Mathf.PerlinNoise(-_seed, -_seed));
    _spaceRotationAxis = new Vector3(
      Mathf.PerlinNoise(_seed % 250, _seed % 250),
      Mathf.PerlinNoise(_seed % 1235, _seed % 1235),
      Mathf.PerlinNoise(_seed % 9523, _seed % 9523)
    ).normalized;
    _cloudVolume.TryGet<VolumetricClouds>(out _clouds);
    _cloudVolume.TryGet<VisualEnvironment>(out _environment);
    _cloudVolume.TryGet<PhysicallyBasedSky>(out _physicalSky);
    _environment.windOrientation.value = Mathf.PerlinNoise(_seed * 10, -_seed * 10) * 360;
  }

  private void Update() {
    _weatherState.x += Time.deltaTime / _dayNightCycleSpeed;
    _weatherState.x %= 1;
    _weatherPhases.x += Mathf.PerlinNoise(_seed + Time.time, _seed + Time.time) * _cloudCycleSpeed * Time.deltaTime;
    _weatherPhases.y += Mathf.PerlinNoise(-_seed - Time.time, -_seed - Time.time) * _rainCycleSpeed * Time.deltaTime;
    _weatherPhases.z += Mathf.PerlinNoise(_seed - Time.time, -_seed + Time.time) * _windCycleSpeed * Time.deltaTime;
    _spacePhase += Time.deltaTime / _dayNightCycleSpeed;
    _spacePhaseMajor += Time.deltaTime / _spaceCycleSpeed;
    _weatherPhases.x %= 1;
    _weatherPhases.y %= 1;
    _weatherPhases.z %= 1;
    _spacePhase %= 1;
    _spacePhaseMajor %= 1;
    float nightFactor = Mathf.SmoothStep(1, 0, Mathf.Pow(_weatherState.x > 0.5f ? 1 - _weatherState.x : _weatherState.x, 1.25f) * 4);
    if (_updateCounter2 < _frameUpdateDelay2) {
      _updateCounter2++;
    } else {
      _updateCounter2 = 0;
      _sunTransform.localRotation = Quaternion.AngleAxis(_weatherState.x * 360, Vector3.right) * _sunInitRot;
      _physicalSky.spaceRotation.value = Quaternion.AngleAxis(_spacePhase * 360, _spaceRotationAxis).eulerAngles;
    }
    if (_updateCounter < _frameUpdateDelay) {
      _updateCounter++;
      return;
    } else {
      _updateCounter = 0;
    }
    _weatherState.y = _cloudDensityCurve.Evaluate(_weatherPhases.x);
    _weatherState.z = _rainDensityCurve.Evaluate(_weatherPhases.y);
    _weatherState.w = _windSpeedCurve.Evaluate(_weatherPhases.z);
    _clouds.shapeFactor.value = _weatherState.y;
    _environment.windSpeed.value = _weatherState.z * _maxWindSpeed;
    _rainEffect.SetInt("RainRate", (int) (_weatherState.z * _rainMaxIntensity));
    _physicalSky.spaceEmissionMultiplier.value = nightFactor * _maxSpaceIntensity * _spaceCycleCurve.Evaluate(_spacePhaseMajor);
    _asteroidEffect.SetFloat("SpawnRate", nightFactor * _maxAsteroidRate);
  }
}