using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceObject : MonoBehaviour
{
    [SerializeField] private GameObject _testPrefab;
    public int x;
    public int z;

    private void Start() {
        Instantiate(AmalgamNoise.GetPosition(WorldGenInfo.AmalgamNoiseParams), )
    }
}
