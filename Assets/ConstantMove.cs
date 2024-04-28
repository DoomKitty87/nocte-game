using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMove : MonoBehaviour
{

  [SerializeField] private float _speed;

  [SerializeField] private float _lifetime;

  private void Start()
  {
    Destroy(gameObject, _lifetime);
  }

  void Update()
  {
    transform.position += transform.forward * _speed * Time.deltaTime;
  }

}
