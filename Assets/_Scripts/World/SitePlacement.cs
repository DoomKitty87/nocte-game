using UnityEngine;
using System.Collections.Generic;

public class SitePlacement : MonoBehaviour
{

  // Meant to be put on a GameObject that is instantiated as a site and has multiple components to be placed on ground.

  [System.Serializable]
  private struct Component
  {
    public Transform transform;
    public bool alignToNormal;
  }

  [SerializeField] private Component[] _components;

  private WorldGenerator _worldGen;

  private void Start()
  {
    _worldGen = GameObject.Find("Holder").GetComponent<WorldGenerator>();
    for (int i = 0; i < _components.Length; i++)
    {
      _components[i].transform.position = new Vector3(_components[i].transform.position.x, _worldGen.GetHeightValue(new Vector2(_components[i].transform.position.x, _components[i].transform.position.z)), _components[i].transform.position.z);
      if (_components[i].alignToNormal) {
        Vector3 pointA = new Vector3(_components[i].transform.position.x + 1, _worldGen.GetHeightValue(new Vector2(_components[i].transform.position.x + 1, _components[i].transform.position.z)), _components[i].transform.position.z);
        Vector3 pointB = new Vector3(_components[i].transform.position.x - 1, _worldGen.GetHeightValue(new Vector2(_components[i].transform.position.x - 1, _components[i].transform.position.z)), _components[i].transform.position.z);
        Vector3 pointC = new Vector3(_components[i].transform.position.x, _worldGen.GetHeightValue(new Vector2(_components[i].transform.position.x, _components[i].transform.position.z + 1)), _components[i].transform.position.z + 1);
        Vector3 pointD = new Vector3(_components[i].transform.position.x, _worldGen.GetHeightValue(new Vector2(_components[i].transform.position.x, _components[i].transform.position.z - 1)), _components[i].transform.position.z - 1);
        Vector3 normal = Vector3.Cross(pointA - pointB, pointC - pointD).normalized;
        _components[i].transform.rotation = Quaternion.LookRotation(normal) * _components[i].transform.localRotation * Quaternion.Euler(0, 0, -90);
      }
    }
  }
}