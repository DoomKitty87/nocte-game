using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanDetectable : MonoBehaviour
{
  
  void Start() {
    ScanHighlighting.Instance.AddTarget(transform);
  }

}
