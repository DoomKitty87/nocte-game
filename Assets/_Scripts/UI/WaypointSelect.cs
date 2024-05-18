using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointSelect : MonoBehaviour
{

  public int index;

  public void SelectWaypoint() {
    WaypointManager.Instance.SelectWaypoint(index);
  }

}
