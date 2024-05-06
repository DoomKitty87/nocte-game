using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
  [Header("References")]
  private WorldGenerator _worldGenerator = WorldGenInfo._worldGenerator;
  [Header("Settings")]
  [SerializeField] private int _perChunk;
  [Header("Output")]
  public List<Vector3> _foodLocations;
  
  public void PlaceFoodPoints() {
    // 1: For each chunk pick _perChunk points
    // Check each point to make sure they are not in the water with _worldGenerator.GetWater();
    // if they are pick a new one
    // after all points are valid get mesh height with GetHeightValue and add to list at that height
    
    float tileSize = _worldGenerator.GetTileSize();
    
    foreach (WorldGenerator.WorldTile chunk in _worldGenerator.GetWorldTiles()) {
      Vector3 chunkPos = chunk.obj.transform.position;
      for (int i = 0; i < _perChunk; i++) {
        Vector2 pointPos = new Vector2(Random.Range(-tileSize, tileSize), Random.Range(-tileSize, tileSize));
        // take it away Harper
      }
    }
  }
}