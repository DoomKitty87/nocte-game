using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterColliderObjectHandler : MonoBehaviour
{
    [SerializeField] private Transform _waterColliderObject;

    private WorldGenerator _worldGenerator;

    private void Start() {
        _worldGenerator = WorldGenInfo._worldGenerator;
    } 


    void Update() {
        Vector2 positionXZ = new Vector2(transform.position.x, transform.position.z);
        float height = _worldGenerator.GetWater(positionXZ);
        _waterColliderObject.position = new Vector3(transform.position.x, height, transform.position.z);
    }
}
