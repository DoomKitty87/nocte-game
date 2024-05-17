using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneDispenser : MonoBehaviour
{

  [SerializeField] private GameObject _dronePrefab;

  [SerializeField] private Transform _droneSpawnPoint;

  [SerializeField] private Transform _door;

  private float _doorYInit;

  [SerializeField] private float _cooldown;

  private void Start() {
    _doorYInit = _door.localPosition.y;
  }

  private void OnTriggerStay(Collider other) {
    if (other.CompareTag("Player")) {
      Instantiate(_dronePrefab, _droneSpawnPoint.position, Quaternion.identity);
      StartCoroutine(LowerDoor());
      GetComponent<Collider>().enabled = false;
    }
  }

  public void DisableSpawner() {
    StopAllCoroutines();
    GetComponent<Collider>().enabled = false;
  }

  private IEnumerator LowerDoor() {
    while (_door.localPosition.y > 0f) {
      _door.localPosition -= Vector3.up * Time.deltaTime;
      yield return null;
    }
    yield return new WaitForSeconds(_cooldown);
    while (_door.localPosition.y < _doorYInit) {
      _door.localPosition += Vector3.up * Time.deltaTime;
      yield return null;
    }
    yield return new WaitForSeconds(1f);
    GetComponent<Collider>().enabled = true;
  }

}
