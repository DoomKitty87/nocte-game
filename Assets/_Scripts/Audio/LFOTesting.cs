using System;
using Unity.Mathematics;
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

  private double[] _beatPhases = new double[7];
  // 1 Beat, 2 Beat, 4 Beat, 2 Bar, 4 Bar, 8 Bar, 16 Bar
  private double[] _subDPhases = new double[2];
  // 8th, 16th
  private int _sampleRate;
  private bool[] _beat = new bool[7];
  private bool[] _subBeat = new bool[2];
  private int _currentNote = 0;

  private float[] _majorScale = {261.63f, 293.66f, 329.63f, 349.23f, 392f, 440f, 493.88f, 523.25f};
  private int[] _changeOptions = {0, 3, 5, 7, -3, -5, -7};
  [SerializeField] private Instrument[] _instruments;

  [SerializeField] private float _tempo = 120f;
  [SerializeField] private float _chorusVariance = 4f;

  private void Awake() {
    _sampleRate = AudioSettings.outputSampleRate;
    // Debug.Log(_sampleRate);
  }

  //TODO: Add multiple beat subdivisions being tracked because that's the only way I can think of to do that

  private void OnAudioFilterRead(float[] data, int channels) {
    for (int sample = 0; sample < data.Length; sample += channels) {
      float value = 0;
      if (_beat[0]) {
        _beat[0] = false;
      }
      if (_subBeat[0]) {
        Debug.Log(_currentNote);
        _instruments[1].frequency = _majorScale[_currentNote];
        _instruments[2].frequency = _majorScale[_currentNote] + _chorusVariance;
        _instruments[3].frequency = _majorScale[_currentNote] - _chorusVariance;
        _currentNote += _changeOptions[Mathf.FloorToInt((noise.snoise(new float2(sample, sample)) + 1) / 2 * _changeOptions.Length)];
        if (_currentNote >= _majorScale.Length) {
          _currentNote %= _majorScale.Length;
        } else if (_currentNote < 0) {
          _currentNote += _majorScale.Length;
        }
        _subBeat[0] = false;
      }

      for (int instrument = 0; instrument < _instruments.Length; instrument++) {
        switch (_instruments[instrument].instrumentType) {
          case 0:
            value += SoundPrimitives.kickDrum(_instruments[instrument].phase,
              0.5f, 1, 20, _beatPhases[0]);
            break;
          case 1:
            value += SoundPrimitives.leadSynth(_instruments[instrument].phase,
              0.1f, 25f, _beatPhases[2]);
              break;
        }
        _instruments[instrument].phase = (_instruments[instrument].phase + 1f / _sampleRate * _instruments[instrument].frequency) % 1;
      }

      for (int i = 0; i < _beatPhases.Length; i++) {
        _beatPhases[i] += 1f / _sampleRate * _tempo / 60 / Mathf.Pow(2, i);
        if (_beatPhases[i] >= 1) {
          _beatPhases[i] %= 1;
          _beat[i] = true;
        }
      }
      for (int i = 0; i < _subDPhases.Length; i++) {
        _subDPhases[i] += 1f / _sampleRate * _tempo / 60 / Mathf.Pow(2, -i - 1);
        if (_subDPhases[i] >= 1) {
          _subDPhases[i] %= 1;
          _subBeat[i] = true;
        }
      }
      
      for (int channel = 0; channel < channels; channel++) {
        data[sample + channel] = value;
      }
    }
  }
}