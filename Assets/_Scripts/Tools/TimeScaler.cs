using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : MonoBehaviour
{
  [Range(0, 2)] [SerializeField] private float _timeScale;

  private void Update() {
    Time.timeScale = _timeScale;
  }

}
