using UnityEngine;
using System.Collections;

public class EntryAnimationHandler : MonoBehaviour
{

  [SerializeField] private Transform _landingPod;
  [SerializeField] private Transform _playerTransform;

  [SerializeField] private float _entrySpeed = 1.0f;
  [SerializeField] private float _entryHeight = 10.0f;
  [SerializeField] private float _entryAngle = 45.0f;
  [SerializeField] private AnimationCurve _entryCurve;

  [SerializeField] private GameObject _uiCanvas;

  private Vector3 _startingPosition;
  private Vector3 _landingPosition;

  private void Start() {
    WorldGenerator.GenerationComplete += StartAnimation;
  }

  public void StartAnimation() {
    _uiCanvas.SetActive(false);
    _landingPosition = new Vector3(0, WorldGenInfo._worldGenerator.GetHeightValue(new Vector2(0, 0)), 0);
    _startingPosition = _landingPosition + Quaternion.Euler(_entryAngle, 0, 0) * new Vector3(0, _entryHeight, 0);
    _landingPod.position = _startingPosition;
    _landingPod.rotation = Quaternion.Euler(_entryAngle, 0, 0);
    StartCoroutine(EntryAnimation());
  }

  private IEnumerator EntryAnimation() {
    float t = 0;
    while (t < 1) {
      t += Time.deltaTime * _entrySpeed;
      _landingPod.position = Vector3.Lerp(_startingPosition, _landingPosition, _entryCurve.Evaluate(t));
      yield return null;
    }
    // Do player walk out animation

    Done();
  }
  
  private void Done() {
    _uiCanvas.SetActive(true);
    PlayerWorldGeneratorCompatibility._entryAnimationFinished = true;
  }

}