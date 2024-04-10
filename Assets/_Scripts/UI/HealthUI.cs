using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class HealthUI : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _healthNumber;
  [SerializeField] private Image _healthBar;
  [SerializeField] private HealthInterface _playerHealth;
  [SerializeField] private float _healthAnimateTime;
  private bool _changingHealth;

  public void OnHealthChanged(float initialHealth, float currentHealth, float maxHealth) {
    if (_changingHealth) StopCoroutine("AnimateHealthChange");
    StartCoroutine(AnimateHealthChange(initialHealth, currentHealth, maxHealth));
  }
  // Note: this will skip animation if more damage is received - I don't think that's a problem?

  private IEnumerator AnimateHealthChange(float initialHealth, float currentHealth, float maxHealth) {
    _changingHealth = true;
    float time = 0;
    while (time < _healthAnimateTime) {
      //_healthNumber.text = Mathf.SmoothStep(initialHealth, currentHealth, time / _healthAnimateTime).ToString();
      _healthBar.fillAmount = Mathf.SmoothStep(initialHealth / maxHealth, currentHealth / maxHealth, time / _healthAnimateTime);
      time += Time.deltaTime;
      yield return null;
    }
    _healthNumber.text = $"{currentHealth} HP";
    _healthBar.fillAmount = currentHealth / maxHealth;
    _changingHealth = false;
  }

  private void Start() {
    _playerHealth._onHealthChanged.AddListener(OnHealthChanged);
    _healthNumber.text = $"{_playerHealth.CurrentHealth} HP";
    _healthBar.fillAmount = _playerHealth.CurrentHealth / _playerHealth.MaxHealth;
  }
}