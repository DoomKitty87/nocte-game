using UnityEngine;

public class ProceduralMusic : MonoBehaviour
{

  private int _sampleRate;

  private void Awake() {
    _sampleRate = AudioSettings.outputSampleRate;
  }

  private void OnAudioFilterRead(float[] data, int channels) {
    for (int sample = 0; sample < data.Length; sample += channels) {
      float value = 0;

      for (int beatSub = 0; beatSub < _beat.Length; beatSub++) {
        if (_beat[beatSub]) {
          // Do something on subdivisions here

          _beat[beatSub] = false;
        }
      }

      // Add to value in here

      // Advance phase for all subdivisions
      for (int beatSub = 0; beatSub < _beatPhases.Length; beatSub++) {
        _beatPhases[beatSub] += 1f / _sampleRate * _tempo / 60 / Mathf.Pow(2, beatSub - 2);
        if (_beatPhases[beatSub] >= 1) {
          _beatPhases[beatSub] %= 1;
          _beatPhases[beatSub] = true;
        }
      }

      // Write data to channels
      for (int channel = 0; channel < channels; channel++) {
        data[sample + channel] = value;
      }
    }
  }
}