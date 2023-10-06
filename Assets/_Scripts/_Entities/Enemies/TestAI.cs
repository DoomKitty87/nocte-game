using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Movement))]

public class TestAI : MonoBehaviour
{
    public string _species;
    public bool _flockable;
    public bool _inflock;
    public bool _infighter;
    public bool _leading;
    public bool _following;
    public GameObject _leader;
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
        List<GameObject> sameAnimals = CheckDistanceSameSpecies();
        if (sameAnimals.Count() > 0 && _flockable)
        {
            JoinFlock(sameAnimals);
        }
        if (_following)
        {
            FollowLeader();
        } else
        {
            CheckDistancePlayer();
        }
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
        Collider[] sameAnimalOverlap = Physics.OverlapSphere(transform.position, 30f);
        List<GameObject> sameAnimalsInRange = new List<GameObject>();
        foreach (Collider entity in sameAnimalOverlap)
        {
            TestAI entityAI = entity.gameObject.GetComponent<TestAI>();
            if (entityAI == null)
            {
                continue;
            }
            if (entity.gameObject.GetComponent<TestAI>()._species == this._species)
            {
                sameAnimalsInRange.Add(entity.gameObject);
            }
        }
        return sameAnimalsInRange;
    }

    private void JoinFlock(List<GameObject> animals)
    {
        animals = animals.OrderByDescending(an => an.GetComponent<TestAI>()._priority).ToList();
        if (animals[0].GetComponent<TestAI>()._priority > this._priority && animals[0] != this)
        {
            _leading = false;
            _following = true;
            _leader = animals[0];
        } else
        {
            _leading = true;
            _following = false;
            _leader = null;
        }
    }

    private void FollowLeader()
    {
        transform.LookAt(_leader.transform);
        _movement.SetInputVector(_leader.transform.forward);
    }
}
