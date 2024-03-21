using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DenSpawner : MonoBehaviour
{
    // should:
    // spawn one den per chunk
    // pick random ppint in each chunk
    // get height at point
    // instantiate den prefab at the point
    // assign player transform to den object
    // 
    // 
    // 

    [SerializeField] private WorldGenerator _worldGenerator;
    [SerializeField] private GameObject _denPrefab;
    
    
    private void InstantiateDen() {
        float x = Random.Range(0, 960);
        float z = Random.Range(0, 960);
        float y = _worldGenerator.GetHeightValue(new Vector2(x, z));
        
        
    } 
}
