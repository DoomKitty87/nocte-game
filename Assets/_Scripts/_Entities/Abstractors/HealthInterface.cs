using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnHealthInitializeEvent : UnityEvent<float> {}
[Serializable]
public class OnHealthChangedEvent : UnityEvent<float, float, float> {}
[Serializable]
public class OnDamageEvent : UnityEvent<Vector3> {}
[Serializable]
public class OnHealEvent : UnityEvent {}
[Serializable]
public class OnHealthZeroEvent : UnityEvent {}
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
  [Tooltip("OnHealthZero()")] public OnHealthZeroEvent _onHealthZero;
  [Tooltip("OnHealthChanged(healthBeforeDamage, _currentHealth, _maxHealth) Doesn't fire when health changes to a value below zero.")] public OnHealthChangedEvent _onHealthChanged;
  [Tooltip("OnDamage(hitPosition)")] public OnDamageEvent _onDamage;
  [Tooltip("OnHeal()")] public OnHealEvent _onHeal;

  [SerializeField] private bool _isPlayer = false;

  [SerializeField] private float _regenRate = 0;
  [SerializeField] private float _regenDelay = 0;

  private float _regenTimer = 0;

  private void OnValidate() {
    _currentHealth = _maxHealth;
  }

  private void Update() {
    if (!_isPlayer) return;
    _regenTimer += Time.deltaTime;
    if (_currentHealth <= 0) {
      _onHealthZero?.Invoke();
    }
    if (_currentHealth < GetMaxHealth() && _regenRate > 0 && _regenTimer >= _regenDelay) {
      float regenMultiplier = 1;
      if (!UpgradeInfo.GetUpgrade(("Heath Regen")).isLocked) {
        regenMultiplier = UpgradeInfo.GetUpgrade("Health Regen").value + 0.5f;
      }
      Heal(_regenRate * regenMultiplier * Time.deltaTime);
    }
  }

  private float GetMaxHealth() {
    if (!_isPlayer && UpgradeInfo.GetUpgrade("Max Health").isLocked) {
      float healthMultiplier = UpgradeInfo.GetUpgrade("Max Health").value + 1;
      return _maxHealth * healthMultiplier;
    } else {
      return _maxHealth;
    }
  }

  private void Start() {
    _currentHealth = GetMaxHealth();
    _onHealthInitialize?.Invoke(GetMaxHealth());
  }

  public void Heal(float healPoints) {
    _onHeal?.Invoke();
    float initialHealth = _currentHealth;
    if (_currentHealth + healPoints >= GetMaxHealth()) _currentHealth = GetMaxHealth();
    else _currentHealth += healPoints;
    _onHealthChanged?.Invoke(initialHealth, _currentHealth, GetMaxHealth());
  }

  public void Damage(float damagePoints, Vector3 hitPosition) {
    _onDamage?.Invoke(hitPosition);
    
    if (_isPlayer && BackgroundInfo._godMode) return;
    
    if (_currentHealth - damagePoints <= 0) {
      _onHealthZero?.Invoke();
    }
    else {
      float initialHealth = _currentHealth;
      _currentHealth -= damagePoints;
      _regenTimer = 0;
      _onHealthChanged?.Invoke(initialHealth, _currentHealth, GetMaxHealth());
    }
  }
}