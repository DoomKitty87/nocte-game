using System;
using UnityEngine;
using System.Collections.Generic;
public class WorldGen : MonoBehaviour
{
    private struct WorldTile
    {
        public GameObject obj;
        public Mesh mesh;
        public MeshCollider meshCollider;
        public float[] temperatureMap;
        public float[] humidityMap;
        public int x;
        public int z;

    }
    
    public int _xSize = 64;
    public int _zSize = 64;

    public float _xResolution = 0.25f;
    public float _zResolution = 0.25f;
    
    public int _xTiles = 101;
    public int _zTiles = 101;
    public float _scale = 1000;
    
    public float _amplitude = 50;
    public int _octaves = 10;

    public float _temperatureScale = 2;
    public float _humidityScale = 1.5f;

    public int _seed = 0;
    
    public Material _material;
    public Gradient _colorGradient;
    public bool _useColorGradient;
    public AnimationCurve _easeCurve;
    
    public int _maxUpdatesPerFrame = 5;

    public bool _hasColliders;

    private WorldTile[] _tilePool;
    private int[,] _tilePositions;

    private int _lastPlayerChunkX;
    private int _lastPlayerChunkZ;
    
    private List<int> _updateQueue = new List<int>();

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
                _tilePool[_tilePositions[0, i]].x = playerXChunkScale - (_xTiles - 1) / 2;
                _tilePool[_tilePositions[0, i]].z = i + playerZChunkScale - (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[0, i]);
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
                _tilePool[_tilePositions[_xTiles - 1, i]].x = playerXChunkScale + (_xTiles - 1) / 2;
                _tilePool[_tilePositions[_xTiles - 1, i]].z = i + playerZChunkScale - (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[_xTiles - 1, i]);
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
                _tilePool[_tilePositions[i, 0]].x = i + playerXChunkScale - (_xTiles - 1) / 2;
                _tilePool[_tilePositions[i, 0]].z = playerZChunkScale - (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[i, 0]);
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
                _tilePool[_tilePositions[i, _zTiles - 1]].x = i + playerXChunkScale - (_xTiles - 1) / 2;
                _tilePool[_tilePositions[i, _zTiles - 1]].z = playerZChunkScale + (_zTiles - 1) / 2;
                QueueTileUpdate(_tilePositions[i, _zTiles - 1]);
            }
        }

        if (deltaZ != 0 || deltaX != 0) {
            for (int x = 0; x < _xTiles; x++) {
                for (int z = 0; z < _zTiles; z++) {
                    float maxDist = Mathf.Max(Mathf.Abs(_tilePool[_tilePositions[x, z]].x - playerXChunkScale), Mathf.Abs(_tilePool[_tilePositions[x, z]].z - playerZChunkScale));
                    if (_hasColliders && maxDist < 2) UpdateCollider(_tilePositions[x, z]);
                    else if (_hasColliders) _tilePool[_tilePositions[x, z]].meshCollider.enabled = false;
                }
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

    private void Update() {
        for (int i = 0; i < _maxUpdatesPerFrame; i++) {
            if (_updateQueue.Count > 0) {
                UpdateTile(_updateQueue[0]);
                _updateQueue.RemoveAt(0);
            }
        }
    }
    
    private void SetupPool()
    {
        _tilePool = new WorldTile[_xTiles * _zTiles];
        _tilePositions = new int[_xTiles, _zTiles];
        for (int x = -(_xTiles - 1) / 2, i = 0; x <= (_xTiles - 1) / 2; x++)
        {
            for (int z = -(_zTiles - 1) / 2; z <= (_zTiles - 1) / 2; z++) {
                GenerateTile(x, z, i);
                i++;
            }
        }
    }

    private void GenerateTile(int x, int z, int index)
    {
        GameObject go = new GameObject("Tile");
        go.transform.parent = transform;
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = _material;
        mf.mesh = new Mesh();
        Mesh msh = mf.mesh;
        Vector3[] vertexData = NoiseMaps.GenerateTerrain(x * _xSize * _xResolution + _seed, z * _zSize * _zResolution + _seed, _xSize, _zSize, _scale, _amplitude, _octaves, _easeCurve, _xResolution, _zResolution);
        msh.vertices = vertexData;
        WindTriangles(msh);
        if (!_useColorGradient) CalculateUVs(msh);
        else CalculateColors(msh);
        UpdateMesh(msh);
        float[] temperatureMap = NoiseMaps.GenerateTemperatureMap(vertexData, x * _xSize * _xResolution * _seed, z * _zSize * _zResolution * _seed, _xSize, _zSize, _scale / _temperatureScale, _easeCurve, _xResolution, _zResolution);
        float[] humidityMap = NoiseMaps.GenerateHumidityMap(vertexData, temperatureMap, x * _xSize * _xResolution / _seed, z * _zSize * _zResolution / _seed, _xSize, _zSize, _scale / _humidityScale, _easeCurve, _xResolution, _zResolution);
        go.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
        go.isStatic = true;
        
        //If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
        go.tag = "Ground";
        
        WorldTile tile = new WorldTile();
        if (_hasColliders) {
            tile.meshCollider = go.AddComponent<MeshCollider>();
            if (Math.Abs(x) < 2 && Math.Abs(z) < 2) tile.meshCollider.sharedMesh = msh;
            else tile.meshCollider.enabled = false;
        }
        tile.obj = go;
        tile.mesh = msh;
        tile.temperatureMap = temperatureMap;
        tile.humidityMap = humidityMap;
        tile.x = x;
        tile.z = z;
        _tilePool[index] = tile;
        _tilePositions[index / _zTiles, index % _zTiles] = index;
    }

    private void QueueTileUpdate(int index) {
        _updateQueue.Add(index);
    }
    
    //Regenerate given tile based on an LOD parameter.
    private void UpdateTile(int index) {
        int x = _tilePool[index].x;
        int z = _tilePool[index].z;
        _tilePool[index].mesh.Clear();
        _tilePool[index].mesh.vertices = NoiseMaps.GenerateTerrain(x * _xSize * _xResolution + _seed, z * _zSize * _zResolution + _seed, _xSize, _zSize, _scale, _amplitude, _octaves, _easeCurve, _xResolution, _zResolution);
        _tilePool[index].temperatureMap = NoiseMaps.GenerateTemperatureMap(_tilePool[index].mesh.vertices, x * _xSize * _xResolution * _seed, z * _zSize * _zResolution * _seed, _xSize, _zSize, _scale / _temperatureScale, _easeCurve, _xResolution, _zResolution);
        _tilePool[index].humidityMap = NoiseMaps.GenerateHumidityMap(_tilePool[index].mesh.vertices, _tilePool[index].temperatureMap, x * _xSize * _xResolution / _seed, z * _zSize * _zResolution / _seed, _xSize, _zSize, _scale / _humidityScale, _easeCurve, _xResolution, _zResolution);
        WindTriangles(_tilePool[index].mesh);
        if (!_useColorGradient) CalculateUVs(_tilePool[index].mesh);
        else CalculateColors(_tilePool[index].mesh);
        UpdateMesh(_tilePool[index].mesh);
        _tilePool[index].obj.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
    }

    private void WindTriangles(Mesh targetMesh) {
        int[] triangles = new int[_xSize * _zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < _zSize; z++)
        {
            for (int x = 0; x < _xSize; x++)
            {
                triangles[tris] = vert + 0;
                triangles[tris + 1] = vert + _xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + _xSize + 1;
                triangles[tris + 5] = vert + _xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        targetMesh.triangles = triangles;
    }

    private void CalculateUVs(Mesh targetMesh) {
        int xSize = _xSize + 1;
        int zSize = _zSize + 1;

        Vector2[] uvs = new Vector2[xSize * zSize];

        Vector3[] vertices = targetMesh.vertices;
        for (int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x / xSize, vertices[i].z / xSize);
        }

        targetMesh.uv = uvs;
    }

    private void CalculateColors(Mesh targetMesh) {
        Vector3[] vertices = targetMesh.vertices;
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            colors[i] = _colorGradient.Evaluate(vertices[i].y / (_amplitude * 2));
        }

        targetMesh.colors = colors;
    }

    private void UpdateCollider(int index) {
        _tilePool[index].meshCollider.enabled = true;
        _tilePool[index].meshCollider.sharedMesh = _tilePool[index].mesh;
    }

    private static void UpdateMesh(Mesh targetMesh)
    {
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
    }

}