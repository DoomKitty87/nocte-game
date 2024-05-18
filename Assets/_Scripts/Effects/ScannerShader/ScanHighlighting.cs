using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanHighlighting : MonoBehaviour
{

  [SerializeField] private GameObject _highlightPrefab;
  [SerializeField] private Transform _highlightParent;
  private List<ScanData> _highlightTargets = new List<ScanData>();

  public static ScanHighlighting Instance { get; private set; }

  public void AddTarget(ScanData target) {
    _highlightTargets.Add(target);
  }

  private void Awake() {
    Instance = this;
  }

  private void Start() {
    ScanEffect.OnScan += Highlight;
  }

  private void OnDisable() {
    ScanEffect.OnScan -= Highlight;
  }

  public void Highlight() {
    Camera camera = Camera.main;
    for (int i = 0; i < _highlightTargets.Count; i++) {
      Vector3 scanOrigin = ScanEffect.Instance._scanOrigin.position;
      Vector3 endPosition = _highlightTargets[i]._transform.position;
      Vector2 line1 = (new Vector2(endPosition.x, endPosition.z) - new Vector2(scanOrigin.x, scanOrigin.z)).normalized;
      Vector2 line2 = new Vector2(camera.transform.forward.x, camera.transform.forward.z);
      float theta = Vector2.Angle(line1, line2);
      float distance = Vector2.Distance(new Vector2(scanOrigin.x, scanOrigin.z), new Vector2(endPosition.x, endPosition.z));
      if (distance > ScanEffect.Instance._scanDistance || theta > 60) {
        continue;
      }
      _highlightTargets[i]._scanEvent.Invoke();
      float timeOffset = ScanEffect.Instance._inverseSpeedCurve.Evaluate(distance / ScanEffect.Instance._scanDistance) * ScanEffect.Instance._scanDuration + 0.5f;
      GameObject highlight = Instantiate(_highlightPrefab, _highlightTargets[i]._transform.position, Quaternion.identity);
      highlight.GetComponent<ScanHighlight>()._position = _highlightTargets[i]._transform.position;
      highlight.GetComponent<ScanHighlight>()._camera = camera;
      highlight.GetComponent<ScanHighlight>()._startOffset = timeOffset;
      highlight.transform.SetParent(_highlightParent);
    }
  }

}
