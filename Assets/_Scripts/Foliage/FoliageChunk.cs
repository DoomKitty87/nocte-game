using System.Collections.Generic;
using UnityEngine;


namespace Foliage
{
  public class FoliageChunk
  {
    private readonly FoliageRenderer[] _renderers;

    private float _chunkSize;
    private Vector2 _chunkCenter;

    public FoliageChunk() {
      
    }
    
    public FoliageChunk(IReadOnlyList<FoliageScriptable> scriptables, Vector2Int chunkPos, float chunkSize, Vector2 cameraPos) {
      var numberOfScriptables = scriptables.Count;
      _chunkSize = chunkSize;
      _chunkCenter = new Vector2(chunkPos.x * chunkSize, chunkPos.y * chunkSize);
      _renderers = new FoliageRenderer[numberOfScriptables];

      for (var i = 0; i < numberOfScriptables; i++) {
        _renderers[i] = new FoliageRenderer(scriptables[i], chunkPos, chunkSize, cameraPos);
      }
    }


    public void Render(Vector2 cameraPosition, Camera camera) {
      if (camera.WorldToScreenPoint(new Vector3(_chunkCenter.x, camera.transform.position.y, _chunkCenter.y)).z < -_chunkSize * 1.5f) return;
      foreach (var renderer in _renderers) renderer.Render(cameraPosition);
    }


    public void CleanUp() {
      foreach (var renderer in _renderers) renderer.CleanUp();
    }
  }
}