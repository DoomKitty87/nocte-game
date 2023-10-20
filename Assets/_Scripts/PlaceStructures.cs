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

  private int[] _beamWindingOrder = {
    0, 2, 1, 2, 3, 1, // Bottom Face
    4, 6, 5, 4, 7, 5, // Top Face
    0, 4, 1, 4, 5, 1, // Front Face
    1, 5, 3, 5, 7, 3, // Right Face
    3, 7, 2, 7, 6, 2, // Back Face
    2, 6, 0, 6, 4, 0 // Left Face
  }

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
      // go = Instantiate(_outerStructures[nodeChoice], outPosition, Quaternion.FromToRotation(Vector3.up, normal));
      go = Instantiate(_outerStructures[nodeChoice], outPosition, Quaternion.identity);
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
      List<Vector3> vertices = new List<Vector3>();
      List<int> triangles = new List<int>();
      if (heighte < outPosition.y) {
        for (int i = 0; i < _beamWindingOrder.Length; i++) {
          triangles.Add(_beamWindingOrder[i] + vertices.Length);
        }
        vertices.AddRange(GeneratePillar(new Vector3(outPosition.x + bounds.x / 2, heighte, outPosition.x + bounds.z / 2), new Vector3(outPosition.x + bounds.x / 2, outPosition.y, outPosition.x + bounds.z / 2)));
      }

      if (heightf < outPosition.y) {
                for (int i = 0; i < _beamWindingOrder.Length; i++) {
          triangles.Add(_beamWindingOrder[i] + vertices.Length);
        }
        vertices.AddRange(GeneratePillar(new Vector3(outPosition.x - bounds.x / 2, heightf, outPosition.x + bounds.z / 2), new Vector3(outPosition.x - bounds.x / 2, outPosition.y, outPosition.x + bounds.z / 2)));
      }

      if (heightg < outPosition.y) {
                for (int i = 0; i < _beamWindingOrder.Length; i++) {
          triangles.Add(_beamWindingOrder[i] + vertices.Length);
        }
        vertices.AddRange(GeneratePillar(new Vector3(outPosition.x - bounds.x / 2, heightg, outPosition.x - bounds.z / 2), new Vector3(outPosition.x - bounds.x / 2, outPosition.y, outPosition.x - bounds.z / 2)));
      }

      if (heighth < outPosition.y) {
                for (int i = 0; i < _beamWindingOrder.Length; i++) {
          triangles.Add(_beamWindingOrder[i] + vertices.Length);
        }
        vertices.AddRange(GeneratePillar(new Vector3(outPosition.x + bounds.x / 2, heighth, outPosition.x - bounds.z / 2), new Vector3(outPosition.x + bounds.x / 2, outPosition.y, outPosition.x - bounds.z / 2)));
      }
      
      msh.mesh = new Mesh():
      msh.mesh.vertices = vertices.ToArray();
      msh.mesh.triangles = triangles.ToArray();
      supportBeams.AddComponent<MeshCollider>():
      go.transform.parent = transform;
    }
  }

  private Vector3[] GeneratePillar(Vector3 vertexA, Vector3 vertexB) {
    Vector3[] vertices = new Vector3[8];
    int[] triangles = new int[36];

    vertices[0] = new Vector3(vertexA.x - _beamWidth / 2, vertexA.y, vertexA.z - _beamWidth / 2);
    vertices[1] = new Vector3(vertexA.x + _beamWidth / 2, vertexA.y, vertexA.z - _beamWidth / 2);
    vertices[2] = new Vector3(vertexA.x - _beamWidth / 2, vertexA.y, vertexA.z + _beamWidth / 2);
    vertices[3] = new Vector3(vertexA.x + _beamWidth / 2, vertexA.y, vertexA.z + _beamWidth / 2);

    vertices[4] = new Vector3(vertexB.x - _beamWidth / 2, vertexB.y, vertexB.z - _beamWidth / 2);
    vertices[5] = new Vector3(vertexB.x + _beamWidth / 2, vertexB.y, vertexB.z - _beamWidth / 2);
    vertices[6] = new Vector3(vertexB.x - _beamWidth / 2, vertexB.y, vertexB.z + _beamWidth / 2);
    vertices[7] = new Vector3(vertexB.x + _beamWidth / 2, vertexB.y, vertexB.z + _beamWidth / 2);

    return vertices;
  }

}