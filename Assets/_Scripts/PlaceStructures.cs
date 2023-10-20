using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlaceStructures : MonoBehaviour
{

  [SerializeField] private WorldGenerator _worldGen;
  [SerializeField] private GameObject _centralStructure;
  [SerializeField] private GameObject[] _outerStructures;
  [SerializeField] private int _nodeCount;
  [SerializeField] private float _nodeDistance;
  [SerializeField] private float _beamWidth;

  private void Start() {
    Vector3 mainPosition = new Vector3(0, 0, 0);
    mainPosition.y = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z));
    float heighta = _worldGen.GetHeightValue(new Vector2(mainPosition.x - 1, mainPosition.z));
    float heightb = _worldGen.GetHeightValue(new Vector2(mainPosition.x + 1, mainPosition.z));
    float heightc = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z - 1));
    float heightd = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z + 1));
    Vector3 normal = -Vector3.Cross(new Vector3(1, heightb, 0) - new Vector3(-1, heighta, 0),
      new Vector3(0, heightd, 1) - new Vector3(0, heightc, -1)).normalized;
    GameObject go = Instantiate(_centralStructure, mainPosition, Quaternion.FromToRotation(Vector3.up, normal));
    Vector2 bounds;
    if (go.GetComponent<MeshFilter>()) bounds = go.GetComponent<MeshFilter>().mesh.bounds.size;
    else bounds = new Vector2(10, 10);
    float heighte = _worldGen.GetHeightValue(new Vector2(mainPosition.x + bounds.x / 2, mainPosition.z + bounds.y / 2));
    float heightf = _worldGen.GetHeightValue(new Vector2(mainPosition.x - bounds.x / 2, mainPosition.z + bounds.y / 2));
    float heightg = _worldGen.GetHeightValue(new Vector2(mainPosition.x - bounds.x / 2, mainPosition.z - bounds.y / 2));
    float heighth = _worldGen.GetHeightValue(new Vector2(mainPosition.x + bounds.x / 2, mainPosition.z - bounds.y / 2));
    float minHeight = mainPosition.y;
    if (heighte < minHeight) minHeight = heighte;
    if (heightf < minHeight) minHeight = heightf;
    if (heightg < minHeight) minHeight = heightg;
    if (heighth < minHeight) minHeight = heighth;
    go.transform.position = new Vector3(go.transform.position.x, minHeight, go.transform.position.z);
    go.transform.parent = transform;

    for (int i = 0; i < _nodeCount; i++) {
      float nodeRotation = (_worldGen.GetSeedHash() * (i + 1)) % 1000 / 1000 * 2 * Mathf.PI;
      Vector3 outPosition = new Vector3(_nodeDistance * Mathf.Cos(nodeRotation), 0, _nodeDistance * Mathf.Sin(nodeRotation));
      int nodeChoice = Mathf.FloorToInt(_worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z)) % 1 * _outerStructures.Length);
      outPosition.y = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z));
      heighta = _worldGen.GetHeightValue(new Vector2(outPosition.x - 1, outPosition.z));
      heightb = _worldGen.GetHeightValue(new Vector2(outPosition.x + 1, outPosition.z));
      heightc = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z - 1));
      heightd = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z + 1));
      normal = -Vector3.Cross(new Vector3(1, heightb, 0) - new Vector3(-1, heighta, 0),
        new Vector3(0, heightd, 1) - new Vector3(0, heightc, -1)).normalized;
      go = Instantiate(_outerStructures[nodeChoice], outPosition, Quaternion.FromToRotation(Vector3.up, normal));
      if (go.GetComponent<MeshFilter>()) bounds = go.GetComponent<MeshFilter>().mesh.bounds.size;
      else bounds = new Vector2(10, 10);
      heighte = _worldGen.GetHeightValue(new Vector2(outPosition.x + bounds.x / 2, outPosition.z + bounds.y / 2));
      heightf = _worldGen.GetHeightValue(new Vector2(outPosition.x - bounds.x / 2, outPosition.z + bounds.y / 2));
      heightg = _worldGen.GetHeightValue(new Vector2(outPosition.x - bounds.x / 2, outPosition.z - bounds.y / 2));
      heighth = _worldGen.GetHeightValue(new Vector2(outPosition.x + bounds.x / 2, outPosition.z - bounds.y / 2));
      minHeight = outPosition.y;
      if (heighte < minHeight) minHeight = heighte;
      if (heightf < minHeight) minHeight = heightf;
      if (heightg < minHeight) minHeight = heightg;
      if (heighth < minHeight) minHeight = heighth;
      // If adjusting by minHeight (ends up clipping) go.transform.position = new Vector3(go.transform.position.x, minHeight, go.transform.position.z);
      // Place support beams
      GameObject supportBeams = new GameObject();
      supportBeams.AddComponent<MeshRenderer>();
      MeshFilter msh = supportBeams.AddComponent<MeshFilter>();
      // This is rudimentary rework with more procedural version
      if (heighte < outPosition.y) {
        Vector3 vertexA = new Vector3(outPosition.x + bounds.x / 2 + _beamWidth / 2, heighte, outPosition.x + bounds.y / 2 + _beamWidth / 2);
      }

      if (heightf < outPosition.y) {
        
      }

      if (heightg < outPosition.y) {
        
      }

      if (heighth < outPosition.y) {
        
      }
      go.transform.parent = transform;
    }
  }

}