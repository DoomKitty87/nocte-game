using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExtractionGenerator : MonoBehaviour
{

  [SerializeField] private int _extractorIndex;

  public string interactPrompt => "Activate Extractor";

  [SerializeField] private Transform _lever;

  private bool _used = false;

  public void Interacted() {
    if (_used) return;
    ExtractionSiteManager.Instance.ActivateExtractor(_extractorIndex);
    _used = true;
    StartCoroutine(Animation());
  }

  private IEnumerator Animation() {
    float t = 0;
    while (t < 1) {
      _lever.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(50, -50, t));
      t += Time.deltaTime;
      yield return null;
    }
  }

}
