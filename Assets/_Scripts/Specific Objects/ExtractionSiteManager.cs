using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExtractionSiteManager : MonoBehaviour
{

  public static ExtractionSiteManager Instance {get; private set;}

  [SerializeField] private TextMeshProUGUI _statusText;

  [SerializeField] private TextMeshProUGUI[] _extractorStatus;
  [SerializeField] private GameObject _activated;

  [SerializeField] private Dialogue _dialogue;

  private bool[] extractorsActive = new bool[3] {false, false, false};
  private int extractorsActiveCount = 0;
  
  void Awake() {
    Instance = this;
  }

  public void ActivateExtractor(int index) {
    if (extractorsActive[index]) return;
    extractorsActive[index] = true;
    _extractorStatus[index].text = "EXTRACTOR " + index
    + " ACTIVE";
    extractorsActiveCount++;
    if (extractorsActiveCount == 3) {
      GeneratorActivated();
    }
  }

  private void GeneratorActivated() {
    _statusText.text = "EXTRACTORS ONLINE\nGENERATOR ONLINE";
    _activated.SetActive(true);
    // PlayerMetaProgression.Instance.ObtainBlueprint(3);
    // Animation or whatever
    //Debug.Log("Generator activated; player obtained Utility Blueprint (Grappling Hook)");
  }

  public void ActivateTerminal() {
    _activated.SetActive(false);
    PlayerMetaProgression.Instance.Unlock(9);
    DialogueHandler.Instance.PlayDialogue(_dialogue);
    CommandCenter.Instance.DoneWithExtraction();
    Vector3 commandPosition = PlaceStructures.Instance.GetCommandCenter();
    Waypoint waypoint = new Waypoint();
    waypoint._position = commandPosition;
    waypoint._name = "Command Center";
    WaypointManager.Instance.CollectedWaypoint(waypoint);
    // Give player coordinates of command center
    Debug.Log("Generator activated; player obtained Utility Blueprint (Grappling Hook)");
  }
  
}
