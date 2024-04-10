using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExtractionGenerator : MonoBehaviour
{

  [SerializeField] private int _extractorIndex;

  public string interactPrompt => "Activate Extractor";

  public void Interacted() {
    ExtractionSiteManager.Instance.ActivateExtractor(_extractorIndex);
  }

}
