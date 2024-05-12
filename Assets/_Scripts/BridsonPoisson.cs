using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;

public class BridsonPoisson : MonoBehaviour
{
  public GameObject instance;

  [SerializeField] private int _rn = 500;
  [SerializeField] private int _r = 30;
  [SerializeField] private int _k = 30;
  [SerializeField] private int _n = 2;
  [SerializeField] private int _pts = 100;
  private float _cellSize;

  private int[,] _grid;
  private List<Vector2Int> _activeList;

  private void Start()
  {
    _activeList = new List<Vector2Int>();
    var locations = GeneratePoints();

    foreach (Vector2Int p in locations)
    {
      Instantiate(instance, new Vector3(p.x, 0, p.y), Quaternion.identity);
    }
  }

  List<Vector2Int> GeneratePoints()
  {
    _cellSize = _r / math.sqrt(_n);
    _grid = new int[2 * _rn, 2 * _rn];
    for (int i = 0; i < 2 * _rn; i++)
    {
      for (int j = 0; j < 2 * _rn; j++)
      {
        _grid[i, j] = -1;
      }
    }
    Vector2Int xNaught = new Vector2Int(Random.Range(0, 2 * _rn), Random.Range(0, 2 * _rn));
    _grid[xNaught.x, xNaught.y] = 0;
    _activeList.Add(xNaught);

    for (int iter = 0; iter < (2 * _pts - 1); iter++)
    {
      int i = Random.Range(0, _activeList.Count);
      Debug.Log(i);
      Vector2Int xI = _activeList[i];
      List<Vector2Int> samples = new List<Vector2Int>();
      for (int jter = 0; jter < _k; jter++)
      {
        float angle = Random.value * 2 * (float)Math.PI;
        float distance = Random.Range(_r, 2 * _r);
        float x = distance * (float)Math.Cos(angle) + xI.x;
        float y = distance * (float)Math.Sin(angle) + xI.y;

        if (x < 0 || x >= 2 * _rn || y < 0 || y >= 2 * _rn)
        {
          continue;
        }
        samples.Add(new Vector2Int((int)x, (int)y));
      }

      int startCount = _activeList.Count;
      foreach (Vector2Int sample in samples)
      {
        Vector2Int ul = new Vector2Int(sample.x - _r, sample.y + _r);
        Vector2Int dr = new Vector2Int(sample.x + _r, sample.y - _r);
        bool invalid = false;

        for (int yter = ul.y; yter < dr.y; yter++)
        {
          for (int xter = ul.x; xter < dr.x; xter++)
          {
            if (_grid[xter, yter] != -1)
            {
              invalid = true;
            }
          }
        }
        if (!invalid)
        {
          _activeList.Add(sample);
          _grid[sample.x, sample.y] = _activeList.Count;
        }
        if (startCount == _activeList.Count)
        {
          _activeList.RemoveAt(i);
        }
      }
    }
    return _activeList;
  }
}