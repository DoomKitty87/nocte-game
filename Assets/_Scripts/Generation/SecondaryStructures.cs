using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecondaryStructures : MonoBehaviour
{

  public struct StructureData
  {
    public Vector2 position;
    public GameObject reference;
    public int type;
  }

  [SerializeField] private WorldGenerator _worldGen;
  [SerializeField] private int _structureRenderDistance = 1500; // [World Units]
  [SerializeField] private float _chunkStructureChance = 0.2f;
  [SerializeField] private GameObject[] _availableStructures;

  private List<StructureData> _structures = new List<StructureData>();

  public void GenerateChunkStructures(Vector2 corner0, Vector2 corner1) {
    Vector2 position = new Vector2((corner1.x - corner0.x) * PSRHash(corner0), (corner1.y - corner0.y) * PSRHash(corner1)) + corner0;
    if (PSRHash(position) < _chunkStructureChance) {
      GenerateStructure(position, 0);
    }
  }

  private void GenerateStructure(Vector2 position, int type) {
    StructureData data = new StructureData();
    data.position = position;
    // Debug.Log(Mathf.CeilToInt(PSRHash(position * 32.15f) * _availableStructures.Length) - 1);
    data.reference = Instantiate(_availableStructures[Mathf.CeilToInt(PSRHash(position * 32.15f) * _availableStructures.Length) - 1], Vector3.zero, Quaternion.identity, transform);
    data.reference.transform.position = new Vector3(position.x, _worldGen.GetHeightValue(position), position.y);
    data.type = type;
    _structures.Add(data);
  }

  public void CheckStructures(Vector2 playerPosition) {
    foreach (StructureData structure in _structures) {
      if (Mathf.Max(Mathf.Abs(playerPosition.x - structure.position.x), Mathf.Abs(playerPosition.y - structure.position.y)) > _structureRenderDistance) {
        structure.reference.SetActive(false);
      } else {
        structure.reference.SetActive(true);
      }
    }
  }

  private float PSRHash(Vector2 position) {
    return Mathf.Clamp(Mathf.PerlinNoise(position.x * 52.341f + _worldGen._seed % 1000, position.y * 26.758f + _worldGen._seed % 1000), 0.0001f, 1);
  }

}