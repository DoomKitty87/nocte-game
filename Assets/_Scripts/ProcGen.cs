using UnityEngine;

public class ProcGen : MonoBehaviour
{
    
    public int xSize = 32;
    public int zSize = 32;

    public float xResolution = 1;
    public float zResolution = 1;
    
    public int xTiles = 1;
    public int zTiles = 1;

    public float scale = 1000;

    public float amplitude = 50;
    public int octaves = 4;

    public int seed = 0;
    
    public Material material;
    public AnimationCurve easeCurve;

    private Mesh[] _meshes;

    private void Start()
    {
        scale = 1 / scale;
        for (int z = 0; z < zTiles; z++)
        {
            for (int x = 0; x < xTiles; x++)
            {
                GameObject go = new GameObject("Tile");
                go.transform.parent = transform;
                
                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = material;
                mf.mesh = new Mesh();
                Mesh msh = mf.mesh;
                msh.vertices = LandscapeModifiers.ContourMountains(NoiseMaps.GenerateTerrain(x * xSize * xResolution + seed, z * zSize * zResolution + seed, xSize, zSize, scale, amplitude, octaves, easeCurve, xResolution, zResolution));
                WindTriangles(msh);
                UpdateMesh(msh);
                go.transform.position = new Vector3(x * xSize * xResolution, 0, z * zSize * zResolution);
            }
        }
    }

    private void WindTriangles(Mesh targetMesh)
    {
        int[] triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        targetMesh.triangles = triangles;
    }

    private static void UpdateMesh(Mesh targetMesh)
    {
        targetMesh.RecalculateNormals();
    }

}