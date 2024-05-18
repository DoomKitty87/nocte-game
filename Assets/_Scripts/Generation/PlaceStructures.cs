using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class PlaceStructures : MonoBehaviour
{

  [SerializeField] private WorldGenerator _worldGen;
  [SerializeField] private GameObject _centralStructure;
  [SerializeField] private GameObject[] _outerStructures;
  [SerializeField] private float _nodeDistance;
  [SerializeField] private float _beamWidth;
  [SerializeField] private Material _beamMaterial;
  [SerializeField] private float _groundInset;
  [SerializeField] private int _structureRenderDistance = 2500; // [World Units]
  [SerializeField] private float _centerOffsetRadiusAmplitude;
  [SerializeField] private float _normalCutoff;
  [SerializeField] private float _normalRadius;
  [SerializeField] private float _roadResolution;
  [SerializeField] private float _roadWidth;
  [SerializeField] private float _roadDepth;
  [SerializeField] private float _roadInset;
  [SerializeField] private float _roadBevel;
  [SerializeField] private float _roadWaterImpact;
  [SerializeField] private float _roadNoiseAmplitude;
  [SerializeField] private int _roadPathfindingPoints;
  [SerializeField] private float _roadMaxAngle;
  [SerializeField] private Material _roadMaterial;

  [SerializeField] private float _heightCutoff;

  private Vector3[] _structurePositions;
  private List<Transform> _structures = new List<Transform>();

  public static Vector2 CentralPosition;

  private int[] _beamWindingOrder = {
    1, 2, 0, 1, 3, 2, // Bottom Face
    4, 6, 5, 6, 7, 5, // Top Face
    0, 4, 1, 4, 5, 1, // Front Face
    1, 5, 3, 5, 7, 3, // Right Face
    3, 7, 2, 7, 6, 2, // Back Face
    2, 6, 0, 6, 4, 0 // Left Face
  };

  public static PlaceStructures Instance { get; private set; }

  private void Awake() {
    if (Instance == null) Instance = this;
  }

  private void OnDisable() {
    if (Instance == this) Instance = null;
  }

  public float GetNearestSite(Vector3 position) {
    float minDistance = float.MaxValue;
    for (int i = 1; i < _structurePositions.Length; i++) {
      float distance = Vector3.Distance(position, _structurePositions[i]);
      if (distance < minDistance) {
        minDistance = distance;
      }
    }
    return minDistance;
  }

  private void Start() {
    Random.InitState(_worldGen.Seed);
    float offsetAngle = Random.value * Mathf.PI * 2;
    Vector3 positionOffset = new Vector3(Mathf.Cos(offsetAngle), 0, Mathf.Sin(offsetAngle)) * Random.value * _centerOffsetRadiusAmplitude;
    _structurePositions = new Vector3[_outerStructures.Length + 1];
    Vector3 mainPosition = positionOffset;
    mainPosition.y = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z));
    float pointA = _worldGen.GetHeightValue(new Vector2(mainPosition.x - _normalRadius, mainPosition.z));
    float pointB = _worldGen.GetHeightValue(new Vector2(mainPosition.x + _normalRadius, mainPosition.z));
    float pointC = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z - _normalRadius));
    float pointD = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z + _normalRadius));
    Vector3 normal = -Vector3.Cross(new Vector3(1, pointB, 0) - new Vector3(-1, pointA, 0),
      new Vector3(0, pointD, 1) - new Vector3(0, pointC, -1)).normalized;
    int s = 1;
    while (normal.y < _normalCutoff || _worldGen.GetWaterSaturation(new Vector2(mainPosition.x, mainPosition.z)) || _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z)) > _heightCutoff) {
      if (s > 10000) {
        Debug.LogError("Could not find a valid central position");
        break;
      }
      offsetAngle = Random.value * Mathf.PI * 2;
      // Debug.Log(offsetAngle);
      positionOffset = new Vector3(Mathf.Cos(offsetAngle), 0, Mathf.Sin(offsetAngle)) * Random.value * _centerOffsetRadiusAmplitude;
      mainPosition = positionOffset;
      mainPosition.y = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z));
      pointA = _worldGen.GetHeightValue(new Vector2(mainPosition.x - _normalRadius, mainPosition.z));
      pointB = _worldGen.GetHeightValue(new Vector2(mainPosition.x + _normalRadius, mainPosition.z));
      pointC = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z - _normalRadius));
      pointD = _worldGen.GetHeightValue(new Vector2(mainPosition.x, mainPosition.z + _normalRadius));
      normal = -Vector3.Cross(new Vector3(1, pointB, 0) - new Vector3(-1, pointA, 0),
        new Vector3(0, pointD, 1) - new Vector3(0, pointC, -1)).normalized;
      s++;
    }
    // GameObject go = Instantiate(_centralStructure, mainPosition, Quaternion.FromToRotation(Vector3.up, normal));
    CentralPosition = new Vector2(mainPosition.x, mainPosition.z);
    GameObject go = Instantiate(_centralStructure, mainPosition, Quaternion.identity);
    _structurePositions[0] = mainPosition;
    Vector2 bounds;
    if (go.GetComponent<MeshFilter>()) bounds = go.GetComponent<MeshFilter>().mesh.bounds.size;
    else bounds = new Vector2(50, 50);
    float heighte = _worldGen.GetHeightValue(new Vector2(mainPosition.x + bounds.x / 2, mainPosition.z + bounds.y / 2));
    float heightf = _worldGen.GetHeightValue(new Vector2(mainPosition.x - bounds.x / 2, mainPosition.z + bounds.y / 2));
    float heightg = _worldGen.GetHeightValue(new Vector2(mainPosition.x - bounds.x / 2, mainPosition.z - bounds.y / 2));
    float heighth = _worldGen.GetHeightValue(new Vector2(mainPosition.x + bounds.x / 2, mainPosition.z - bounds.y / 2));
    float maxHeight = mainPosition.y;
    if (heighte > maxHeight) maxHeight = heighte;
    if (heightf > maxHeight) maxHeight = heightf;
    if (heightg > maxHeight) maxHeight = heightg;
    if (heighth > maxHeight) maxHeight = heighth;
    go.transform.position = new Vector3(go.transform.position.x, maxHeight, go.transform.position.z);
    go.transform.parent = transform;
    _structures.Add(go.transform);
    mainPosition = go.transform.position;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    // if (heighte < mainPosition.y) {
    //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
    //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
    //   }
    //   vertices.AddRange(GeneratePillar(new Vector3(mainPosition.x + bounds.x / 2, heighte - _groundInset, mainPosition.z + bounds.y / 2), new Vector3(mainPosition.x + bounds.x / 2, mainPosition.y, mainPosition.z + bounds.y / 2)));
    // }
    //
    // if (heightf < mainPosition.y) {
    //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
    //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
    //   }
    //   vertices.AddRange(GeneratePillar(new Vector3(mainPosition.x - bounds.x / 2, heightf - _groundInset, mainPosition.z + bounds.y / 2), new Vector3(mainPosition.x - bounds.x / 2, mainPosition.y, mainPosition.z + bounds.y / 2)));
    // }
    //
    // if (heightg < mainPosition.y) {
    //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
    //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
    //   }
    //   vertices.AddRange(GeneratePillar(new Vector3(mainPosition.x - bounds.x / 2, heightg - _groundInset, mainPosition.z - bounds.y / 2), new Vector3(mainPosition.x - bounds.x / 2, mainPosition.y, mainPosition.z - bounds.y / 2)));
    // }
    //
    // if (heighth < mainPosition.y) {
    //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
    //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
    //   }
    //   vertices.AddRange(GeneratePillar(new Vector3(mainPosition.x + bounds.x / 2, heighth - _groundInset, mainPosition.z - bounds.y / 2), new Vector3(mainPosition.x + bounds.x / 2, mainPosition.y, mainPosition.z - bounds.y / 2)));
    // }

    // if (vertices.Count > 0) {
    //   GameObject supportBeams = new GameObject();
    //   MeshRenderer mr = supportBeams.AddComponent<MeshRenderer>();
    //   mr.material = _beamMaterial;
    //   MeshFilter msh = supportBeams.AddComponent<MeshFilter>();
    //   msh.mesh = new Mesh();
    //   msh.mesh.vertices = vertices.ToArray();
    //   msh.mesh.triangles = triangles.ToArray();
    //   supportBeams.AddComponent<MeshCollider>();
    //   supportBeams.transform.parent = go.transform;
    // }
    float[] rotations = new float[_outerStructures.Length];
    for (int i = 0; i < _outerStructures.Length; i++) {
      float nodeRotation = Random.value * Mathf.PI * 2;
      for (int j = 0; j < i; j++) {
        if (Mathf.Abs(nodeRotation - rotations[j]) < Mathf.PI / 4) nodeRotation += Mathf.PI / 2;
      }
      rotations[i] = nodeRotation;
      float nodeRadius = _nodeDistance * (Random.value + 0.5f);
      Vector3 outPosition = new Vector3(nodeRadius * Mathf.Cos(nodeRotation), 0, nodeRadius * Mathf.Sin(nodeRotation)) + positionOffset;
      outPosition.y = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z));
      s = 1;
      float heighta = _worldGen.GetHeightValue(new Vector2(outPosition.x - _normalRadius, outPosition.z));
      float heightb = _worldGen.GetHeightValue(new Vector2(outPosition.x + _normalRadius, outPosition.z));
      float heightc = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z - _normalRadius));
      float heightd = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z + _normalRadius));
      normal = -Vector3.Cross(new Vector3(1, heightb, 0) - new Vector3(-1, heighta, 0),
        new Vector3(0, heightd, 1) - new Vector3(0, heightc, -1)).normalized;
      float minDist = float.MaxValue;

      for (int j = 0; j < _structurePositions.Length; j++) {
        float dist = Vector3.Distance(outPosition, _structurePositions[j]);
        if (dist < minDist) minDist = dist;
      }

      while (normal.y < _normalCutoff || _worldGen.GetWaterSaturation(new Vector2(outPosition.x, outPosition.z)) || _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z)) > _heightCutoff || minDist < 500) {
        if (s > 10000) {
          Debug.LogError("Could not find a valid outer position");
          break;
        }
        nodeRotation = Random.value * Mathf.PI * 2;
        // Debug.Log(nodeRotation);
        nodeRadius = _nodeDistance * (Random.value + 0.5f);
        outPosition = new Vector3(nodeRadius * Mathf.Cos(nodeRotation), 0, nodeRadius * Mathf.Sin(nodeRotation)) + positionOffset;
        outPosition.y = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z));
        heighta = _worldGen.GetHeightValue(new Vector2(outPosition.x - _normalRadius, outPosition.z));
        heightb = _worldGen.GetHeightValue(new Vector2(outPosition.x + _normalRadius, outPosition.z));
        heightc = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z - _normalRadius));
        heightd = _worldGen.GetHeightValue(new Vector2(outPosition.x, outPosition.z + _normalRadius));
        normal = -Vector3.Cross(new Vector3(1, heightb, 0) - new Vector3(-1, heighta, 0),
          new Vector3(0, heightd, 1) - new Vector3(0, heightc, -1)).normalized;

        minDist = float.MaxValue;
        for (int j = 0; j < _structurePositions.Length; j++) {
          float dist = Vector3.Distance(outPosition, _structurePositions[j]);
          if (dist < minDist) minDist = dist;
        }
        s++;
      }

      // Vector2 mainPosition2 = new Vector2(mainPosition.x, mainPosition.z);
      // Vector2 outPosition2 = new Vector2(outPosition.x, outPosition.z);
      // int roadPoints = Mathf.FloorToInt(Vector2.Distance(mainPosition2, outPosition2) / _roadResolution);
      // Vector2 val0 = mainPosition2 + (outPosition2 - mainPosition2).normalized * 25;
      // Vector2 val1 = outPosition2 + (mainPosition2 - outPosition2).normalized * 25;

      // Vector2[] roadPath = RoadGenerator.GenerateRoadPath(val0, val1, roadPoints, _roadPathfindingPoints, _roadMaxAngle);

      // Vector2[] roadPlane =
      //   RoadGenerator.PlanePointsFromLine(roadPath, _roadWidth, _roadNoiseAmplitude);
      // Vector3[] roadPlane3 = new Vector3[roadPlane.Length];
      // Color[] roadVertexColors = new Color[roadPlane.Length * 2];
      // for (int j = 0; j < roadPlane.Length; j++) {
      //   roadPlane3[j] = new Vector3(roadPlane[j].x, _worldGen.GetHeightValue(roadPlane[j]), roadPlane[j].y) + Vector3.up * _roadWaterImpact * _worldGen.GetRiverValue(roadPlane[j]);
      //   if (roadPlane3[j].y < WorldGenInfo._lakePlaneHeight) roadPlane3[j].y = WorldGenInfo._lakePlaneHeight;
      //   roadVertexColors[j] = new Color(roadPlane3[j].y == WorldGenInfo._lakePlaneHeight ? 1 : _worldGen.GetRiverValue(roadPlane[j]), 0, 0, 0);
      //   roadVertexColors[j + roadPlane.Length] = new Color(roadPlane3[j].y == WorldGenInfo._lakePlaneHeight ? 1 : _worldGen.GetRiverValue(roadPlane[j]), 0, 0, 0);
      // }

      // Mesh road = RoadGenerator.MeshFromPlane(roadPlane3, _roadDepth, _roadInset, _roadBevel);
      // road.colors = roadVertexColors;
      // GameObject obj = new GameObject();
      // obj.layer = 6;
      // obj.AddComponent<MeshFilter>().mesh = road;
      // obj.AddComponent<MeshRenderer>().material = _roadMaterial;
      // obj.AddComponent<MeshCollider>();
      // obj.name = "RoadSegment";
      // obj.transform.parent = transform;
      // go = Instantiate(_outerStructures[nodeChoice], outPosition, Quaternion.FromToRotation(Vector3.up, normal));
      go = Instantiate(_outerStructures[i], outPosition, Quaternion.identity);
      _structurePositions[i + 1] = outPosition;
      if (go.GetComponent<MeshFilter>()) bounds = go.GetComponent<MeshFilter>().mesh.bounds.size;
      else bounds = new Vector2(50, 50);
      heighte = _worldGen.GetHeightValue(new Vector2(outPosition.x + bounds.x / 2, outPosition.z + bounds.y / 2));
      heightf = _worldGen.GetHeightValue(new Vector2(outPosition.x - bounds.x / 2, outPosition.z + bounds.y / 2));
      heightg = _worldGen.GetHeightValue(new Vector2(outPosition.x - bounds.x / 2, outPosition.z - bounds.y / 2));
      heighth = _worldGen.GetHeightValue(new Vector2(outPosition.x + bounds.x / 2, outPosition.z - bounds.y / 2));
      maxHeight = outPosition.y;
      if (heighte > maxHeight) maxHeight = heighte;
      if (heightf > maxHeight) maxHeight = heightf;
      if (heightg > maxHeight) maxHeight = heightg;
      if (heighth > maxHeight) maxHeight = heighth;
      go.transform.position = new Vector3(go.transform.position.x, maxHeight, go.transform.position.z);
      outPosition = go.transform.position;
      // Place support beams
      vertices = new List<Vector3>();
      triangles = new List<int>();

      // if (heighte < outPosition.y) {
      //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
      //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
      //   }
      //   vertices.AddRange(GeneratePillar(new Vector3(outPosition.x + bounds.x / 2, heighte - _groundInset, outPosition.z + bounds.y / 2), new Vector3(outPosition.x + bounds.x / 2, outPosition.y, outPosition.z + bounds.y / 2)));
      // }
      //
      // if (heightf < outPosition.y) {
      //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
      //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
      //   }
      //   vertices.AddRange(GeneratePillar(new Vector3(outPosition.x - bounds.x / 2, heightf - _groundInset, outPosition.z + bounds.y / 2), new Vector3(outPosition.x - bounds.x / 2, outPosition.y, outPosition.z + bounds.y / 2)));
      // }
      //
      // if (heightg < outPosition.y) {
      //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
      //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
      //   }
      //   vertices.AddRange(GeneratePillar(new Vector3(outPosition.x - bounds.x / 2, heightg - _groundInset, outPosition.z - bounds.y / 2), new Vector3(outPosition.x - bounds.x / 2, outPosition.y, outPosition.z - bounds.y / 2)));
      // }
      //
      // if (heighth < outPosition.y) {
      //   for (int j = 0; j < _beamWindingOrder.Length; j++) {
      //     triangles.Add(_beamWindingOrder[j] + vertices.Count);
      //   }
      //   vertices.AddRange(GeneratePillar(new Vector3(outPosition.x + bounds.x / 2, heighth - _groundInset, outPosition.z - bounds.y / 2), new Vector3(outPosition.x + bounds.x / 2, outPosition.y, outPosition.z - bounds.y / 2)));
      // }

      // if (vertices.Count > 0) {
      //   GameObject supportBeams = new GameObject();
      //   MeshRenderer mr = supportBeams.AddComponent<MeshRenderer>();
      //   mr.material = _beamMaterial;
      //   MeshFilter msh = supportBeams.AddComponent<MeshFilter>();
      //   msh.mesh = new Mesh();
      //   msh.mesh.vertices = vertices.ToArray();
      //   msh.mesh.triangles = triangles.ToArray();
      //   supportBeams.AddComponent<MeshCollider>();
      //   supportBeams.transform.parent = go.transform;
      // }

      go.transform.parent = transform;
      _structures.Add(go.transform);
    }
  
    //for (int i = 0; i < _structurePositions.Length; i++) {
    //  for (int j = i + 1; j < _structurePositions.Length; j++) {
    //    GameObject obj = Instantiate(_roadObject, Vector3.zero, Quaternion.identity);
    //    obj.GetComponent<RoadHandler>().SetRoadPositions(_structurePositions[i], _structurePositions[j]);
    //  }
    //}
  }

  private Vector3[] GeneratePillar(Vector3 vertexA, Vector3 vertexB) {
    Vector3[] vertices = new Vector3[8];

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

  public void CheckStructures(Vector2 playerPosition) {
    for (int i = 0; i < _structures.Count; i++) {
      if ((new Vector2(_structures[i].position.x, _structures[i].position.z) - playerPosition).magnitude > _structureRenderDistance) _structures[i].gameObject.SetActive(false);
      else _structures[i].gameObject.SetActive(true);
    }
  }

}