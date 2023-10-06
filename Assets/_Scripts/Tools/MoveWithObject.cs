using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithObject : MonoBehaviour
{
  [SerializeField] private GameObject _targetObject;
  private Vector3 _currentPosition;
  private Vector3 _targetObjectOffset;
  private void Start() {
    _targetObjectOffset = _currentPosition - _targetObject.transform.position;
  }
  private void Update() {
    transform.position = _targetObject.transform.position;
  }

}
