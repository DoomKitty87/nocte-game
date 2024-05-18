using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public struct ScanData
{
  
  public UnityEvent _scanEvent;
  public Transform _transform;

}

public class ScanDetectable : MonoBehaviour
{

  [SerializeField] private ScanData _scanData;
  
  void Start() {
    ScanHighlighting.Instance.AddTarget(_scanData);
  }

}
