using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]

public class TestAI : MonoBehaviour
{
    private Movement _movement;
    private GameObject _player;

    private void OnValidate()
    {
        _movement = gameObject.GetComponent<Movement>();
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        CheckDistance();
    }

    // deals with encounters (plcl, plnr, plfr, sacl, sanr) 
    private void CheckDistance()
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
}
