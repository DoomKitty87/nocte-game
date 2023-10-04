using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]

public class TestAI : MonoBehaviour
{
    public string _species;
    public bool _flocking;
    public bool _infighter;
    public bool _leader;
    public bool _follower;
    public int _priority;
    private Movement _movement;
    private GameObject _player;

    private void OnValidate()
    {
        _movement = gameObject.GetComponent<Movement>();
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _priority = Random.Range(0, 1000000);
    }

    private void Update()
    {
        CheckDistancePlayer();
    }

    // deals with encounters (plcl, plnr, plfr, sacl, sanr) 
    private void CheckDistancePlayer()
    {
        float distancePlayer = Vector3.Distance(this.transform.position, _player.transform.position);
        if (distancePlayer < 5)
        {
            _movement.SetBoolInputs(true, false);
        } else
        {
            _movement.SetBoolInputs(false, false);
            _movement.SetInputVector(_player.transform.position - this.transform.position);
        }
    }

    private List<GameObject> CheckDistanceSameSpecies()
    {
        Collider[] sameAnimalOverlap = Physics.OverlapSphere(transform.position, 5f);
        List<GameObject> sameAnimalsInRange = new List<GameObject>();
        foreach (Collider entity in sameAnimalOverlap)
        {
            if (entity.gameObject.GetComponent<TestAI>()._species == _species)
            {
                sameAnimalsInRange.Add(entity.gameObject);
            }
        }
        return sameAnimalsInRange;
    }

    private void CreateFlock()
    {

    }
}
