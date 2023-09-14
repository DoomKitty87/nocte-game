using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnHealthInitializeEvent : UnityEvent<float> {}
[System.Serializable]
public class OnHealthChangedEvent : UnityEvent<float, float, float> {}
public class HealthInterface : MonoBehaviour
{

  public float CurrentHealth {
    get {
      if (_currentHealth <= 0) {
        return 0;
      }
      return _currentHealth;
    }
  }
  public float MaxHealth => _maxHealth;
  
  [SerializeField] private float _currentHealth;
  [SerializeField] private float _maxHealth;
  [Tooltip("OnHealthInitialize(_maxHealth)")] public OnHealthInitializeEvent _onHealthInitialize;
  [Tooltip("OnHealthZero()")] public UnityEvent _onHealthZero;
  [Tooltip("OnHealthChanged(healthBeforeDamage, _currentHealth, _maxHealth) Doesn't fire when health changes to a value below zero.")] public OnHealthChangedEvent _onHealthChanged;
  
  
  private void Start() {
    _currentHealth = _maxHealth;
    _onHealthInitialize?.Invoke(_maxHealth);
  }

  public void Heal(float healPoints) {
    float initialHealth = _currentHealth;
    if (_currentHealth + healPoints >= _maxHealth) _currentHealth = _maxHealth;
    else _currentHealth += healPoints;
    _onHealthChanged?.Invoke(initialHealth, _currentHealth, _maxHealth);
  }

  public void Damage(float damagePoints) {
    if (_currentHealth - damagePoints <= 0) {
      _onHealthZero?.Invoke();
    }
    else {
      float initialHealth = _currentHealth;
      _currentHealth -= damagePoints;
      _onHealthChanged?.Invoke(initialHealth, _currentHealth, _maxHealth);
    }
  }
}