using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{

    public void Start() {
        this.GetComponent<Rigidbody>().AddForce(transform.up * 20);
    }

}



