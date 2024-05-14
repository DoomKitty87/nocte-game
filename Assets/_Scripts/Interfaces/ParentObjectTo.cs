using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentObjectTo : MonoBehaviour
{
    [SerializeField] private GameObject _parentObject;

    public void ParentToObject() {
        transform.parent = _parentObject.transform;
    }
}
