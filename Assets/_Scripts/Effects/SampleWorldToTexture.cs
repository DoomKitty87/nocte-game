using UnityEngine;

public class SampleWorldToTexture
{

  public static Texture2D SampleWorld(float scale, int resolution) {
    Texture2D tex = new Texture2D(resolution, resolution);

    float[] data = new float[resolution * resolution];
    for (int x = 0; x < resolution; x++) {
      for (int y = 0; y < resolution; y++) {
        data[x * resolution + y] = WorldGenInfo._worldGenerator.GetHeightValue(new Vector2(x, y) * scale);
      }
    }

    tex.SetPixelData(data, 0);

    return tex;
  }
}