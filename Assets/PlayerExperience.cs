using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerExperience : MonoBehaviour
{
  
  public static PlayerExperience Instance { get; private set; }

  public UnityEvent OnGainExperience;

  private float _experience = 0;

  private float _difficulty = 3;

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
    _experience += experience * (1 / (0.6f * (MathF.Pow(_difficulty, 0.8f)))) ;
    if (_experience >= 100) {
			_experience -= 100;
			UpgradeInfo.AddXPLevel();
			_difficulty++;
    }
		OnGainExperience.Invoke();
	}
}