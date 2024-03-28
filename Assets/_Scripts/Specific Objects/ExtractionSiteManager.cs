using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractionSiteManager : MonoBehaviour
{

  public static ExtractionSiteManager Instance {get; private set;}

  private bool[] extractorsActive = new bool[3] {false, false, false};
  private int extractorsActiveCount = 0;
  
  void Awake() {
    Instance = this;
  }

  public void ActivateExtractor(int index) {
    if (extractorsActive[index]) return;
    extractorsActive[index] = true;
    extractorsActiveCount++;
    if (extractorsActiveCount == 3) {
      GeneratorActivated();
    }
  }

  private void GeneratorActivated() {
    PlayerMetaProgression.Instance.ObtainBlueprint(0);
    // Animation or whatever
    Debug.Log("Generator activated; player obtained Utility Blueprint (Grappling Hook)");
  }
  
}
