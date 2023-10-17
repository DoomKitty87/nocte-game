using UnityEngine;

public class AudioTest : MonoBehaviour
{

  [SerializeField, Range(0, 1)] private float _amplitude = 0.5f;
  [SerializeField] private float _frequency = 261.62f;

  private double _phase;
  private int _sampleRate;

  private void Awake()
  {
    _sampleRate = AudioSettings.outputSampleRate;
  }

  private void OnAudioFilterRead(float[] data, int channels) { 
    double phaseIncrement = _frequency / _sampleRate;

    for (int sample = 0; sample < data.Length; sample += channels) {
      float value = WavePrimitives.sineWave(_phase) * _amplitude;
      _phase = (_phase + phaseIncrement) % 1;

      for (int channel = 0; channel < channels; channel++) {
        data[sample + channel] = value;
      }
    }
  }
}