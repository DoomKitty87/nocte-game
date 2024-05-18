using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyScript : MonoBehaviour
{
  private IEnumerator DestoryCoroutine() {
    yield return new WaitForSeconds(5);
    Destroy(gameObject);
  }
  public void DestroyObject() {
    StartCoroutine(DestoryCoroutine());
  }
}
