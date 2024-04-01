using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiveGlass : MonoBehaviour
{
    private Rigidbody[] rb;
    private int index = 0;

    private void Start()
    {
        rb = transform.GetChild(1).GetComponentsInChildren<Rigidbody>();

        transform.GetChild(0).GetComponent<BulletInteract>().Interaction += Explode;
    }

    private void Explode(float damage, Vector3 position) {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);

        foreach (Rigidbody r in rb)
        {
            r.isKinematic = false;
            r.AddExplosionForce(1000, position, 2);
        }

        Invoke(nameof(DestroyGlass), .25f);
    }


    private void DestroyGlass() {
        foreach (Rigidbody r in rb)
        {
            Invoke(nameof(DestroyObject), Random.Range(0f, 0.5f));
        }
    }

    private void DestroyObject() {
        Destroy(rb[index].gameObject);
        index++;
    }
}
