using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScanHighlight : MonoBehaviour
{
  
  [HideInInspector] public Vector3 _position;
  [HideInInspector] public Camera _camera;
  [HideInInspector] public float _startOffset;

  [SerializeField] private float _highlightDuration = 1.5f;

  private float t = 0;
  private bool _started = false;

  void Update() {
    t += Time.deltaTime;
    if (t >= _startOffset && _started == false) {
      _started = true;
      t = 0;
      GetComponent<Image>().enabled = true;
    }
    if (t >= _highlightDuration && _started == true) {
      Destroy(gameObject);
    }
    transform.position = _camera.WorldToScreenPoint(_position);
  }

}
