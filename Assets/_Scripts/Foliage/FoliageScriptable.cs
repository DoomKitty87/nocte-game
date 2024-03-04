using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Foliage Name", menuName = "ScriptableObjects/FoliageObject")]
public class FoliageScriptable : ScriptableObject
{
    public FoliageType type;

    public FoliageLODData[] _lodRanges;
}

[System.Serializable]
public class FoliageLODData
{
    public Mesh Mesh;
    public Material Material;
    public int Distance;
    public float Density;
}

public enum FoliageType
{
    PineTree,
    OakTree,
    Bush
}