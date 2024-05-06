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
  private List<Vector2> _activeList;

  private void Start() {
    GeneratePoint();
  }

  void GeneratePoint() {
    _cellSize = _r / math.sqrt(_n);
    _grid = new int[_rn, _rn];
    Vector2Int xNaught = new Vector2Int(Random.Range(-_rn, _rn), Random.Range(-_rn, _rn));
    _grid[xNaught.x, xNaught.y] = 1;
  }
}