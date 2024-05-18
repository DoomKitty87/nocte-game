using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class ScanHighlight : MonoBehaviour
{
  
  [HideInInspector] public UnityEngine.Vector3 _position;
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
    UnityEngine.Vector3 desiredPosition = _camera.WorldToScreenPoint(_position);
    desiredPosition.z = 0;
    transform.position = UnityEngine.Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 20f);
  }

}
