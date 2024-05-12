using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
  [SerializeField] private PlayerCombatCore _playerCombatCore;
  [SerializeField] private TextMeshProUGUI _ammoText;
  [SerializeField] private Image _ammoFill;
  [SerializeField] private float _fillAnimateTime = 0.3f;
  
  private float _lastAmmo;
  private bool _changingAmmo;
  
  private void Start() {
    _playerCombatCore.AmmoChanged += UpdateAmmoCount;
  }

  public void UpdateAmmoCount(int currentAmmo, int maxAmmo) {
    _ammoText.text = $"{currentAmmo} / {maxAmmo}";
    _ammoFill.fillAmount = currentAmmo / maxAmmo;
    StopCoroutine(AnimateAmmoChange(_lastAmmo, currentAmmo, maxAmmo));
    StartCoroutine(AnimateAmmoChange(_lastAmmo, currentAmmo, maxAmmo));
    _lastAmmo = currentAmmo;
  }
  
  private IEnumerator AnimateAmmoChange(float initialAmmo, float currentAmmo, float maxAmmo) {
    _changingAmmo = true;
    float time = 0;
    while (time < _fillAnimateTime) {
      //_healthNumber.text = Mathf.SmoothStep(initialAmmo, currentAmmo, time / _fillAnimateTime).ToString();
      _ammoFill.fillAmount = Mathf.SmoothStep(initialAmmo / maxAmmo, currentAmmo / maxAmmo, time / _fillAnimateTime);
      time += Time.deltaTime;
      yield return null;
    }
    _ammoText.text = $"{Mathf.FloorToInt(currentAmmo)} / {maxAmmo}";
    _ammoFill.fillAmount = currentAmmo / maxAmmo;
    _changingAmmo = false;
  }
  
  private void OnDisable() {
    _playerCombatCore.AmmoChanged -= UpdateAmmoCount;
  }
}