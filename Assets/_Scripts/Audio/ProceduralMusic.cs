using System;
using UnityEngine;
using Unity.Mathematics;

public class ProceduralMusic : MonoBehaviour
{

  private struct Instrument {

    public int type;
    // Instrument types:
    // 0: Kick drum/808
    // 1: Synth lead
    // 2: Synth chord
    // 3: Synth bass
    // 4: Hi-hat closed
    public float volume;
    public float frequency;
    public int subdivision;
    // 0 = 1/16th note, 1 = 1/18th, etc
    public double phase; 

    public Instrument(int type) {
      this.type = type;
      volume = 0;
      frequency = 440;
      phase = 0;
      subdivision = 0;
    }

  }

  private int _sampleRate;
  private bool[] _beat = new bool[9];
  private double[] _beatPhases = new double[9];

  private Instrument[] _instruments = {
    new Instrument(0),
    new Instrument(1),
    new Instrument(2),
    new Instrument(3),
    new Instrument(4)
  };

  private int _tempo;
  private int _currentScale;
  private int _currentKey;
  // 0 is C, increasing by half-steps

  private void Awake() {
    _sampleRate = AudioSettings.outputSampleRate;
    SetupInstruments();
  }

  private void OnAudioFilterRead(float[] data, int channels) {
    for (int sample = 0; sample < data.Length; sample += channels) {
      float value = 0;

      for (int beatSub = 0; beatSub < _beat.Length; beatSub++) {
        if (_beat[beatSub]) {
          // Do something on subdivisions here
          if (beatSub == 2) {
            int note =
                MusicConstants.scales[_currentScale][
                  (int)((noise.snoise(new float2(sample * 2f, 0)) + 1) / 2 * MusicConstants.scales[_currentScale].Length)] + _currentKey;
            note %= 11;
            _instruments[1].frequency = MusicConstants.notes[note];
          }
          _beat[beatSub] = false;
        }
      }

      // Add to value in here

      for (int instrument = 0; instrument < _instruments.Length; instrument++) {
        switch (_instruments[instrument].type) {
          case 0:
            // Kick drum
            value += SoundPrimitives.kickDrum(_instruments[instrument].phase,
              _instruments[instrument].volume, 1, 20, _beatPhases[_instruments[instrument].subdivision]);
            break;
          case 1:
            // Synth lead
            value += SoundPrimitives.leadSynth(_instruments[instrument].phase,
              _instruments[instrument].volume, 25, _beatPhases[_instruments[instrument].subdivision]);
            break;
          case 2:
            // Synth chord
            break;
          case 3:
            // Synth bass
            break;
          case 4:
            // Hi-hat closed
            break;
        }
        _instruments[instrument].phase = (_instruments[instrument].phase + 1d / _sampleRate * _instruments[instrument].frequency) % 1;
      }

      // Advance phase for all subdivisions
      for (int beatSub = 0; beatSub < _beatPhases.Length; beatSub++) {
        _beatPhases[beatSub] += 1d / _sampleRate * _tempo / 60d / Mathf.Pow(2, beatSub - 2);
        if (_beatPhases[beatSub] >= 1) {
          _beatPhases[beatSub] %= 1;
          _beat[beatSub] = true;
        }
      }

      // Write data to channels
      for (int channel = 0; channel < channels; channel++) {
        data[sample + channel] = value;
      }
    }
  }

  private void SetupInstruments() {
    _currentKey = (int) ((noise.snoise(new float2(DateTime.UtcNow.GetHashCode(), 0)) + 1) / 2 * MusicConstants.notes.Length);
    _currentScale = (int) ((noise.snoise(new float2(0, DateTime.UtcNow.GetHashCode())) + 1) / 2 * MusicConstants.scales.Length);
    _instruments[0].frequency = MusicConstants.notes[_currentKey] / 4;
    _instruments[0].volume = 0.5f;
    _instruments[0].subdivision = 2;

    _instruments[1].frequency = MusicConstants.notes[_currentKey];
    _instruments[1].volume = 0.1f;
    _instruments[1].subdivision = 1;
    
    _tempo = 120;
    
    Debug.Log("Set up instruments.");
  }

}