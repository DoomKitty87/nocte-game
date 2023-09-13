using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LODGeneration : MonoBehaviour
{
    private struct WorldTile
    {
        public GameObject obj;
        public Mesh mesh;
        public float[] temperatureMap;
        public float[] humidityMap;
    }
    
    public int _xSize = 32;
    public int _zSize = 32;

    public float _xResolution = 0.25f;
    public float _zResolution = 0.25f;
    
    public int _xTiles = 101;
    public int _zTiles = 101;
    public float _scale = 1000;

    public int _lodNoFalloffDist = 10;

    public float _amplitude = 50;
    public int _octaves = 10;

    public float _temperatureScale = 2;
    public float _humidityScale = 1.5f;

    public int _seed = 0;
    
    public Material _material;
    public AnimationCurve _easeCurve;

    public bool _hasColliders;

    private WorldTile[] _tilePool;
    private int[,] _tilePositions;

    private int _lastPlayerChunkX;
    private int _lastPlayerChunkZ;

    public void UpdatePlayerLoadedChunks(float playerX, float playerZ)
    {
        int playerXChunkScale = (int) (playerX / (_xSize * _xResolution));
        int playerZChunkScale = (int) (playerZ / (_zSize * _zResolution));

        int deltaX = playerXChunkScale - _lastPlayerChunkX;
        int deltaZ = playerZChunkScale - _lastPlayerChunkZ;

        if (deltaX < 0) {
            int[] tempValues = new int[_zTiles];
            for (int i = 0; i < _zTiles; i++) {
                tempValues[i] = _tilePositions[_xTiles - 1, i];
            }
            
            for (int x = _xTiles - 1; x > 0; x--) {
                for (int z = 0; z < _zTiles; z++) {
                    _tilePositions[x, z] = _tilePositions[x - 1, z];
                }
            }
            
            for (int i = 0; i < _zTiles; i++) {
                _tilePositions[0, i] = tempValues[i];
            }
            
            for (int i = 0; i < _zTiles; i++) {
                int lod = 0;
                if (Mathf.Abs(0 - (_xTiles - 1) * 2) > 10 || Mathf.Abs(i - (_zTiles - 1) * 2) > 10) {
                    lod = 2;
                }
                UpdateTile(_tilePositions[0, i], lod, playerXChunkScale - (_xTiles - 1) / 2, i + playerZChunkScale - (_zTiles - 1) / 2);
            }
        }
        else if (deltaX > 0) {
            int[] tempValues = new int[_zTiles];
            for (int i = 0; i < _zTiles; i++) {
                tempValues[i] = _tilePositions[0, i];
            }
            
            for (int x = 0; x < _xTiles - 1; x++) {
                for (int z = 0; z < _zTiles; z++) {
                    _tilePositions[x, z] = _tilePositions[x + 1, z];
                }
            }
            
            for (int i = 0; i < _zTiles; i++) {
                _tilePositions[_xTiles - 1, i] = tempValues[i];
            }
            
            for (int i = 0; i < _zTiles; i++) {
                int lod = 0;
                if (Mathf.Abs(_xTiles - 1 - (_xTiles - 1) * 2) > 10 || Mathf.Abs(i - (_zTiles - 1) * 2) > 10) {
                    lod = 2;
                }
                UpdateTile(_tilePositions[_xTiles - 1, i], lod, playerXChunkScale + (_xTiles - 1) / 2, i + playerZChunkScale - (_zTiles - 1) / 2);
            }
        }

        if (deltaZ < 0) {
            int[] tempValues = new int[_xTiles];
            for (int i = 0; i < _xTiles; i++) {
                tempValues[i] = _tilePositions[i, _zTiles - 1];
            }
            
            for (int x = 0; x < _xTiles; x++) {
                for (int z = _zTiles - 1; z > 0; z--) {
                    _tilePositions[x, z] = _tilePositions[x, z - 1];
                }
            }
            
            for (int i = 0; i < _xTiles; i++) {
                _tilePositions[i, 0] = tempValues[i];
            }
            
            for (int i = 0; i < _xTiles; i++) {
                int lod = 0;
                if (Mathf.Abs(i - (_xTiles - 1) * 2) > 10 || Mathf.Abs(0 - (_zTiles - 1) * 2) > 10) {
                    lod = 2;
                }
                UpdateTile(_tilePositions[i, 0], lod, i + playerXChunkScale - (_xTiles - 1) / 2, playerZChunkScale - (_zTiles - 1) / 2);
            }
        }
        else if (deltaZ > 0) {
            int[] tempValues = new int[_xTiles];
            for (int i = 0; i < _xTiles; i++) {
                tempValues[i] = _tilePositions[i, 0];
            }
            
            for (int x = 0; x < _xTiles; x++) {
                for (int z = 0; z < _zTiles - 1; z++) {
                    _tilePositions[x, z] = _tilePositions[x, z + 1];
                }
            }
            
            for (int i = 0; i < _xTiles; i++) {
                _tilePositions[i, _zTiles - 1] = tempValues[i];
            }
            
            for (int i = 0; i < _xTiles; i++) {
                int lod = 0;
                if (Mathf.Abs(i - (_xTiles - 1) * 2) > 10 || Mathf.Abs(_zTiles - 1 - (_zTiles - 1) * 2) > 10) {
                    lod = 2;
                }
                UpdateTile(_tilePositions[i, _zTiles - 1], lod, i + playerXChunkScale - (_xTiles - 1) / 2, playerZChunkScale + (_zTiles - 1) / 2);
            }
        }
        
        _lastPlayerChunkX = playerXChunkScale;
        _lastPlayerChunkZ = playerZChunkScale;
    }
    
    private void Start()
    {
        _scale = 1 / _scale;
        SetupPool();
    }
    
    private void SetupPool()
    {
        _tilePool = new WorldTile[_xTiles * _zTiles];
        _tilePositions = new int[_xTiles, _zTiles];
        for (int x = -(_xTiles - 1) / 2, i = 0; x <= (_xTiles - 1) / 2; x++)
        {
            for (int z = -(_zTiles - 1) / 2; z <= (_zTiles - 1) / 2; z++) {
                int lod = 0;
                if (Mathf.Abs(x - (_xTiles - 1) * 2) > 10 || Mathf.Abs(z - (_zTiles - 1) * 2) > 10) {
                    lod = 2;
                }
                GenerateTile(x, z, i, lod);
                i++;
            }
        }
    }

    private void GenerateTile(int x, int z, int index, int lod)
    {
        GameObject go = new GameObject("Tile");
        go.transform.parent = transform;
        int lodFactor = (int) Mathf.Pow(2, lod);
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = _material;
        mf.mesh = new Mesh();
        Mesh msh = mf.mesh;
        Vector3[] vertexData = NoiseMaps.GenerateTerrain(x * _xSize * _xResolution + _seed, z * _zSize * _zResolution + _seed, _xSize / lodFactor, _zSize / lodFactor, _scale, _amplitude, _octaves, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        msh.vertices = vertexData;
        WindTriangles(msh, lodFactor);
        CalculateUVs(msh, lodFactor);
        UpdateMesh(msh);
        float[] temperatureMap = NoiseMaps.GenerateTemperatureMap(vertexData, x * _xSize * _xResolution * _seed, z * _zSize * _zResolution * _seed, _xSize / lodFactor, _zSize / lodFactor, _scale / _temperatureScale, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        float[] humidityMap = NoiseMaps.GenerateHumidityMap(vertexData, temperatureMap, x * _xSize * _xResolution / _seed, z * _zSize * _zResolution / _seed, _xSize / lodFactor, _zSize / lodFactor, _scale / _humidityScale, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        if (_hasColliders) go.AddComponent<MeshCollider>();
        go.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
        go.isStatic = true;
        
        //If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
        
        WorldTile tile = new WorldTile();
        tile.obj = go;
        tile.mesh = msh;
        tile.temperatureMap = temperatureMap;
        tile.humidityMap = humidityMap;
        _tilePool[index] = tile;
        _tilePositions[index / _zTiles, index % _zTiles] = index;
    }
    
    //Regenerate given tile based on an LOD parameter.
    private void UpdateTile(int index, int lod, int x, int z) {
        int lodFactor = (int) Mathf.Pow(2, lod);
        _tilePool[index].mesh.vertices = NoiseMaps.GenerateTerrain(x * _xSize * _xResolution + _seed, z * _zSize * _zResolution + _seed, _xSize / lodFactor, _zSize / lodFactor, _scale, _amplitude, _octaves, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        _tilePool[index].temperatureMap = NoiseMaps.GenerateTemperatureMap(_tilePool[index].mesh.vertices, x * _xSize * _xResolution * _seed, z * _zSize * _zResolution * _seed, _xSize / lodFactor, _zSize / lodFactor, _scale / _temperatureScale, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        _tilePool[index].humidityMap = NoiseMaps.GenerateHumidityMap(_tilePool[index].mesh.vertices, _tilePool[index].temperatureMap, x * _xSize * _xResolution / _seed, z * _zSize * _zResolution / _seed, _xSize / lodFactor, _zSize / lodFactor, _scale / _humidityScale, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        WindTriangles(_tilePool[index].mesh, lodFactor);
        CalculateUVs(_tilePool[index].mesh, lodFactor);
        UpdateMesh(_tilePool[index].mesh);
        _tilePool[index].obj.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
    }

    private void WindTriangles(Mesh targetMesh, int lod) {
        int xSize = _xSize / lod;
        int zSize = _zSize / lod;
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

    private void CalculateUVs(Mesh targetMesh, int lod) {
        int xSize = _xSize / lod + 1;
        int zSize = _zSize / lod + 1;

        Vector2[] uvs = new Vector2[xSize * zSize];

        Vector3[] vertices = targetMesh.vertices;
        for (int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x / xSize, vertices[i].z / xSize);
        }

        targetMesh.uv = uvs;
    }

    private static void UpdateMesh(Mesh targetMesh)
    {
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
    }

}