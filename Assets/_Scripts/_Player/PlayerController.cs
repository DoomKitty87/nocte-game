using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    [Header("Movement Parameters")] 
    [SerializeField] private float _walkSpeed = 3.0f;
    [SerializeField] private float _gravityScale = 3.0f;
    
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float mouse_lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float mouse_lookSpeedY = 2.0f;
    [SerializeField] private bool mouse_invertMouseX = false;
    [SerializeField] private bool mouse_invertMouseY = false;
    [SerializeField, Range(1, 180)] private float mouse_upperLookLimit = 80.0f;
    [SerializeField, Range(1, 10)] private float mouse_lowerLookLimit = 80.0f;
    
    // private Camera
}
