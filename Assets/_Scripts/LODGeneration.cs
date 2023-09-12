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
    
    public int _xTiles = 100;
    public int _zTiles = 100;
    public float _scale = 1000;

    public float _amplitude = 50;
    public int _octaves = 10;

    public float _temperatureScale = 2;
    public float _humidityScale = 1.5f;

    public int _seed = 0;
    
    public Material _material;
    public AnimationCurve _easeCurve;

    public bool _hasColliders;

    private WorldTile[] _tilePool;
    private int[] _tilePositions;

    private int _lastPlayerChunkX;
    private int _lastPlayerChunkZ;

    public void UpdatePlayerLoadedChunks(float playerX, float playerZ)
    {
        int playerXChunkScale = (int) (playerX / (_xSize * _xResolution));
        int playerZChunkScale = (int) (playerZ / (_zSize * _zResolution));

        int deltaX = playerXChunkScale - _lastPlayerChunkX;
        int deltaZ = playerZChunkScale - _lastPlayerChunkZ;

        if (deltaX < 0)
        {
            //If i dont know how to do this just switch it to 2d array cause i hate 1d
            int[] tempValues = new int[_zTiles];
            for (int i = 0; i < _zTiles; i++)
            {
                tempValues[i] = _tilePositions[(_xTiles - 1) * _zTiles + i];
            }

            for (int i = _tilePositions.Length - 1 - _zTiles; i >= 0; i++)
            {
                _tilePositions[i + _zTiles] = _tilePositions[i];
            }

            for (int i = 0; i < _zTiles; i++)
            {
                _tilePositions[i] = tempValues[i];
            }
        }

        if (deltaZ != 0)
        {
            
        }

        _lastPlayerChunkX = playerXChunkScale;
        _lastPlayerChunkZ = playerZChunkScale;
    }
    
    private void Start()
    {
        _scale = 1 / _scale;
        SetupPool();
    }

    //Tile positions are setup as
    //Z horizontal * X vertical (in a 2d array)
    private void SetupPool()
    {
        _tilePool = new WorldTile[_xTiles * _zTiles];
        for (int x = -_xTiles / 2, i = 0; x < _xTiles / 2; x++)
        {
            for (int z = -_zTiles / 2; z < _zTiles / 2; z++)
            {
                GenerateTile(x, z, i);
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
        UpdateMesh(msh);
        float[] temperatureMap = NoiseMaps.GenerateTemperatureMap(vertexData, x * _xSize * _xResolution * _seed, z * _zSize * _zResolution * _seed, _xSize, _zSize, _scale / _temperatureScale, _easeCurve, _xResolution, _zResolution);
        float[] humidityMap = NoiseMaps.GenerateHumidityMap(vertexData, temperatureMap, x * _xSize * _xResolution / _seed, z * _zSize * _zResolution / _seed, _xSize, _zSize, _scale / _humidityScale, _easeCurve, _xResolution, _zResolution);
        if (_hasColliders) go.AddComponent<MeshCollider>();
        go.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
        go.isStatic = true;
        WorldTile tile = new WorldTile();
        tile.obj = go;
        tile.mesh = msh;
        tile.temperatureMap = temperatureMap;
        tile.humidityMap = humidityMap;
        _tilePool[index] = tile;
        _tilePositions[index] = index;
    }
    
    //Regenerate given tile based on an LOD parameter.
    private void UpdateTile(int index, int lod, int x, int z)
    {
        int lodFactor = (int) Mathf.Pow(2, lod);
        _tilePool[index].mesh.vertices = NoiseMaps.GenerateTerrain(x * _xSize * _xResolution + _seed, z * _zSize * _zResolution + _seed, _xSize / lodFactor, _zSize / lodFactor, _scale, _amplitude, _octaves, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        _tilePool[index].temperatureMap = NoiseMaps.GenerateTemperatureMap(_tilePool[index].mesh.vertices, x * _xSize * _xResolution * _seed, z * _zSize * _zResolution * _seed, _xSize / lodFactor, _zSize / lodFactor, _scale / _temperatureScale, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
        _tilePool[index].humidityMap = NoiseMaps.GenerateHumidityMap(_tilePool[index].mesh.vertices, _tilePool[index].temperatureMap, x * _xSize * _xResolution / _seed, z * _zSize * _zResolution / _seed, _xSize / lodFactor, _zSize / lodFactor, _scale / _humidityScale, _easeCurve, _xResolution * lodFactor, _zResolution * lodFactor);
    }

    private void WindTriangles(Mesh targetMesh)
    {
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

    private static void UpdateMesh(Mesh targetMesh)
    {
        targetMesh.RecalculateNormals();
    }

}