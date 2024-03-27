using UnityEngine;


namespace Foliage
{
  [CreateAssetMenu(fileName = "Foliage Name", menuName = "ScriptableObjects/FoliageObject")]
  public class FoliageScriptable : ScriptableObject
  {
    public bool _useSubmesh;
    public Material Material;
    public Material Material2;
    public Material BillboardMaterial;
    public Mesh BillboardMesh;
    public bool UseBillboard;
    public GameObject ColliderPrefab;
    public FoliageLODData[] _lodRanges;
    public int _maxBillboardDistance;
    public ComputeShader _positionComputeShader;
    public ComputeShader _cullingComputeShader;
  }


  [System.Serializable]
  public class FoliageLODData
  {
    public Mesh Mesh;
    public int Distance;
    public int Density;
  }

}