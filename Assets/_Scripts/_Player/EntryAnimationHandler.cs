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
  [SerializeField]private GameObject _uiCanvas;

  private Vector3 _startingPosition;
  private Vector3 _landingPosition;

  private bool _animating = false;
  private float t = 0;
  private bool _startAnimation = false;

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
        // Do player walk out animation
        Done();
      }
    }
  }

  public void StartAnimation() {
    _startAnimation = false;
    Debug.Log(_uiCanvas.name);
    _uiCanvas.SetActive(false);
    _landingPosition = new Vector3(0, WorldGenInfo._worldGenerator.GetHeightValue(new Vector2(0, 0)), 0);
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