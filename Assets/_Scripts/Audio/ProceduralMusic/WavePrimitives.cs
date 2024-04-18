using UnityEngine;

public static class WavePrimitives
{

  public static float sineWave(double phase)
  {
    return Mathf.Sin((float) phase * Mathf.PI * 2);
  }

  public static float squareWave(double phase)
  {
    return phase < 0.5 ? 1 : -1;
  }

  public static float sawWave(double phase)
  {
    return (float) phase * 2 - 1;
  }
}