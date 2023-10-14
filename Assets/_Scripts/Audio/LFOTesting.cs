using System;
using UnityEngine;

public class LFOTesting : MonoBehaviour
{

  [System.Serializable]
  private struct Instrument {
    
    public float frequency;
    public float phase;
    public int instrumentType;
    // Kick = 0
    // Synth Lead = 1
  }

  private double _beatPhase;
  private int _sampleRate;
  private bool _beat;
  private int _nextNote;

  private float[] _majorScale = {261.63f, 293.66f, 329.63f, 349.23f, 392f, 440f, 493.88f, 523.25f};
  [SerializeField] private Instrument[] _instruments;

  [SerializeField] private float _tempo = 120f;

  private void Awake() {
    _sampleRate = AudioSettings.outputSampleRate;
    Debug.Log(_sampleRate);
  }

  //TODO: Add multiple beat subdivisions being tracked because that's the only way I can think of to do that

  private void OnAudioFilterRead(float[] data, int channels) {
    for (int sample = 0; sample < data.Length; sample += channels) {
      float value = 0;
      if (_beat) {
        _tempo += 4;
        _instruments[1].frequency = _majorScale[_nextNote];
        _nextNote++;
        _nextNote %= _majorScale.Length;
        _beat = false;
      }

      for (int instrument = 0; instrument < _instruments.Length; instrument++) {
        switch (_instruments[instrument].instrumentType) {
          case 0:
            value += SoundPrimitives.kickDrum(_instruments[instrument].phase,
              0.5f, 1f, 20, _beatPhase);
            break;
          case 1:
            value += SoundPrimitives.leadSynth(_instruments[instrument].phase,
              0.5f, 4f, _beatPhase);
              break;
        }
        _instruments[instrument].phase = (_instruments[instrument].phase + 1f / _sampleRate * _instruments[instrument].frequency) % 1;
      }

      _beatPhase += 1f / _sampleRate * _tempo / 60;
      if (_beatPhase >= 1f) {
        _beatPhase %= 1;
        _beat = true;
      }
      for (int channel = 0; channel < channels; channel++) {
        data[sample + channel] = value;
      }
    }
  }
}