using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BridsonPoisson : MonoBehaviour
{
  private readonly int _rn = 200;
  private readonly int _r = 5;
  private readonly int _k = 30;
  private readonly int _n = 2;
  private readonly int _pts = 10000;
  private float _cellSize;

  private int[,] _grid;
  private List<Vector2Int> _activeList;

  private void Start() {
    GeneratePoint();
  }

  void GeneratePoint() {
    _cellSize = _r / math.sqrt(_n);
    _grid = new int[_rn, _rn];
    Vector2Int xNaught = new Vector2Int(Random.Range(-_rn, _rn), Random.Range(-_rn, _rn));
    _grid[xNaught.x, xNaught.y] = 1;
    _activeList.Add(xNaught);

    for (int iter = 0; iter < (2 * _pts - 1); iter++) {
      int i = Random.Range(-_activeList.Count, _activeList.Count);
      Vector2Int xI = _activeList[i];
      List<Vector2Int> samples = new List<Vector2Int>();
      for (int jter = 0; jter < _k; jter++) {
        float angle = Random.value * 2 * (float)Math.PI;
        float distance = Random.Range(_r, 2 * _r);
        float x = distance * (float)Math.Cos(angle) + xI.x;
        float y = distance * (float)Math.Sin(angle) + xI.y;
        samples.Add(new Vector2Int((int)x, (int)y));
      }

      foreach (Vector2Int sample in samples) {
        // i want to verify there are no "1"s in the 
        // float distance = Math.Sqrt(()**2 + ()**2)
      } 
    }
  }
}