using UnityEngine;

public class WaypointTrigger : MonoBehaviour
{

  public Waypoint _waypoint;

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player")) {
      WaypointManager.Instance.CollectedWaypoint(_waypoint);
      gameObject.SetActive(false);
    }
  }

}