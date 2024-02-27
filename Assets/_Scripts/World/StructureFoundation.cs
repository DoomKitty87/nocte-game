using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StructureFoundation : MonoBehaviour
{

  [SerializeField] private Material _material;

  private void Start() {
    GameObject go = new GameObject();
    go.transform.parent = transform;
    Vector3[] vertices = new Vector3[8];
    int[] triangles = new int[24];

    Mesh gomesh = gameObject.GetComponent<MeshFilter>().mesh;

    Vector2 centermin = new Vector2(gomesh.bounds.center.x + transform.position.x, gomesh.bounds.center.z + transform.position.z);
    float halfx = gomesh.bounds.size.x / 2 * transform.localScale.x;
    float halfz = gomesh.bounds.size.z / 2 * transform.localScale.z;
    float y = gomesh.bounds.min.y * transform.localScale.y + transform.position.y;

    Vector3 corner1 = new Vector3(centermin.x - halfx, y, centermin.y - halfz);
    Vector3 corner2 = new Vector3(centermin.x - halfx, y, centermin.y + halfz);
    Vector3 corner3 = new Vector3(centermin.x + halfx, y, centermin.y + halfz);
    Vector3 corner4 = new Vector3(centermin.x + halfx, y, centermin.y - halfz);

    vertices[0] = corner1;
    vertices[1] = corner2;
    vertices[2] = corner3;
    vertices[3] = corner4;

    vertices[4] = new Vector3(corner1.x, WorldGenInfo._worldGenerator.GetHeightValue(new Vector2(corner1.x, corner1.z)), corner1.z);
    vertices[5] = new Vector3(corner2.x, WorldGenInfo._worldGenerator.GetHeightValue(new Vector2(corner2.x, corner2.z)), corner2.z);
    vertices[6] = new Vector3(corner3.x, WorldGenInfo._worldGenerator.GetHeightValue(new Vector2(corner3.x, corner3.z)), corner3.z);
    vertices[7] = new Vector3(corner4.x, WorldGenInfo._worldGenerator.GetHeightValue(new Vector2(corner4.x, corner4.z)), corner4.z);

    triangles[0] = 4;
    triangles[1] = 1;
    triangles[2] = 0;

    triangles[3] = 4;
    triangles[4] = 5;
    triangles[5] = 1;

    triangles[6] = 5;
    triangles[7] = 2;
    triangles[8] = 1;

    triangles[9] = 5;
    triangles[10] = 6;
    triangles[11] = 2;

    triangles[12] = 6;
    triangles[13] = 3;
    triangles[14] = 2;

    triangles[15] = 6;
    triangles[16] = 7;
    triangles[17] = 3;

    triangles[18] = 7;
    triangles[19] = 0;
    triangles[20] = 3;

    triangles[21] = 7;
    triangles[22] = 4;
    triangles[23] = 0;

    Mesh msh = new Mesh();
    msh.vertices = vertices;
    msh.triangles = triangles;
    msh.RecalculateNormals();
    msh.RecalculateBounds();
    go.AddComponent<MeshFilter>().mesh = msh;
    go.AddComponent<MeshRenderer>().material = _material;
    go.AddComponent<MeshCollider>().sharedMesh = msh;
  }

}