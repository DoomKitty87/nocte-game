using System.Collections;
using System.Collections.Generic;
using _Scripts._Entities.Creatures.CreatureAI;
using UnityEngine;

public class DenScript : MonoBehaviour
{
    [SerializeField] private GameObject _coyotePrefab;
    void Update()
    {
        if (transform.childCount < 3) {
            SpawnCoyote();
        }
    }

    private void SpawnCoyote() {
        GameObject coyote = Instantiate(_coyotePrefab, transform);
        coyote.GetComponent<EnemyController>().SetDenPos(transform.position);
    }
}
