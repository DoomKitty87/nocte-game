using UnityEngine;

public class GrassManager : MonoBehaviour
{
    [SerializeField, Min(0)] private int _distance;
    
    private Vector3[][] _vertices;
    private int[][] _tris;
    private Bounds[] _bounds;
    private Vector3[] _positions;
    private WorldGenerator _worldGenerator;
    private RenderGrass _renderGrass;
    
    private void Awake() {
        _worldGenerator = GetComponent<WorldGenerator>();
        _renderGrass = GetComponent<RenderGrass>();
    }

    public void GenerateGrass() {
        if (_distance == 0) return;
        var worldTiles = _worldGenerator.GetVertices(_distance);

        _renderGrass._meshes = worldTiles.Item1;
        for (int i = 0; i < worldTiles.Item2.Length; i++)
            worldTiles.Item2[i] += (((_worldGenerator.Size - 1) / 2) * _worldGenerator.Resolution) * new Vector3(1, 0, 1);
        _renderGrass._positions = worldTiles.Item2;
        
        _renderGrass._regenerateGrass = true;
        _renderGrass._enableGrass = true;
    }
}
