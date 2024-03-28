using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExtractionGenerator : MonoBehaviour, IInteractable
{

  [SerializeField] private int _extractorIndex;

  public void Interact() {
    ExtractionSiteManager.Instance.ActivateExtractor(_extractorIndex);
  }

}
