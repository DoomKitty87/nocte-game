using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMove : MonoBehaviour
{

  [HideInInspector] public float _speed = 1f;

  [SerializeField] private float _lifetime;

  private void Start()
  {
    Destroy(gameObject, _lifetime);
  }

  private void Update() {
    var t = transform;
    t.position += t.forward * (_speed * Time.deltaTime);
  }

}
