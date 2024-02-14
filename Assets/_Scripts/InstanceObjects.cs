using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InstanceObjects : MonoBehaviour
{
    public Transform _playerTransform;
    
    private InstanceObjectsHandler _instanceObjects;

    public int numberOfInstancesSquared = 5;

    private List<Vector3> positions = new List<Vector3>();
    public Mesh mesh;
    public Material material;

    private Matrix4x4[] instData;

    private void Awake() {
        _instanceObjects = new InstanceObjectsHandler();
        _instanceObjects.InitializeDictionary();
    }

    // Temporary test case
    private void Start() {
        for (int i = 0; i < numberOfInstancesSquared; i++) {
            for (int j = 0; j < numberOfInstancesSquared; j++) {
                _instanceObjects.AddObject(new Vector2(i * 5 - (numberOfInstancesSquared / 2 * 5), j * 5 - (numberOfInstancesSquared / 2 * 5)), FoliageType.TestObject);
                positions.Add(new Vector3(i * 5 - (numberOfInstancesSquared / 2 * 5), 0, j * 5 - (numberOfInstancesSquared / 2 * 5)));
            }
        }

        instData = new Matrix4x4[positions.Count];
    }

    private void Update() {
        TestRenderObject();
        return;
        
        foreach (FoliageChunk chunk in _instanceObjects.ChunksDictionary.Values) {
            foreach (List<FoliageData> type in chunk.FoliageTypePerChunk.Values) {
                RenderObject(type);
            }
        }
    }

    private void TestRenderObject() {
        Material mat = material;
        Mesh mes = mesh;
            
        RenderParams rp = new RenderParams(mat);
        
        for (int j = 0; j < positions.Count; j++) {
            instData[j] = Matrix4x4.Translate(positions[j]);
        }
            
        Graphics.RenderMeshInstanced(rp, mes, 0, instData);
    }
    
    private void RenderObject(List<FoliageData> typeOfFoliage) {
        FoliageData firstData = typeOfFoliage[0];
        FoliageType type = firstData.Type;
        FoliageMetaData metaData = _instanceObjects.GetFoliageMetaData(type);
        
        // TODO: Optimize this by precalculating number of possible LOD in chunk
        int numberOfLODRanges = _instanceObjects.GetFoliageMetaData(firstData.Type)._lodData.Length;
        float[] lodRanges = new float[numberOfLODRanges];
        List<FoliageData>[] lodLevels = new List<FoliageData>[numberOfLODRanges];
        

        BuildLODRanges(metaData, numberOfLODRanges, ref lodRanges);

        for (int i = 0; i < lodLevels.Length; i++) {
            lodLevels[i] = new List<FoliageData>();
        }
        
        foreach (FoliageData data in typeOfFoliage) {
            int lodLevel = GetLODRange(data.Position, lodRanges);
            lodLevels[lodLevel].Add(data);
        }

        for (int i = 0; i < lodLevels.Length - 1; i++) {
            if (lodLevels[i].Count == 0) continue;
            
            Material mat = metaData._lodData[i]._material;
            Mesh mesh = metaData._lodData[i]._mesh;
            
            RenderParams rp = new RenderParams(mat);

            Matrix4x4[] instData = new Matrix4x4[lodLevels[i].Count];

            for (int j = 0; j < instData.Length; j++) {
                instData[j] = Matrix4x4.Translate(lodLevels[i][j].Position);
            }
            
            Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
        }
    }

    private void BuildLODRanges(FoliageMetaData data, int numberOfLODRanges, ref float[] array) {
        for (int i = 0; i < numberOfLODRanges; i++) {
            array[i] = data._lodData[i]._lodRange * data._lodData[i]._lodRange; // Squared due to DistanceSquared function
        }
    }

    private int GetLODRange(Vector3 pos, float[] ranges) {
        return 0;
        float distance = DistanceSquared(_playerTransform.position, pos);
        
        // Searches for LOD range, if none are found then defaults to last range.
        for (int i = 0; i < ranges.Length - 1; i++) {
            // Debug.Log(ranges[i]);
            if (distance < ranges[i]) return i;
        }

        return ranges.Length - 1;
    }

    private float DistanceSquared(Vector3 vector1, Vector3 vector2) {
        Vector3 diff = vector2 - vector1;
        float distance = (diff.x * diff.x) + (diff.y * diff.y) + (diff.z * diff.z);
        return distance;
    }
}

/*
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level7 : MonoBehaviour
{
    [SerializeField] private Mesh _instanceMesh;
    [SerializeField] private Material _instanceMaterial;
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _sliderValueText;
    
    
    private static int _countMultiplier = 1;
    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private ComputeBuffer _argsBuffer;
    private int _count;

    private ComputeBuffer _positionBuffer1, _positionBuffer2;
    private int _cachedMultiplier = 1;

    private void Start()
    {
        _count = SceneTools.GetCount * _countMultiplier;
        ApplyMultiplierUpdate(_countMultiplier, true);
    
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();

        SceneTools.Instance.SetCountText(_count);
        SceneTools.Instance.SetNameText("GPU Instancing Indirect");
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);

        if (Input.GetMouseButtonUp(0) && _countMultiplier != _cachedMultiplier)
        {
            _countMultiplier = _cachedMultiplier;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDisable()
    {
        _positionBuffer1?.Release();
        _positionBuffer1 = null;

        _positionBuffer2?.Release();
        _positionBuffer2 = null;

        _argsBuffer?.Release();
        _argsBuffer = null;
    }

    private void UpdateBuffers()
    {
        // Positions
        _positionBuffer1?.Release();
        _positionBuffer2?.Release();
        _positionBuffer1 = new ComputeBuffer(_count, 16);
        _positionBuffer2 = new ComputeBuffer(_count, 16);

        var positions1 = new Vector4[_count];
        var positions2 = new Vector4[_count];

        // Grouping cubes into a bunch of spheres
        var offset = Vector3.zero;
        var batchIndex = 0;
        var batch = 0;
        for (var i = 0; i < _count; i++)
        {
            var dir = Random.insideUnitSphere.normalized;
            positions1[i] = dir * Random.Range(10, 15) + offset;
            positions2[i] = dir * Random.Range(30, 50) + offset;

            positions1[i].w = Random.Range(-3f, 3f);
            positions2[i].w = batch;

            if (batchIndex++ == 250000)
            {
                batchIndex = 0;
                batch++;
                offset += new Vector3(90, 0, 0);
            }
        }

        _positionBuffer1.SetData(positions1);
        _positionBuffer2.SetData(positions2);
        _instanceMaterial.SetBuffer("position_buffer_1", _positionBuffer1);
        _instanceMaterial.SetBuffer("position_buffer_2", _positionBuffer2);
        _instanceMaterial.SetColorArray("color_buffer", SceneTools.Instance.ColorArray);

        // Verts
        _args[0] = _instanceMesh.GetIndexCount(0);
        _args[1] = (uint)_count;
        _args[2] = _instanceMesh.GetIndexStart(0);
        _args[3] = _instanceMesh.GetBaseVertex(0);

        _argsBuffer.SetData(_args);
    }

    public void UpdateMultiplier(float val)
    {
        ApplyMultiplierUpdate(Mathf.CeilToInt(val));
    }

    private void ApplyMultiplierUpdate(int val, bool applySliderChange = false)
    {
        _sliderValueText.text = $"Multiplier: {val.ToString()}";
        _cachedMultiplier = val;
        if(applySliderChange) _slider.value = val;
    }
}
*/
