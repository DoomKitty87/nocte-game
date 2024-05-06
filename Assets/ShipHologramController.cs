using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ShipHologramController : MonoBehaviour
{

    [SerializeField] private VisualEffect VFX;

    [SerializeField] private float rotationSpeed;

    [SerializeField] private Vector3 rotation;
    void Update() {
        rotation += Vector3.up * (rotationSpeed * Time.deltaTime);
        VFX.SetVector3("Rotation", rotation);
    }
}
