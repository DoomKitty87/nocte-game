using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
  private WorldGenerator _worldGenerator = WorldGenInfo._worldGenerator;
  [SerializeField] private int _chunkDensity;
  public List<Vector3> _foodLocations;

  public void PlaceFoodPoints() {
    // for each chunk pick like 5 points
    // check each point to make sure they are not in the water
    // if they are pick a new one
    // after all points are valid get mesh height and instantiate at that height
    foreach (var chunk in _chunkPool) {
      x1 = chunk._chunkCenter + 
    }
  }
}