using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRenderObjectsIndirect : MonoBehaviour
{
    public int _length;
    public float _distance;
    public int _numberOfRenderers;
    public float _distanceBetweenRenderers;

    public Mesh _mesh;
    public Material _material;
    
    // private RenderFoliage[] _foliageRenderers;
    private Dictionary<int, RenderFoliage> _foliageRenderers = new Dictionary<int, RenderFoliage>();
    
    private void Start() {
        for (int i = 0; i < _numberOfRenderers; i++) {
            _foliageRenderers.Add(i, new RenderFoliage(_mesh, _material));
            
            _foliageRenderers[i]._initialized = true;
            
            Vector3[] positions = CreatePositions(i);

            _foliageRenderers[i].UpdateBuffer(positions, Vector3.zero);
        }
    }
    
    public void Update() {
        for (int i = 0; i < _numberOfRenderers; i++) {
            _foliageRenderers[i].Render();
        }
    }

    private Vector3[] CreatePositions(int index) {
        int count = _length * _length * _length;
        
        Vector3[] positionsToAdd = new Vector3[count];
        
        int i = 0;
        for (int j = 0; j < _length; j++) {
            for (int k = 0; k < _length; k++) {
                for (int l = 0; l < _length; l++) {
                    positionsToAdd[i] = new Vector3(j * _distance + index * _distanceBetweenRenderers, k * _distance, l * _distance);
                    i++;
                }
            }
        }

        return positionsToAdd;
    }
}
