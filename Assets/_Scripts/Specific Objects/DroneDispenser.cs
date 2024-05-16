using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneDispenser : MonoBehaviour
{

  [SerializeField] private GameObject _dronePrefab;

  [SerializeField] private Transform _droneSpawnPoint;

  [SerializeField] private Transform _door;

  private void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
      Instantiate(_dronePrefab, _droneSpawnPoint.position, Quaternion.identity);
      StartCoroutine(LowerDoor());
      GetComponent<Collider>().enabled = false;
    }
  }

  private IEnumerator LowerDoor() {
    while (_door.position.y > -1.5f) {
      _door.position -= new Vector3(0, 0.1f, 0);
      yield return null;
    }
  }

}
