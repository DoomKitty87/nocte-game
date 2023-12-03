using UnityEditor;
using UnityEngine;
// =================================
// Desc: Gets RB velocity, shows as value in scene
// =================================
[RequireComponent(typeof(Rigidbody))]
public class DisplaySpeed : MonoBehaviour
{
  [SerializeField] private Rigidbody _rigidbody;
  [SerializeField] private float _speed;
  private void OnValidate() {
    _rigidbody = gameObject.GetComponent<Rigidbody>();
  }

  private void Update() {
    _speed = _rigidbody.velocity.magnitude;
  }

  #if UNITY_EDITOR
  private void OnDrawGizmos() {
    Handles.Label(transform.position + Vector3.up * 2, _speed.ToString());
  }
  #endif
}
