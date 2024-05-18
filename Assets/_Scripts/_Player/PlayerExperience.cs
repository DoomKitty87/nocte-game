using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerExperience : MonoBehaviour
{
  
  public static PlayerExperience Instance { get; private set; }

  [SerializeField] private UnityEvent OnGainExperience;

  private float _experience = 0;

  private void Start() {
    if (Instance == null) Instance = this;
  }

  private void OnDisable() {
    if (Instance == this) Instance = null;
  }

  public float CheckExperience() {
    return _experience;
  }

  public void GainExperience(float experience) {
    _experience += experience;
    OnGainExperience.Invoke();
    Debug.Log("Gained Experience " + experience);
  }
  
}