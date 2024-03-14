using System.Collections.Generic;
using UnityEngine;


namespace Foliage
{
  public class FoliageChunk
  {
    private readonly FoliageRenderer[] _renderers;

    public FoliageChunk(IReadOnlyList<FoliageScriptable> scriptables, Vector2Int chunkPos, float chunkSize, Vector2 cameraPos) {
      var numberOfScriptables = 0;
      _renderers = new FoliageRenderer[numberOfScriptables];


      for (var i = 0; i < numberOfScriptables; i++) {
        _renderers[i] = new FoliageRenderer(scriptables[i], chunkPos, chunkSize, cameraPos);
      }
    }


    public void Render(Vector2 cameraPosition) {
      foreach (var renderer in _renderers) renderer.Render(cameraPosition);
    }


    public void CleanUp() {
      foreach (var renderer in _renderers) renderer.CleanUp();
    }
  }
}