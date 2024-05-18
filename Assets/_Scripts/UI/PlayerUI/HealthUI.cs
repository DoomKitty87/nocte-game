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
  [SerializeField] private AnimationCurve _healthChangeCurve;
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
      _healthNumber.text = $"{Mathf.FloorToInt(Mathf.Lerp(initialHealth, currentHealth, _healthChangeCurve.Evaluate(time)))} <sup>/ {((int)maxHealth).ToString()}</sup>";
      _healthBar.fillAmount = Mathf.Lerp(initialHealth / maxHealth, currentHealth / maxHealth, _healthChangeCurve.Evaluate(time));
      time += Time.deltaTime;
      yield return null;
    }
    _healthNumber.text = $"{((int)currentHealth).ToString()} <sup>/ {((int)maxHealth).ToString()}</sup>";
    _healthBar.fillAmount = currentHealth / maxHealth;
    _changingHealth = false;
  }

  private void Start() {
    _playerHealth._onHealthChanged.AddListener(OnHealthChanged);
    _healthNumber.text = $"{((int)_playerHealth.CurrentHealth).ToString()} <sup>/ {((int)_playerHealth.MaxHealth).ToString()}</sup>";
    _healthBar.fillAmount = _playerHealth.CurrentHealth / _playerHealth.MaxHealth;
  }
}