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

    Vector2 centermin = new Vector2(gomesh.bounds.min.x + gomesh.bounds.size.x / 2 + transform.position.x, gomesh.bounds.min.z + gomesh.bounds.size.z / 2 + transform.position.z);
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

    triangles[0] = 0;
    triangles[1] = 1;
    triangles[2] = 4;

    triangles[3] = 1;
    triangles[4] = 5;
    triangles[5] = 4;

    triangles[6] = 1;
    triangles[7] = 2;
    triangles[8] = 5;

    triangles[9] = 2;
    triangles[10] = 6;
    triangles[11] = 5;

    triangles[12] = 2;
    triangles[13] = 3;
    triangles[14] = 6;

    triangles[15] = 3;
    triangles[16] = 7;
    triangles[17] = 6;

    triangles[18] = 3;
    triangles[19] = 0;
    triangles[20] = 7;

    triangles[21] = 0;
    triangles[22] = 4;
    triangles[23] = 7;

    Mesh msh = new Mesh();
    msh.vertices = vertices;
    msh.triangles = triangles;
    go.AddComponent<MeshFilter>().mesh = msh;
    go.AddComponent<MeshRenderer>().material = _material;
    go.AddComponent<MeshCollider>().sharedMesh = msh;
  }

}