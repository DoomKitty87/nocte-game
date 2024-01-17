using System;
using UnityEngine;

public class ExtractChildren : MonoBehaviour
{
  private void Awake() {
    int numberOfChildren = transform.childCount;
    for (int i = 0; i < numberOfChildren; i++) {
      transform.GetChild(0).parent = null;
    }
    
    Destroy(transform.gameObject);
  }
}
