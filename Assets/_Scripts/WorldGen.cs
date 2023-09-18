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
        public float[] largeScaleHeight;
        public int x;
        public int z;
        public int grassIndexStart;
        public int grassCount;

    }

    private struct GrassData
    {

        public Vector4 position;
        public Vector2 uv;
        public float displacement;

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

    public GameObject _scatterMesh;
    public float _scatterDensity = 0.05f;

    private WorldTile[] _tilePool;
    private int[,] _tilePositions;

    private int _lastPlayerChunkX;
    private int _lastPlayerChunkZ;
    
    private List<int> _updateQueue = new List<int>();
    private List<int[]> _generateQueue = new List<int[]>();

    private Texture2D _wind;
    
    public Material _material2;
    public Mesh _mesh;

    public int _maxGrassDistChunks = 10;

    GraphicsBuffer _commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;
    private ComputeBuffer _positionsBuffer;
    const int _commandCount = 2;
    private List<GrassData> _grassData;
    private RenderParams _rp;

    private float _playerX, _playerZ;
    private int _playerXChunkScale, _playerZChunkScale;

    private Vector2 _windPos;
    
    //TODO: Improve grass performance (frustum culling?)

    public void UpdatePlayerLoadedChunks(Vector3 playerPos)
    {
        int playerXChunkScale = (int) (playerPos.x / (_xSize * _xResolution));
        int playerZChunkScale = (int) (playerPos.z / (_zSize * _zResolution));

        int deltaX = playerXChunkScale - _lastPlayerChunkX;
        int deltaZ = playerZChunkScale - _lastPlayerChunkZ;

        _playerX = playerPos.x;
        _playerZ = playerPos.z;
        _playerXChunkScale = playerXChunkScale;
        _playerZChunkScale = playerZChunkScale;
        
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
                    else if (_hasColliders) {
                        if (_tilePool[_tilePositions[x, z]].meshCollider) _tilePool[_tilePositions[x, z]].meshCollider.enabled = false;
                    }
                    if (maxDist <= _maxGrassDistChunks && !(x == _xTiles - 1 && deltaX == 1) && !(x == 0 && deltaX == -1) && !(z == _zTiles - 1 && deltaZ == 1) && !(z == 0 && deltaZ == -1)) {
                        if (_tilePool[_tilePositions[x, z]].grassCount > 0) continue;
                        Vector3[] normals = _tilePool[_tilePositions[x, z]].mesh.normals;
                        Vector3[] vertexData = _tilePool[_tilePositions[x, z]].mesh.vertices;
                        _tilePool[_tilePositions[x, z]].grassIndexStart = _grassData.Count;
                        for (int i = 0; i < vertexData.Length; i += 3) {
                            if (normals[i].y < 0.6f) continue;
                            GrassData gd = new GrassData();
                            gd.position = new Vector4(vertexData[i].x + (_tilePool[_tilePositions[x, z]].x * _xSize * _xResolution), vertexData[i].y, vertexData[i].z + (_tilePool[_tilePositions[x, z]].z * _zSize * _zResolution), 0);
                            gd.uv = new Vector2(gd.position.x / (_xSize * _xResolution * _xTiles), gd.position.z / (_zSize * _zResolution * _zTiles));
                            _grassData.Add(gd);
                        }
                        _tilePool[_tilePositions[x, z]].grassCount = _grassData.Count - _tilePool[_tilePositions[x, z]].grassIndexStart;
                    }
                    else if (_tilePool[_tilePositions[x, z]].grassCount > 0) {
                        _grassData.RemoveRange(_tilePool[_tilePositions[x, z]].grassIndexStart, _tilePool[_tilePositions[x, z]].grassCount);
                        for (int i = 0; i < _xTiles * _zTiles; i++) {
                            if (_tilePool[i].grassIndexStart > _tilePool[_tilePositions[x, z]].grassIndexStart) _tilePool[i].grassIndexStart -= _tilePool[_tilePositions[x, z]].grassCount;
                        }
                        _tilePool[_tilePositions[x, z]].grassCount = 0;
                        _tilePool[_tilePositions[x, z]].grassIndexStart = 0;
                    }
                }
            }
        }
        
        _lastPlayerChunkX = playerXChunkScale;
        _lastPlayerChunkZ = playerZChunkScale;
        _rp.matProps.SetVector("_PlayerPosition", playerPos);
    }
    
    private void Start() {
        _scale = 1 / _scale;
        SetupPool();
        _commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, _commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[_commandCount];
        _grassData = new List<GrassData>();
        _positionsBuffer = new ComputeBuffer((_xSize + 1) * (_zSize + 1) * _xTiles * _zTiles, sizeof(float) * 7, ComputeBufferType.Default);
        _rp = new RenderParams(_material2);
        _rp.matProps = new MaterialPropertyBlock();
        _wind = new Texture2D(128, 128);
        _rp.matProps.SetTexture("_Wind", _wind);
        _rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(0, 0, 0)));
        _rp.matProps.SetBuffer("_PositionsBuffer", _positionsBuffer);
    }

    private void Update() {
        for (int i = 0; i < _maxUpdatesPerFrame; i++) {
            if (_updateQueue.Count > 0 && _generateQueue.Count == 0) {
                UpdateTile(_updateQueue[0]);
                _updateQueue.RemoveAt(0);
            }
            else if (_generateQueue.Count > 0) {
                GenerateTile(_generateQueue[0][0], _generateQueue[0][1], _generateQueue[0][2]);
                _generateQueue.RemoveAt(0);
                if (_generateQueue.Count == 0) Time.timeScale = 1f;
            }
            else break;
        }
        if (_generateQueue.Count != 0) return;
        UpdateWind();
        _wind.Apply();
        Graphics.RenderMeshIndirect(_rp, _mesh, _commandBuf, _commandCount);
    }
    
    private void SetupPool() {
        Time.timeScale = 0f;
        _tilePool = new WorldTile[_xTiles * _zTiles];
        _tilePositions = new int[_xTiles, _zTiles];
        for (int x = -(_xTiles - 1) / 2, i = 0; x <= (_xTiles - 1) / 2; x++)
        {
            for (int z = -(_zTiles - 1) / 2; z <= (_zTiles - 1) / 2; z++) {
                QueueTileGen(x, z, i);
                i++;
            }
        }
    }

    private void OnDestroy() {
        _commandBuf?.Release();
        _positionsBuffer?.Release();
        _commandBuf = null;
        _positionsBuffer = null;
    }

    private void UpdateWind() {
        _windPos += new Vector2(Time.deltaTime, Time.deltaTime);
        float[] windData = NoiseMaps.GenerateWindMap(128, 128, _windPos.x, _windPos.y, 0.1f);
        for (int i = 0; i < windData.Length; i++) {
            _wind.SetPixel(i % 128, i / 128, new Color(windData[i], windData[i], windData[i], 1));
        }
        _wind.Apply();
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
        float seed = int.Parse(Hash128.Compute(_seed).ToString(), System.Globalization.NumberStyles.HexNumber);
        Vector3[] vertexData = NoiseMaps.GenerateTerrain(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _scale, _amplitude, _octaves, _easeCurve, _xResolution, _zResolution);
        msh.vertices = vertexData;
        WindTriangles(msh);

        
        float[] temperatureMap = NoiseMaps.GenerateTemperatureMap(vertexData, x * _xSize * _xResolution + (seed * 2), z * _zSize * _zResolution + (seed * 2), _xSize, _zSize, _scale / _temperatureScale, _easeCurve, _xResolution, _zResolution);
        float[] humidityMap = NoiseMaps.GenerateHumidityMap(vertexData, temperatureMap, x * _xSize * _xResolution + (seed * 0.5f), z * _zSize * _zResolution + (seed * 0.5f), _xSize, _zSize, _scale / _humidityScale, _easeCurve, _xResolution, _zResolution);
        float[] largeScaleHeight = NoiseMaps.GenerateLargeScaleHeight(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _scale, _amplitude,  _easeCurve, _xResolution, _zResolution);
        go.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
        go.isStatic = true;
        
        //If you need to put anything else (tag, components, etc) on the tile, do it here. If it needs to change every time the LOD is changed, do it in the UpdateTile function.
        go.tag = "Ground";
        
        WorldTile tile = new WorldTile();
        if (_hasColliders && Math.Abs(x) < 2 && Math.Abs(z) < 2) {
            tile.meshCollider = go.AddComponent<MeshCollider>();
            tile.meshCollider.sharedMesh = msh;
        }
        tile.obj = go;
        tile.mesh = msh;
        tile.temperatureMap = temperatureMap;
        tile.humidityMap = humidityMap;
        tile.largeScaleHeight = largeScaleHeight;
        tile.x = x;
        tile.z = z;
        _tilePool[index] = tile;
        UpdateMesh(msh);
        if (!_useColorGradient) CalculateUVs(msh);
        else CalculateColors(index);
        //ScatterObjects(msh, _scatterMesh, _scatterDensity, index);
        _tilePositions[index / _zTiles, index % _zTiles] = index;
        if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) <= _maxGrassDistChunks) {
            Vector3[] normals = msh.normals;
            _tilePool[index].grassIndexStart = _grassData.Count;
            for (int i = 0; i < vertexData.Length; i += 3) {
                if (normals[i].y < 0.6f) continue;
                GrassData gd = new GrassData();
                gd.position = new Vector4(vertexData[i].x + (x * _xSize * _xResolution), vertexData[i].y, vertexData[i].z + (z * _zSize * _zResolution), 0);
                gd.uv = new Vector2(Mathf.Abs(gd.position.x / (_xSize * _xResolution * _xTiles)), Mathf.Abs(gd.position.z / (_zSize * _zResolution * _zTiles)));
                _grassData.Add(gd);
            }
            _tilePool[index].grassCount = _grassData.Count - _tilePool[index].grassIndexStart;
        }

        if (index == (_xTiles * _zTiles) - 1) {
            UpdateGrassBuffers();
        }
    }

    private void QueueTileUpdate(int index) {
        _updateQueue.Add(index);
    }

    private void QueueTileGen(int x, int z, int index) {
        _generateQueue.Add(new int[] {
            x, z, index
        }); 
    }
    
    //Regenerate given tile based on an LOD parameter.
    private void UpdateTile(int index) {
        int x = _tilePool[index].x;
        int z = _tilePool[index].z;
        _tilePool[index].mesh.Clear();
        float seed = int.Parse(Hash128.Compute(_seed).ToString(), System.Globalization.NumberStyles.HexNumber);
        _tilePool[index].mesh.vertices = NoiseMaps.GenerateTerrain(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _scale, _amplitude, _octaves, _easeCurve, _xResolution, _zResolution);
        _tilePool[index].temperatureMap = NoiseMaps.GenerateTemperatureMap(_tilePool[index].mesh.vertices, x * _xSize * _xResolution + (seed * 2), z * _zSize * _zResolution + (seed * 2), _xSize, _zSize, _scale / _temperatureScale, _easeCurve, _xResolution, _zResolution);
        _tilePool[index].humidityMap = NoiseMaps.GenerateHumidityMap(_tilePool[index].mesh.vertices, _tilePool[index].temperatureMap, x * _xSize * _xResolution + (seed * 0.5f), z * _zSize * _zResolution + (seed * 0.5f), _xSize, _zSize, _scale / _humidityScale, _easeCurve, _xResolution, _zResolution);
        _tilePool[index].largeScaleHeight = NoiseMaps.GenerateLargeScaleHeight(x * _xSize * _xResolution + seed, z * _zSize * _zResolution + seed, _xSize, _zSize, _scale, _amplitude,  _easeCurve, _xResolution, _zResolution);

        WindTriangles(_tilePool[index].mesh);
        UpdateMesh(_tilePool[index].mesh);
        if (!_useColorGradient) CalculateUVs(_tilePool[index].mesh);
        else CalculateColors(index);
        //ScatterObjects(_tilePool[index].mesh, _scatterMesh, _scatterDensity, index);
        _tilePool[index].obj.transform.position = new Vector3(x * _xSize * _xResolution, 0, z * _zSize * _zResolution);
        int maxDist = Mathf.Max(Mathf.Abs(x - _playerXChunkScale), Mathf.Abs(z - _playerZChunkScale));
        if (maxDist <= _maxGrassDistChunks) {
            Vector3[] normals = _tilePool[index].mesh.normals;
            Vector3[] vertexData = _tilePool[index].mesh.vertices;
            _tilePool[index].grassIndexStart = _grassData.Count;
            for (int i = 0; i < vertexData.Length; i += 3) {
                if (normals[i].y < 0.6f) continue;
                GrassData gd = new GrassData();
                gd.position = new Vector4(vertexData[i].x + (_tilePool[index].x * _xSize * _xResolution), vertexData[i].y, vertexData[i].z + (_tilePool[index].z * _zSize * _zResolution), 0);
                gd.uv = new Vector2(gd.position.x / (_xSize * _xResolution * _xTiles), gd.position.z / (_zSize * _zResolution * _zTiles));
                _grassData.Add(gd);
            }
            _tilePool[index].grassCount = _grassData.Count - _tilePool[index].grassIndexStart;
        }

        if (_updateQueue.Count > 1) return;
        UpdateGrassBuffers();
    }

    private void UpdateGrassBuffers() {
        _rp.worldBounds = new Bounds(new Vector3(_playerX, 0, _playerZ), _xTiles * _xSize * _xResolution * Vector3.one); // use tighter bounds for better FOV culling
        _commandData[0].indexCountPerInstance = _mesh.GetIndexCount(0);
        _commandData[0].instanceCount = (uint) _grassData.Count;
        _commandData[1].indexCountPerInstance = _mesh.GetIndexCount(0);
        _commandData[1].instanceCount = (uint) _grassData.Count;
        _commandBuf.SetData(_commandData);
        _positionsBuffer.SetData(_grassData);
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

    private void CalculateColors(int index) {
        Color[] colors = new Color[_tilePool[index].temperatureMap.Length];
        for (int i = 0; i < _tilePool[index].temperatureMap.Length; i++) {
            colors[i] = _colorGradient.Evaluate(_tilePool[index].temperatureMap[i]);
        }

        _tilePool[index].mesh.colors = colors;
    }

    private void ScatterObjects(Mesh targetMesh, GameObject toScatter, float density, int index) {
        Vector3[] vertices = targetMesh.vertices;
        List<Vector2> points = PoissonDisk.GeneratePoints(1 / density, new Vector2(_xSize, _zSize));
        for (int i = 0; i < points.Count; i++) {
            Vector3 vertex = vertices[(int)points[i].x + (int)points[i].y * _xSize];
            GameObject go = Instantiate(toScatter, new Vector3(vertex.x + (_tilePool[index].x * _xSize * _xResolution), vertex.y, vertex.z + (_tilePool[index].z * _zSize * _zResolution)), Quaternion.identity);
            go.transform.parent = transform;
        }
    }

    private void UpdateCollider(int index) {
        if (!_tilePool[index].meshCollider) _tilePool[index].meshCollider = _tilePool[index].obj.AddComponent<MeshCollider>();
        else if (_tilePool[index].meshCollider.enabled) return;
        _tilePool[index].meshCollider.enabled = true;
        _tilePool[index].meshCollider.sharedMesh = _tilePool[index].mesh;
    }

    private static void UpdateMesh(Mesh targetMesh) {
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
    }

}