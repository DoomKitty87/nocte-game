using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class EntryAnimationHandler : MonoBehaviour
{

  [SerializeField] private Transform _landingPod;
  [SerializeField] private Transform _playerTransform;

  [SerializeField] private float _entrySpeed = 1.0f;
  [SerializeField] private float _entryHeight = 10.0f;
  [SerializeField] private float _entryAngle = 45.0f;
  [SerializeField] private AnimationCurve _entryCurve;

  [SerializeField] private CinemachineCamera _camera;
  [SerializeField] private GameObject _uiCanvas;

  [SerializeField] private float _dropRange = 250.0f;
  

  private Vector3 _startingPosition;
  private Vector3 _landingPosition;

  private bool _animating = false;
  private float t = 0;
  private bool _startAnimation = false;

  public static Vector3 _dropPosition;

  private void Start() {
    WorldGenerator.GenerationComplete += EnableAnimation;
  }

  private void OnDisable() {
    WorldGenerator.GenerationComplete -= EnableAnimation;
  }

  public void EnableAnimation() {
    _startAnimation = true;
  }

  private void Update() {
    if (_startAnimation) StartAnimation();
    if (_animating) {
      if (t < 1) {
        t += Time.deltaTime * _entrySpeed;
        _landingPod.position = Vector3.Lerp(_startingPosition, _landingPosition, _entryCurve.Evaluate(t));
      } else {
        _animating = false;
        _landingPod.position = _landingPosition;
        // Do player walk out animation
        Done();
      }
    }
  }

  public void StartAnimation() {
    Settings.Instance.LoadSettings();
    _startAnimation = false;
    // Debug.Log(_uiCanvas.name);
    _uiCanvas.SetActive(false);

    Vector2 landPos = new Vector2(0, 0);
    int attempts = 0;
    bool foundPos = false;
    while (foundPos == false) {
      if (attempts > 100) {
        Debug.LogError("Could not find a valid landing position");
        landPos = new Vector2(0, 0);
        break;
      }
      landPos = new Vector2(Random.Range(-_dropRange, _dropRange),
        Random.Range(-_dropRange, _dropRange));
      float pointA = WorldGenInfo._worldGenerator.GetHeightValue(landPos);
      float pointB = WorldGenInfo._worldGenerator.GetHeightValue(landPos + new Vector2(1, 0));
      float pointC = WorldGenInfo._worldGenerator.GetHeightValue(landPos + new Vector2(0, 1));

      Vector3 landNorm = new Vector3(pointA - pointB, 1, pointA - pointC).normalized;

      if (WorldGenInfo._worldGenerator.GetRiverValue(landPos) == 0 && landNorm.y > 0.7f && WorldGenInfo._worldGenerator.GetHeightValue(landPos) > WorldGenInfo._lakePlaneHeight) {
        foundPos = true;
      }

      attempts++;
    }

    _landingPosition = new Vector3(landPos.x, WorldGenInfo._worldGenerator.GetHeightValue(landPos), landPos.y);
    _dropPosition = _landingPosition;
    _startingPosition = _landingPosition + Quaternion.Euler(_entryAngle, 0, 0) * new Vector3(0, _entryHeight, 0);
    _landingPod.position = _startingPosition;
    _landingPod.rotation = Quaternion.Euler(_entryAngle, 0, 0);
    _animating = true;
  }
  
  private void Done() {
    _camera.enabled = false;
    _uiCanvas.SetActive(true);
    PlayerWorldGeneratorCompatibility._entryAnimationFinished = true;
  }

}