using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject for each foliage type.
///
/// Should NOT be used outside of InstanceObjects script.
/// Instead use FoliageData as it contains this object through FoliageType and GetFoliageData.
/// 
/// FoliageType is used for easily choosing a type of foliage from outside scripts,
/// automatically set in InstanceObject InitializeDictionary
///
/// Public fetch function to return current mesh and material at given LOD level
/// </summary>
[CreateAssetMenu(fileName = "FoliageMetaData", menuName = "ScriptableObjects/FoliageMetaData")]
public class FoliageMetaData : ScriptableObject
{
  public FoliageType _type;
    
  [Serializable]
  public class LODData
  {
    public int _lodLevel;
    public float _lodRange;
    public Mesh _mesh;
    public Material _material;
  }
  
  public LODData[] _lodData;
    
  public (Mesh, Material) GetLODData(int lodLevel) {
    LODData data = _lodData[lodLevel];
        
    Mesh lodMesh = data._mesh;
    Material lodMat = data._material;
        
    return ((lodMesh != null && lodMat != null) ? (mesh: lodMesh, material: lodMat) : (null, null));
  }

  public float GetRange(int lodLevel) {
    return _lodData[lodLevel]._lodRange;
  }
}