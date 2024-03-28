using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInteract : MonoBehaviour
{
    public delegate void OnInteraction(float damage, Vector3 hitPoint);
    
    public event OnInteraction Interaction;
    public void Interact(float damage, Vector3 hitPoint) {
        Interaction?.Invoke(damage, hitPoint);
    }
}
