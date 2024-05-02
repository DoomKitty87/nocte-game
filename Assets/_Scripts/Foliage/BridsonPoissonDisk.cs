// using UnityEngine;
// using System.Collections.Generic;
//
// int rn = 50;
// float r = 3f;
// int k = 30;
// int n = 2;
// (int, int)[,] grid = new (int, int)[rn, rn];
// List<(int, int)> activeList = new List<(int, int)>();
// float cellSize = r / Mathf.Sqrt(n);
//
// (int, int) xNaught = (Random.Range(-rn, rn), Random.Range(-rn, rn));
// grid[0,0] = xNaught;
// activeList.Insert(0, xNaught);
//
// activeList[Random.value * activeList.Count]