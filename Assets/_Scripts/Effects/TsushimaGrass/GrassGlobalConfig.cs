using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrassGlobalConfig : MonoBehaviour
{
  [Header("Settings")]
  public int _samplesX, _samplesY;
  public int _tileSplitFactor;
  [Header("Dependencies")]
  public Mesh _grassMesh;
  public Material _renderingMaterial;
  public float _meshBoundsPadding;
  public float _distToPlayerCutoff;
}
