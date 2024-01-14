using UnityEngine;

public class TriggerNode : MonoBehaviour
{

  [SerializeField] private ScriptedTriggers _manager;
  [SerializeField] private int _index;

  private void OnTriggerEnter(Collider other) {
    if (other.gameObject.CompareTag("Player")) _manager.Activate(index);
  }
}