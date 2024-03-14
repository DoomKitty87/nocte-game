using UnityEngine;


namespace Foliage
{
  [CreateAssetMenu(fileName = "Foliage Name", menuName = "ScriptableObjects/FoliageObject")]
  public class FoliageScriptable : ScriptableObject
  {
    public FoliageType type;
    public Material Material;
    public FoliageLODData[] _lodRanges;
    public ComputeShader _positionComputeShader;
  }


  [System.Serializable]
  public class FoliageLODData
  {
    public Mesh Mesh;
    public int Distance;
    public int Density;
  }


  public enum FoliageType
  {
    PineTree,
    OakTree,
    Bush
  }
}