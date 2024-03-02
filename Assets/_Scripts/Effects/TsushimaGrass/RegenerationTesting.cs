using System.Collections;
using System.Collections.Generic;
using Effects.TsushimaGrass;
using UnityEngine;

public class RegenerationTesting : MonoBehaviour
{
    void FixedUpdate()
    {
        gameObject.GetComponent<GrassTilePrimitives>().GenerateGrassHook();
    }
}
