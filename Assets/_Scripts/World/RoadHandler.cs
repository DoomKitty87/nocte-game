using UnityEngine;
using System.Collections.Generic;

public class RoadHandler : MonoBehaviour
{

  public void SetRoadPositions(Vector3 pos1, Vector3 pos2) {
    GetComponent<LineRenderer>().positionCount = 2;
    GetComponent<LineRenderer>().SetPositions(new Vector3[] {pos1, pos2});
  }
}