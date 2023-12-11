 using System;
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GrassScript : MonoBehaviour
{
  public Material material;
  public Mesh mesh;
  public float density;
  private int numInstances;

  private Mesh objMesh;

  struct MyInstanceData
  {
    public Matrix4x4 objectToWorld;
    public float myOtherData;
    public uint renderingLayerMask;
  };

  private void Start() {
    objMesh = GetComponent<MeshFilter>().mesh;

    numInstances = objMesh.vertices.Length;
  }

  void Update()
  {
    RenderParams rp = new RenderParams(material);
    MyInstanceData[] instData = new MyInstanceData[numInstances];
    for(int i=0; i<numInstances; ++i)
    {
      instData[i].objectToWorld = Matrix4x4.Translate(transform.TransformPoint(objMesh.vertices[i]));
      // instData[i].renderingLayerMask = (i & 2) == 0 ? 1u : 2u;
    }
    Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
  }
}