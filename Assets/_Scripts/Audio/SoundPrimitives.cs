using UnityEngine;

public static class SoundPrimitives
{

  public static float chordSynth(double phase, float[] frequencies, float[] amplitudes)
  {
    float value = 0;

    for (int i = 0; i < frequencies.Length; i++)
    {
      value += WavePrimitives.sawWave(phase * frequencies[i]) * amplitudes[i];
    }

    return value;
  }

  public static float kickDrum(double phase, float amplitude, float envelope, float attackBias, double beatPhase)
  {
    float value = WavePrimitives.sineWave(phase) * amplitude;
    value = Mathf.Lerp(0, value, (float) beatPhase * envelope * attackBias);
    value = Mathf.Lerp(value, 0, Mathf.Pow((float) beatPhase, envelope));
    return value;
  }

  public static float leadSynth(double phase, float amplitude, float envelope, double beatPhase) {
    float value = WavePrimitives.sawWave(phase) * amplitude;
    value = Mathf.Lerp(0, value, (float) beatPhase * envelope);
    value = Mathf.Lerp(value, 0, Mathf.Pow((float) beatPhase, envelope));
    return value;
  }
}