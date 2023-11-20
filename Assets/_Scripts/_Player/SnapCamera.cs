using System;
using UnityEngine;

public class SnapCamera : MonoBehaviour
{
    [SerializeField] private Transform _head;
    
    private void Update() {
        transform.position = _head.transform.position;
    }
}
