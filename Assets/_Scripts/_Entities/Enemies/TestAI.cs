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
    public GameObject _leader;
    public int _priority;
    private Movement _movement;
    private GameObject _player;

    public enum AIState
    {
        Leading,
        Following,
        Wandering,
        Inactive
    }
    public AIState _state;

    private void OnValidate()
    {
        _movement = gameObject.GetComponent<Movement>();
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _priority = Random.Range(0, 1000000);
        _state = AIState.Wandering;
    }

    private void Update()
    {
        List<GameObject> sameAnimals = CheckDistanceSameSpecies();
        if (sameAnimals.Count() > 0 && _flockable)
        {
            JoinFlock(sameAnimals);
        }
        if (_state == AIState.Following)
        {
            FollowLeader();
        } else
        {
            CheckDistancePlayer();
        }
    }

    private void CheckDistancePlayer()
    {
        float distancePlayer = Vector3.Distance(this.transform.position, _player.transform.position);
        if (distancePlayer < 5)
        {
            _movement.SetBoolInputs(true, false);
        } else if (distancePlayer < 20)
        {
            _movement.SetBoolInputs(false, false);
            _movement.SetInputVector(_player.transform.position - this.transform.position);
        } else
        {
            _movement.SetBoolInputs(false, false);
            _movement.SetInputVector(new Vector3(0, 0, 0));
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
            _state = AIState.Following;
            _leader = animals[0];
        } else
        {
            _state = AIState.Leading;
            _leader = null;
        }
    }

    private void FollowLeader()
    {
        if (Vector3.Distance(_leader.transform.position, this.transform.position) > 2f)
        {
            transform.LookAt(_leader.transform);
            _movement.SetInputVector(this.transform.forward);
        } else if (Vector3.Distance(_leader.transform.position, this.transform.position) > 15f)
        {
            _movement.SetInputVector(_leader.transform.forward);
        } else
        {
            _state = AIState.Wandering;
        }
    }
}